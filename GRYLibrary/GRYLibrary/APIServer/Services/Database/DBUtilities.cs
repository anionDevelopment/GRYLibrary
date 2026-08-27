using GRYLibrary.Core.Logging.GRYLogger;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

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
                //The exclusive access is held for the entire transaction, so no other user of the database can interfere with it and the connection can not be
                //replaced while the transaction is running.
                interactor.GetGenericDatabaseInteractor().UseConnection(connection => RunTransactionCore(nameOfAction, log, runTransactional, connection, functions, results));
            });
            return [.. results];
        }

        private static void RunTransactionCore<T>(string nameOfAction, IGRYLog log, bool runTransactional, DbConnection connection, Func<DbCommand, T?>[] functions, List<T?> results)
        {
            foreach (Func<DbCommand, T?> function in functions)
            {
                //Every function gets its own transaction, so whether it is committed must be decided per function and not once for all of them.
                bool commit = true;
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
                    try
                    {
                        if (runTransactional)
                        {
                            CompleteTransaction(nameOfAction, log, transaction!, commit);
                        }
                    }
                    finally
                    {
                        transaction?.Dispose();//also required when completing the transaction failed
                    }
                }
            }
        }

        /// <summary>Completes <paramref name="transaction"/> by committing or rolling it back, depending on <paramref name="commit"/>.</summary>
        private static void CompleteTransaction(string nameOfAction, IGRYLog log, DbTransaction transaction, bool commit)
        {
            if (commit)
            {
                log.Log("Commit DB-transaction " + nameOfAction, Microsoft.Extensions.Logging.LogLevel.Trace);
                try
                {
                    transaction.Commit();
                }
                catch (Exception exception)
                {
                    //A failed commit is not the same as a failed statement: the database may have applied the transaction anyway and only the answer did not
                    //reach the application, for example when the connection ran into a timeout while waiting for it. The caller therefore must not assume that
                    //nothing happened, which is why this is stated explicitly instead of only reporting a connection-error.
                    log.Log($"Commit of DB-transaction {nameOfAction} failed. It is undetermined whether the database applied this transaction or not.", exception);
                    throw;
                }
            }
            else
            {
                log.Log("Rollback DB-transaction " + nameOfAction, Microsoft.Extensions.Logging.LogLevel.Trace);
                try
                {
                    transaction.Rollback();
                }
                catch (Exception exception)
                {
                    //A rollback is only done while the exception which caused it is on its way to the caller. Letting the rollback-exception out would replace
                    //that exception and therefore hide the actual reason of the failure, so it is only reported here.
                    log.Log($"Rollback of DB-transaction {nameOfAction} failed.", exception);
                }
            }
        }

        /// <summary>
        /// Reads the current schema (tables, columns, foreign keys) of an already-migrated database and writes it
        /// as a PlantUML entity-relationship diagram to <paramref name="targetFile"/>.
        /// </summary>
        /// <remarks>
        /// This is deliberately based on reading the schema back from the database itself instead of on static
        /// analysis of migration-scripts: that is what guarantees the diagram matches what the migrations actually
        /// produced. Currently only MariaDB/MySQL-flavored <c>information_schema</c> queries are implemented.
        /// </remarks>
        /// <typeparam name="ProjectSpecificDatabaseInteractor">The project-specific database-interactor type.</typeparam>
        /// <param name="database">The database-connection to read the schema from. Its migrations must already have been run.</param>
        /// <param name="log">The logger used while querying the database.</param>
        /// <param name="title">The title shown at the top of the generated diagram.</param>
        /// <param name="targetFile">The file the generated PlantUML diagram is written to.</param>
        /// <returns>The generated PlantUML diagram content.</returns>
        public static string GenerateDatabaseStructurePlantUmlDiagram<ProjectSpecificDatabaseInteractor>(ProjectSpecificDatabaseInteractor database, IGRYLog log, string title, string targetFile)
            where ProjectSpecificDatabaseInteractor : IProjectSpecificDatabaseInteractor
        {
            const string migrationBookkeepingTableName = "GRYMigrationInformation";

            IList<DatabaseStructureColumnInformation> columns = RunTransaction<IList<DatabaseStructureColumnInformation>, ProjectSpecificDatabaseInteractor>(nameof(GenerateDatabaseStructurePlantUmlDiagram), log, database, true, (command) =>
            {
                IList<DatabaseStructureColumnInformation> result = new List<DatabaseStructureColumnInformation>();
                command.CommandText = "SELECT TABLE_NAME, COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE, COLUMN_KEY FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME <> @MigrationTableName ORDER BY TABLE_NAME, ORDINAL_POSITION;";
                command.Parameters.Add(database.GetGenericDatabaseInteractor().GetParameter("MigrationTableName", migrationBookkeepingTableName));
                using DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new DatabaseStructureColumnInformation(
                        tableName: reader.GetString(0),
                        columnName: reader.GetString(1),
                        columnType: reader.GetString(2),
                        isNullable: reader.GetString(3) == "YES",
                        isPrimaryKey: reader.GetString(4) == "PRI"));
                }
                return result;
            })[0]!;

            IList<DatabaseStructureForeignKeyInformation> foreignKeys = RunTransaction<IList<DatabaseStructureForeignKeyInformation>, ProjectSpecificDatabaseInteractor>(nameof(GenerateDatabaseStructurePlantUmlDiagram), log, database, true, (command) =>
            {
                IList<DatabaseStructureForeignKeyInformation> result = new List<DatabaseStructureForeignKeyInformation>();
                command.CommandText = "SELECT TABLE_NAME, COLUMN_NAME, REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME FROM information_schema.KEY_COLUMN_USAGE WHERE TABLE_SCHEMA = DATABASE() AND REFERENCED_TABLE_NAME IS NOT NULL ORDER BY TABLE_NAME, COLUMN_NAME;";
                using DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new DatabaseStructureForeignKeyInformation(
                        tableName: reader.GetString(0),
                        columnName: reader.GetString(1),
                        referencedTableName: reader.GetString(2),
                        referencedColumnName: reader.GetString(3)));
                }
                return result;
            })[0]!;

            string plantUml = GenerateDatabaseStructurePlantUml(title, columns, foreignKeys);
            File.WriteAllText(targetFile, plantUml, new UTF8Encoding(false));
            return plantUml;
        }

        private static string GenerateDatabaseStructurePlantUml(string title, IList<DatabaseStructureColumnInformation> columns, IList<DatabaseStructureForeignKeyInformation> foreignKeys)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("@startuml");
            builder.AppendLine($"title {title}");
            builder.AppendLine("hide circle");
            builder.AppendLine();

            foreach (IGrouping<string, DatabaseStructureColumnInformation> table in columns.GroupBy(c => c.TableName))
            {
                builder.AppendLine($"entity \"{table.Key}\" {{");
                IList<DatabaseStructureColumnInformation> primaryKeyColumns = table.Where(c => c.IsPrimaryKey).ToList();
                IList<DatabaseStructureColumnInformation> otherColumns = table.Where(c => !c.IsPrimaryKey).ToList();
                foreach (DatabaseStructureColumnInformation column in primaryKeyColumns)
                {
                    builder.AppendLine($"  * {column.ColumnName} : {column.ColumnType} <<PK>>");
                }
                if (primaryKeyColumns.Count > 0 && otherColumns.Count > 0)
                {
                    builder.AppendLine("  --");
                }
                foreach (DatabaseStructureColumnInformation column in otherColumns)
                {
                    string nullability = column.IsNullable ? "" : " <<NOT NULL>>";
                    builder.AppendLine($"  {column.ColumnName} : {column.ColumnType}{nullability}");
                }
                builder.AppendLine("}");
                builder.AppendLine();
            }

            foreach (DatabaseStructureForeignKeyInformation foreignKey in foreignKeys)
            {
                builder.AppendLine($"\"{foreignKey.TableName}\" }}o--|| \"{foreignKey.ReferencedTableName}\" : {foreignKey.ColumnName}");
            }

            builder.AppendLine("@enduml");
            return builder.ToString();
        }

        private sealed class DatabaseStructureColumnInformation
        {
            public string TableName { get; }
            public string ColumnName { get; }
            public string ColumnType { get; }
            public bool IsNullable { get; }
            public bool IsPrimaryKey { get; }

            public DatabaseStructureColumnInformation(string tableName, string columnName, string columnType, bool isNullable, bool isPrimaryKey)
            {
                this.TableName = tableName;
                this.ColumnName = columnName;
                this.ColumnType = columnType;
                this.IsNullable = isNullable;
                this.IsPrimaryKey = isPrimaryKey;
            }
        }

        private sealed class DatabaseStructureForeignKeyInformation
        {
            public string TableName { get; }
            public string ColumnName { get; }
            public string ReferencedTableName { get; }
            public string ReferencedColumnName { get; }

            public DatabaseStructureForeignKeyInformation(string tableName, string columnName, string referencedTableName, string referencedColumnName)
            {
                this.TableName = tableName;
                this.ColumnName = columnName;
                this.ReferencedTableName = referencedTableName;
                this.ReferencedColumnName = referencedColumnName;
            }
        }
    }
}
