using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc.Migration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.APIServer.Services.Database
{
    public abstract class GenericDatabaseInteractor : IGenericDatabaseInteractor
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        private readonly IDatabasePersistenceConfiguration _Configuration;

        /// <summary>
        /// Protects the connection for the entire duration of its usage. There is exactly one connection, so whoever needs the database has to wait until the
        /// current user is finished with it. Without that the connection-thread could replace or dispose a connection which somebody is still working with.
        /// </summary>
        private readonly Misc.ExclusiveAccess _ConnectionAccess = new Misc.ExclusiveAccess();

        /// <summary>Protects the fields of this object. Is only taken for short operations which never access the database themself.</summary>
        private readonly object _StateLock = new object();

        private DbConnection? _Connection = null;
        private Exception? _LastConnectionException = null;
        private readonly Thread _ConnectionThread;
        private bool _ThreadEnabled = true;
        private bool _ThreadRunning = true;//is already true before the thread is started, so that disposing directly after the construction waits correctly.
        private bool _LogConnectionErrors = false;
        private bool _IsDisposed = false;

        public IGRYLog Log { get; private set; }

        public GenericDatabaseInteractor(IDatabasePersistenceConfiguration configuration, IServerLog log) : this(configuration, log.Logger)
        {
        }
        public GenericDatabaseInteractor(IDatabasePersistenceConfiguration configuration, IGRYLog log)
        {
            this.Log = log;
            this._Configuration = configuration;
            this._ConnectionThread = new Thread(this.StartTryToConnectScheduler);
            this._ConnectionThread.Start();
        }

        protected abstract DbConnection CreateNewConnectionObject(string connectionString);
        public abstract DbCommand CreateCommand(string sql);
        public abstract string EscapePasswordInConnectionString(string connectionString);
        public abstract string CreateSQLStatementForGetAllTableNames();
        public abstract string CreateSQLStatementForCreatingMigrationMaintenanceTableIfNotExist(string tableName);
        public abstract string GetSQLStatementForSelectMigrationMaintenanceTableContent(string migrationTableName);
        public abstract string GetSQLStatementForRunningMigration(string migrationContent, string migrationTableName, string migrationName, DateTimeOffset now);
        public abstract void Accept(IGenericDatabaseInteractorVisitor visitor);
        public abstract T Accept<T>(IGenericDatabaseInteractorVisitor<T> visitor);

        public abstract DbParameter GetParameter(string parameterName, object? value, Type type);

        public DbParameter GetParameter(string parameterName, object value)
        {
            GUtilities.AssertCondition(value != null, $"value for parameter {parameterName} is null, so a speicfic type for it must be set.");
            return this.GetParameter(parameterName, value, value!.GetType());
        }

        #region Connection-state

        private bool ThreadEnabled
        {
            get { lock (this._StateLock) { return this._ThreadEnabled; } }
            set { lock (this._StateLock) { this._ThreadEnabled = value; } }
        }

        private bool ThreadRunning
        {
            get { lock (this._StateLock) { return this._ThreadRunning; } }
            set { lock (this._StateLock) { this._ThreadRunning = value; } }
        }

        private bool LogConnectionErrors
        {
            get { lock (this._StateLock) { return this._LogConnectionErrors; } }
            set { lock (this._StateLock) { this._LogConnectionErrors = value; } }
        }

        private Exception? LastConnectionException
        {
            get { lock (this._StateLock) { return this._LastConnectionException; } }
            set { lock (this._StateLock) { this._LastConnectionException = value; } }
        }

        private DbConnection? CurrentConnection
        {
            get { lock (this._StateLock) { return this._Connection; } }
            set { lock (this._StateLock) { this._Connection = value; } }
        }

        #endregion

        private void StartTryToConnectScheduler()
        {
            try
            {
                while (this.ThreadEnabled)
                {
                    try
                    {
                        this._ConnectionAccess.Run(this.ConnectIfNotConnected);
                        Thread.Sleep(TimeSpan.FromSeconds(5));//connected. wait some seconds and before checking again if the database is still available.
                    }
                    catch (Exception exception)
                    {
                        this.LastConnectionException = exception;
                        Thread.Sleep(TimeSpan.FromSeconds(2));//not connected. wait a few seconds until checking again if the database is avbailable.
                    }
                }
                this._ConnectionAccess.Run(this.DisposeConnection);
            }
            finally
            {
                this.ThreadRunning = false;
            }
        }

        /// <remarks>Must only be called while the connection-access is taken.</remarks>
        private void ConnectIfNotConnected()
        {
            if (this.IsConnected(out _))
            {
                return;
            }
            try
            {
                this.DisposeConnection();
                DbConnection connection = this.CreateConnection();
                this.CurrentConnection = connection;
                this.Log.Log("Database connected.");
            }
            catch (Exception exception)
            {
                if (this.LogConnectionErrors)
                {
                    this.Log.Log("Error while connecting to database.", exception);
                }
                throw;
            }
        }

        /// <remarks>Must only be called while the connection-access is taken.</remarks>
        private void DisposeConnection()
        {
            DbConnection? connection = this.CurrentConnection;
            this.CurrentConnection = null;
            connection?.Dispose();
        }

        private DbConnection CreateConnection()
        {
            string connectionStringForLog = this._Configuration.DatabaseConnectionString;
            if (this._Configuration.EscapePasswordInLog)
            {
                connectionStringForLog = this.EscapePassword(this._Configuration.DatabaseConnectionString);
            }
            GUtilities.AssertCondition(Regex.IsMatch(connectionStringForLog, @"Database=[^;]+", RegexOptions.IgnoreCase), $"Connectionstring \"{connectionStringForLog}\" does not contain a databasename-specification.");
            this.Log.Log($"Try to create database-connection using connection-string \"{connectionStringForLog}\".", LogLevel.Information);
            DbConnection conn = this.CreateNewConnectionObject(this._Configuration.DatabaseConnectionString);
            conn.Open();
            return conn;
        }

        private string EscapePassword(string databaseConnectionString)
        {
            string output = Regex.Replace(databaseConnectionString, @"Password=[^;]+", "Password=********");
            return output;
        }

        /// <inheritdoc/>
        public void UseConnection(Action<DbConnection> action)
        {
            this.UseConnection<object?>((connection) =>
            {
                action(connection);
                return null;
            });
        }

        /// <inheritdoc/>
        public T UseConnection<T>(Func<DbConnection, T> function)
        {
            return this._ConnectionAccess.Run(() => function(this.GetConnection()));
        }

        /// <inheritdoc/>
        public DbConnection GetConnection()
        {
            Exception? error;
            if (this.TryGetConnection(out DbConnection? connection, out error))
            {
                return connection!;
            }
            else
            {
                string message = "Database not available.";
                this.Log.Log(message, LogLevel.Warning);
                if (error == null)
                {
                    throw new DependencyNotAvailableException(message);
                }
                else
                {
                    throw new DependencyNotAvailableException(message, error);
                }
            }
        }

        /// <inheritdoc/>
        public bool TryGetConnection(out DbConnection? connection, out Exception? err)
        {
            try
            {
                Exception? connectedException;
                bool isConnected = this.IsConnected(out connectedException);
                if (isConnected)
                {
                    err = null;
                    connection = this.CurrentConnection;
                    return true;
                }
                else
                {
                    if (connectedException == null)
                    {
                        err = new DependencyNotAvailableException("Not connected");
                    }
                    else
                    {
                        err = new DependencyNotAvailableException("Not connected", connectedException);
                    }
                    connection = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                connection = null;
                err = e;
                return false;
            }
        }

        public bool IsConnected(out Exception? exception)
        {
            DbConnection? connection = this.CurrentConnection;
            if (connection == null)
            {
                exception = new DependencyNotAvailableException("Connection is null.", this.LastConnectionException);
                return false;
            }
            else
            {
                ConnectionState state = connection.State;
                bool result = state == ConnectionState.Open;
                if (result)
                {
                    exception = null;
                    return true;
                }
                else
                {
                    exception = new DependencyNotAvailableException($"Connection-state is \"{state}\".", this.LastConnectionException);
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public (bool, Exception?) IsAvailable()
        {
            try
            {
                return this._ConnectionAccess.Run<(bool, Exception?)>(() =>
                {
                    bool connected = this.IsConnected(out Exception? connectionExceptionN);
                    if (connected)
                    {
                        using DbCommand command = this.CreateCommand("select 1;");
                        using (DbDataReader reader = command.ExecuteReader())
                        {
                            GUtilities.AssertCondition(reader.HasRows, "Test-statement did not return any row. So database-connection is not ready.");
                            while (reader.Read())
                            {
                                GUtilities.NoOperation(); // Just to ensure that we can read from the reader without any exceptions
                            }
                        }
                        return (true, this.LastConnectionException);
                    }
                    else
                    {
                        Exception connectionException = GUtilities.AssertNotNull(connectionExceptionN, "Unknown connection-exception.");
                        return (false, connectionException);
                    }
                });
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetAllTableNames()
        {
            return this._ConnectionAccess.Run<IEnumerable<string>>(() =>
            {
                IList<string> result = [];
                string sql = this.CreateSQLStatementForGetAllTableNames();
                using DbCommand cmd = this.CreateCommand(sql);
                using DbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
                return result;
            });
        }

        public void Dispose()
        {
            lock (this._StateLock)
            {
                if (this._IsDisposed)
                {
                    return;
                }
                this._IsDisposed = true;
                this._ThreadEnabled = false;
            }
            //Attention: waiting for the connection-thread must not happen while a lock is held, because the connection-thread itself needs the state-lock and
            //the connection-access to finish its current iteration.
            GUtilities.WaitUntilConditionIsTrue(() => (!this.ThreadRunning, null), "Dispose database");
            this._ConnectionAccess.Dispose();
        }

        /// <inheritdoc/>
        public void SetLogConnectionAttemptErrors(bool enabled)
        {
            this.LogConnectionErrors = enabled;
        }

        /// <inheritdoc/>
        public void DoAllMigrations(IList<MigrationInstance> migrations, ITimeService timeService)
        {
            this._ConnectionAccess.Run(() =>
            {
                GRYMigrator migrator = new GRYMigrator(timeService, migrations, this);
                migrator.InitializeDatabaseAndMigrateIfRequired();
            });
        }

        internal bool IsDisposed()
        {
            lock (this._StateLock)
            {
                return this._IsDisposed;
            }
        }

        /// <inheritdoc/>
        public void WaitUntilAvailable(TimeSpan timeSpan)
        {
            GUtilities.WaitUntilConditionIsTrue(this.IsAvailable, timeSpan, "Database-initialization");
        }
    }

    public interface IGenericDatabaseInteractorVisitor
    {
        void Handle(MariaDBDatabaseInteractor mariaDBDatabaseInteractor);
        void Handle(OracleDatabaseInteractor oracleDatabaseInteractor);
        void Handle(SQLServerDatabaseInteractor sQLServerDatabaseInteractor);
        void Handle(PostgreSQLDatabaseInteractor postgreSQLDatabaseInteractor);
    }
    public interface IGenericDatabaseInteractorVisitor<T>
    {
        T Handle(MariaDBDatabaseInteractor mariaDBDatabaseInteractor);
        T Handle(OracleDatabaseInteractor oracleDatabaseInteractor);
        T Handle(SQLServerDatabaseInteractor sQLServerDatabaseInteractor);
        T Handle(PostgreSQLDatabaseInteractor postgreSQLDatabaseInteractor);
    }
}
