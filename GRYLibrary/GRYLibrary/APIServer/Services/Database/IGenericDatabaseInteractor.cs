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
        public DbConnection GetConnection();
        public bool TryGetConnection(out DbConnection? connection, out Exception? error);
        public (bool, Exception?) IsAvailable();
        public void Accept(IGenericDatabaseInteractorVisitor visitor);
        public T Accept<T>(IGenericDatabaseInteractorVisitor<T> visitor);
        public void SetLogConnectionAttemptErrors(bool enabled);
        public void WaitUntilAvailable(TimeSpan timeSpan);
    }
}
