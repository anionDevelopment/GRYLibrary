using GRYLibrary.Core.Logging.GRYLogger;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace GRYLibrary.Core.APIServer.Services.Database
{
    public static class DBUtilities
    {
        public static readonly IDictionary<string, GenericDatabaseInteractor> DatabaseInteractors = new Dictionary<string, GenericDatabaseInteractor>();
        public static GenericDatabaseInteractor ToGenericDatabaseInteractor(IDatabasePersistenceConfiguration databasePersistenceConfiguration, IGRYLog log)
        {
            if (DatabaseInteractors.TryGetValue(databasePersistenceConfiguration.DatabaseType, out GenericDatabaseInteractor gdi))
            {
                if (gdi.IsDisposed())
                {
                    DatabaseInteractors.Remove(databasePersistenceConfiguration.DatabaseType);
                }
            }
            if (!DatabaseInteractors.ContainsKey(databasePersistenceConfiguration.DatabaseType))
            {
                DatabaseInteractors[databasePersistenceConfiguration.DatabaseType] = ToNewGenericDatabaseInteractor(databasePersistenceConfiguration, log);
            }
            return DatabaseInteractors[databasePersistenceConfiguration.DatabaseType];
        }
        public static GenericDatabaseInteractor ToNewGenericDatabaseInteractor(IDatabasePersistenceConfiguration databasePersistenceConfiguration, IGRYLog log)
        {
            return databasePersistenceConfiguration.DatabaseType switch
            {
                "MariaDB" => new MariaDBDatabaseInteractor(databasePersistenceConfiguration, log),
                "PostgreSQL" => new PostgreSQLDatabaseInteractor(databasePersistenceConfiguration, log),
                "Oracle" => new OracleDatabaseInteractor(databasePersistenceConfiguration, log),
                "SQLServer" => new SQLServerDatabaseInteractor(databasePersistenceConfiguration, log),
                _ => throw new NotSupportedException($"Database type {databasePersistenceConfiguration.DatabaseType} is not supported."),
            };
        }
        public static T? GetNullableValue<T>(DbDataReader reader, int parameterIndex)
        {
            if (reader.IsDBNull(parameterIndex))
            {
                return default(T);
            }
            else
            {
                return (T)reader.GetValue(parameterIndex);
            }
        }
        public static void AccessDatabase<ProjectSpecificDatabaseInteractor>(ProjectSpecificDatabaseInteractor database, Action<ProjectSpecificDatabaseInteractor> action)
            where ProjectSpecificDatabaseInteractor : IProjectSpecificDatabaseInteractor
        {
            AccessDatabase<object?, ProjectSpecificDatabaseInteractor>(database, (db) =>
            {
                action(db);
                return null;
            });
        }

        public static T AccessDatabase<T, ProjectSpecificDatabaseInteractor>(ProjectSpecificDatabaseInteractor database, Func<ProjectSpecificDatabaseInteractor, T> function)
            where ProjectSpecificDatabaseInteractor : IProjectSpecificDatabaseInteractor
        {
            return function(database);
        }

        public static void RunTransaction<ProjectSpecificDatabaseInteractor>(string nameOfAction, IGRYLog log, ProjectSpecificDatabaseInteractor database, bool runTransactional, params Action<DbCommand>[] actions)
            where ProjectSpecificDatabaseInteractor : IProjectSpecificDatabaseInteractor
        {
            RunTransaction<object, ProjectSpecificDatabaseInteractor>(nameOfAction, log, database, runTransactional, [.. actions.Select<Action<DbCommand>, Func<DbCommand, object?>>(action => (command) =>
            {
                action(command);
                return null;
            }
            )]);
        }

        public static T?[] RunTransaction<T, ProjectSpecificDatabaseInteractor>(string nameOfAction, IGRYLog log, ProjectSpecificDatabaseInteractor database, bool runTransactional, params Func<DbCommand, T?>[] functions)
            where ProjectSpecificDatabaseInteractor : IProjectSpecificDatabaseInteractor
        {
            List<T?> results = [];
            AccessDatabase(database, interactor =>
            {
                log.Log("Run DB-transaction " + nameOfAction, Microsoft.Extensions.Logging.LogLevel.Trace);
                DbConnection connection = interactor.GetGenericDatabaseInteractor().GetConnection();
                bool commit = true;
                foreach (Func<DbCommand, T?> function in functions)
                {
                    DbTransaction? transaction = null;
                    if (runTransactional)
                    {
                        transaction = connection.BeginTransaction();
                    }
                    try
                    {
                        using (DbCommand cmd = connection.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandTimeout = 300;
                            if (runTransactional)
                            {
                                cmd.Transaction = transaction;
                            }
                            try
                            {
                                T? result = function(cmd);
                                results.Add(result);
                            }
                            catch (Exception e)
                            {
                                commit = false;
                                log.Log($"Error in database occurred while doing DB-transaction {nameOfAction}.", e);
                                throw;
                            }
                        }
                    }
                    finally
                    {
                        if (commit)
                        {
                            log.Log("Commit DB-transaction " + nameOfAction, Microsoft.Extensions.Logging.LogLevel.Trace);
                            transaction.Commit();
                        }
                        else
                        {
                            log.Log("Rollback DB-transaction " + nameOfAction, Microsoft.Extensions.Logging.LogLevel.Trace);
                            transaction.Rollback();
                        }
                    }
                    if (runTransactional)
                    {
                        GRYLibrary.Core.Misc.Utilities.GetValue(transaction).Dispose();
                    }
                }
            });
            return [.. results];
        }
    }
}
