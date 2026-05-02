using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.Logging.GRYLogger;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace GRYLibrary.Core.APIServer.Services.Database
{
    public class PostgreSQLDatabaseInteractor : GenericDatabaseInteractor
    {
        public PostgreSQLDatabaseInteractor(IDatabasePersistenceConfiguration configuration, IServerLog log) : this(configuration, log.Logger)
        {
        }
        public PostgreSQLDatabaseInteractor(IDatabasePersistenceConfiguration configuration, IGRYLog log) : base(configuration, log)
        {
        }
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        private static readonly Regex PasswordHideRegex = new Regex("(PWD|Pwd)=([^;]+)(;|$)");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        public override DbCommand CreateCommand(string sql)
        {
            DbConnection connection = this.GetConnection();
            Misc.Utilities.AssertCondition(connection.State == System.Data.ConnectionState.Open, $"Connection of {connection.GetType().Name} is not open.");
            return new NpgsqlCommand(sql, (NpgsqlConnection)connection);
        }

        public override string EscapePasswordInConnectionString(string connectionString)
        {
            string replaceString = "********";
            connectionString = PasswordHideRegex.Replace(connectionString, match => $"{match.Groups[1]}={replaceString}{match.Groups[3]}");
            return connectionString;
        }

        public override string CreateSQLStatementForCreatingMigrationMaintenanceTableIfNotExist(string migrationTableName)
        {
            return $@"CREATE TABLE IF NOT EXISTS ""{migrationTableName}"" (
    ""MigrationName"" VARCHAR(255),
    ""ExecutionTimestamp"" TIMESTAMP);
";
        }

        public override string GetSQLStatementForSelectMigrationMaintenanceTableContent(string migrationTableName)
        {
            return $@"select ""MigrationName"", ""ExecutionTimestamp"" from ""{migrationTableName}"";";
        }

        public override string GetSQLStatementForRunningMigration(string migrationContent, string migrationTableName, string migrationName, DateTimeOffset now)
        {
            DateTimeOffset noUtc = now.ToUniversalTime();
            return @$"
{migrationContent}
insert into ""{migrationTableName}""(""MigrationName"", ""ExecutionTimestamp"") values ('{migrationName}', '{noUtc:yyyy-MM-dd HH:mm:ss}')
";
        }

        public override string CreateSQLStatementForGetAllTableNames()
        {
            return @"SELECT tablename
FROM pg_catalog.pg_tables
WHERE schemaname NOT IN ('pg_catalog', 'information_schema');";
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
            return new NpgsqlConnection(connectionString);
        }
        public override DbParameter GetParameter(string parameterName, object? value, Type type)
        {
            object formattedValue = this.FormatValue(value);
            Type adaptedType = this.AdaptType(type);
            NpgsqlDbType dbType = this.GetType(adaptedType, parameterName);
            return new NpgsqlParameter()
            {
                ParameterName = "@" + parameterName,
                Value = formattedValue,
                NpgsqlDbType = dbType,
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
            else if (value is UInt16 valusAsUInt16)
            {
                result = (Int16)valusAsUInt16;
            }
            else if (value is UInt32 valusAsUInt32)
            {
                result = (Int32)valusAsUInt32;
            }
            else if (value is UInt64 valusAsUInt64)
            {
                result = (Int64)valusAsUInt64;
            }
            else if (value is UInt128 valusAsUInt128)
            {
                result = (Int128)valusAsUInt128;
            }
            else
            {
                result = value;
            }
            return result;
        }

        private NpgsqlDbType GetType(Type type, string parameterName)
        {
            return type switch
            {
                var t when t == typeof(string) => NpgsqlDbType.Varchar,
                var t when t == typeof(int) => NpgsqlDbType.Integer,
                var t when t == typeof(long) => NpgsqlDbType.Bigint,
                var t when t == typeof(short) => NpgsqlDbType.Smallint,
                var t when t == typeof(byte) => NpgsqlDbType.Smallint,
                var t when t == typeof(bool) => NpgsqlDbType.Boolean,
                var t when t == typeof(DateTime) => NpgsqlDbType.Timestamp,
                var t when t == typeof(DateTimeOffset) => NpgsqlDbType.TimestampTz,
                var t when t == typeof(float) => NpgsqlDbType.Real,
                var t when t == typeof(double) => NpgsqlDbType.Double,
                var t when t == typeof(decimal) => NpgsqlDbType.Numeric,
                var t when t == typeof(Guid) => NpgsqlDbType.Uuid,
                var t when t == typeof(byte[]) => NpgsqlDbType.Bytea,
                var t when t == typeof(char) => NpgsqlDbType.Varchar,
                var t when t == typeof(TimeSpan) => NpgsqlDbType.Interval,

                _ => throw new NotSupportedException($"Type '{type.FullName}' is not supported. (Parametername: {parameterName})")
            };
        }
        private Type AdaptType(Type type)
        {
            return type switch
            {
                var t when t == typeof(UInt16) => typeof(Int16),
                var t when t == typeof(UInt32) => typeof(Int32),
                var t when t == typeof(UInt64) => typeof(Int64),
                var t when t == typeof(UInt128) => typeof(Int128),
                _ => type
            };
        }
    }
}
