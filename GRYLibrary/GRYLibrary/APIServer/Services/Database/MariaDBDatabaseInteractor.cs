using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.Logging.GRYLogger;
using MySqlConnector;
using System;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace GRYLibrary.Core.APIServer.Services.Database
{
    public class MariaDBDatabaseInteractor : GenericDatabaseInteractor
    {

        public MariaDBDatabaseInteractor(IDatabasePersistenceConfiguration configuration, IServerLog log) : this(configuration, log.Logger)
        {
        }
        public MariaDBDatabaseInteractor(IDatabasePersistenceConfiguration configuration, IGRYLog log) : base(configuration, log)
        {
        }

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        private static readonly Regex PasswordHideRegex = new Regex("(PWD|Pwd)=([^;]+)(;|$)");

#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        public override string EscapePasswordInConnectionString(string connectionString)
        {
            string replaceString = "********";
            connectionString = PasswordHideRegex.Replace(connectionString, match => $"{match.Groups[1]}={replaceString}{match.Groups[3]}");
            return connectionString;
        }

        public override DbCommand CreateCommand(string sql)
        {
            DbConnection connection = this.GetConnection();
            Misc.Utilities.AssertCondition(connection.State == System.Data.ConnectionState.Open, $"Connection of {connection.GetType().Name} is not open.");
            return new MySqlCommand(sql, (MySqlConnection)connection);
        }

        public override string CreateSQLStatementForCreatingMigrationMaintenanceTableIfNotExist(string migrationTableName)
        {
            return @$"create table if not exists {migrationTableName}(MigrationName varchar(255), ExecutionTimestamp datetime);";
        }

        public override string GetSQLStatementForRunningMigration(string migrationContent, string migrationTableName, string migrationName, DateTimeOffset now)
        {
            DateTimeOffset noUtc = now.ToUniversalTime();
            return @$"SET autocommit=0;
{migrationContent}
insert into {migrationTableName}(MigrationName, ExecutionTimestamp) values ('{migrationName}', '{noUtc:yyyy-MM-dd HH:mm:ss}')
";
        }

        public override string GetSQLStatementForSelectMigrationMaintenanceTableContent(string migrationTableName)
        {
            return $"select MigrationName, ExecutionTimestamp from {migrationTableName};";
        }

        public override string CreateSQLStatementForGetAllTableNames()
        {
            return $"show tables;";
        }

        public override void Accept(IGenericDatabaseInteractorVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IGenericDatabaseInteractorVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }

        protected override DbConnection CreateNewConnectionObject(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }
        public override DbParameter GetParameter(string parameterName, object? value, Type type)
        {
            return new MySqlParameter("@" + parameterName, this.FormatValue(value))
            {
                MySqlDbType = this.GetType(type)
            };
        }

        private object FormatValue(object? value)
        {
            object result;
            if (value == null)
            {
                result = DBNull.Value;
            }
            else if (value is DateTimeOffset typedValue)
            {
                result = typedValue.ToUniversalTime();
            }
            else
            {
                result = value;
            }
            return result;
        }

        private MySqlDbType GetType(Type type)
        {
            return type switch
            {
                var t when t == typeof(string) => MySqlDbType.VarChar,
                var t when t == typeof(int) => MySqlDbType.Int32,
                var t when t == typeof(long) => MySqlDbType.Int64,
                var t when t == typeof(short) => MySqlDbType.Int16,
                var t when t == typeof(bool) => MySqlDbType.Bit,
                var t when t == typeof(DateTime) => MySqlDbType.DateTime,
                var t when t == typeof(DateTimeOffset) => MySqlDbType.DateTime,
                var t when t == typeof(float) => MySqlDbType.Float,
                var t when t == typeof(double) => MySqlDbType.Double,
                var t when t == typeof(decimal) => MySqlDbType.Decimal,
                var t when t == typeof(Guid) => MySqlDbType.Guid,
                var t when t == typeof(sbyte) => MySqlDbType.Byte,
                var t when t == typeof(byte) => MySqlDbType.UByte,
                var t when t == typeof(byte[]) => MySqlDbType.VarBinary,
                var t when t == typeof(char) => MySqlDbType.VarChar,
                var t when t == typeof(TimeSpan) => MySqlDbType.Time,
                var t when t == typeof(UInt16) => MySqlDbType.UInt16,
                var t when t == typeof(UInt32) => MySqlDbType.UInt32,
                var t when t == typeof(UInt64) => MySqlDbType.UInt64,
                var t when t == typeof(UInt128) => MySqlDbType.UInt64,

                _ => throw new NotSupportedException($"Type '{type.FullName}' is not supported.")
            };
        }
    }
}
