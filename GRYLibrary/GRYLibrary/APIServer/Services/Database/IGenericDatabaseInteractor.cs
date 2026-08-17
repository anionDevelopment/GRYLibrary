using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc.Migration;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace GRYLibrary.Core.APIServer.Services.Database
{
    public interface IGenericDatabaseInteractor : IDisposable
    {
        public void DoAllMigrations(IList<MigrationInstance> migrations, ITimeService timeService);
        public IGRYLog Log { get; }
        public IEnumerable<string> GetAllTableNames();
        public DbCommand CreateCommand(string sql);
        public string EscapePasswordInConnectionString(string connectionString);
        public string CreateSQLStatementForGetAllTableNames();
        public string CreateSQLStatementForCreatingMigrationMaintenanceTableIfNotExist(string tableName);
        public string GetSQLStatementForSelectMigrationMaintenanceTableContent(string migrationTableName);
        public string GetSQLStatementForRunningMigration(string migrationContent, string migrationTableName, string migrationName, DateTimeOffset now);
        public DbParameter GetParameter(string parameterName, object? value, Type type);
        public DbParameter GetParameter(string parameterName, object value);
        /// <summary>Runs <paramref name="action"/> with exclusive access to the database-connection. Waits until the connection is not used by somebody else anymore.</summary>
        public void UseConnection(Action<DbConnection> action);

        /// <summary>Runs <paramref name="function"/> with exclusive access to the database-connection and returns its result. Waits until the connection is not used by somebody else anymore.</summary>
        public T UseConnection<T>(Func<DbConnection, T> function);

        /// <remarks>
        /// There is exactly one connection-object, and it is replaced whenever the connection has to be re-established. The returned object therefore only stays
        /// usable while the caller has the exclusive access to it, so this must only be called inside <see cref="UseConnection{T}(Func{DbConnection, T})"/> or
        /// inside <see cref="DoAllMigrations"/>, which takes the same access.
        /// </remarks>
        public DbConnection GetConnection();
        public bool TryGetConnection(out DbConnection? connection, out Exception? error);
        public (bool, Exception?) IsAvailable();
        public void Accept(IGenericDatabaseInteractorVisitor visitor);
        public T Accept<T>(IGenericDatabaseInteractorVisitor<T> visitor);
        public void SetLogConnectionAttemptErrors(bool enabled);
        public void WaitUntilAvailable(TimeSpan timeSpan);
    }
}
