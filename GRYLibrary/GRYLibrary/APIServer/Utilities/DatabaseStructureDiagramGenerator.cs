using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.Logging.GRYLogger;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace GRYLibrary.Core.APIServer.Utilities
{
    /// <summary>
    /// Generates a PlantUML entity-relationship diagram of the actual database-schema of a project-specific database.
    /// <para>
    /// This class is generic on purpose: it is parametrized only by <see cref="IProjectSpecificDatabaseInteractor"/> and does not know anything about
    /// a database's concrete tables. It reads the schema-information of an already-migrated database back from the database's own
    /// <c>information_schema</c> (columns, primary-keys and foreign-keys) instead of parsing the migration-SQL statically, so the produced diagram always
    /// reflects what the migrations actually produced, on the concrete database-engine they were executed against. It is used by the per-database-type
    /// structure-testcases (<c>DatabaseStructureTestsForMariaDB</c>/<c>DatabaseStructureTestsForPostgreSQL</c>), which first run all migrations against a
    /// real, freshly-reset test-database and then call <see cref="Generate{TProjectSpecificDatabaseInteractor}"/> to turn the resulting schema into a
    /// <c>.plantuml</c> file that is rendered to an SVG-diagram when the reference-documentation is generated.
    /// </para>
    /// <para>
    /// The generated PlantUML-content is deterministic: it does not depend on the database-engine's row-order, on the culture of the executing machine or on
    /// the operating-system, because all tables, columns and relations are sorted by an ordinal string-comparison and the lines are always separated by a
    /// line-feed.
    /// </para>
    /// </summary>
    public static class DatabaseStructureDiagramGenerator
    {
        private const string MigrationBookkeepingTableName = "GRYMigrationInformation";
        private const string MigrationTableNameParameterName = "MigrationTableName";
        private const string PrimaryKeyIndicator = "PRI";
        private const string LineSeparator = "\n";
        private static readonly string[] SystemSchemas = { "information_schema", "pg_catalog", "mysql", "performance_schema", "sys" };

        /// <summary>
        /// Reads the schema of the already-migrated <paramref name="database"/> from <c>information_schema</c>, renders it as a PlantUML
        /// entity-relationship-diagram titled <paramref name="title"/>, writes it to <paramref name="targetFile"/> and returns the generated PlantUML text.
        /// </summary>
        public static string Generate<TProjectSpecificDatabaseInteractor>(TProjectSpecificDatabaseInteractor database, IGRYLog log, string title, string targetFile) where TProjectSpecificDatabaseInteractor : IProjectSpecificDatabaseInteractor
        {
            SchemaQueries queries = database.GetGenericDatabaseInteractor().Accept(new SchemaQueriesVisitor());

            IList<ColumnInformation> columns = DBUtilities.RunTransaction<IList<ColumnInformation>, TProjectSpecificDatabaseInteractor>(nameof(Generate), log, database, true, (command) =>
            {
                IGenericDatabaseInteractor gi = database.GetGenericDatabaseInteractor();
                command.CommandText = queries.ColumnsStatement;
                command.Parameters.Add(gi.GetParameter(MigrationTableNameParameterName, MigrationBookkeepingTableName));
                List<ColumnInformation> result = new List<ColumnInformation>();
                using DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string tableName = reader.GetString(0);
                    string columnName = reader.GetString(1);
                    string columnType = reader.GetString(2);
                    bool isNullable = reader.GetString(3) == "YES";
                    bool isPrimaryKey = reader.GetString(4) == PrimaryKeyIndicator;
                    result.Add(new ColumnInformation(tableName, columnName, columnType, isNullable, isPrimaryKey));
                }
                return result;
            })[0]!;

            IList<ForeignKeyInformation> foreignKeys = DBUtilities.RunTransaction<IList<ForeignKeyInformation>, TProjectSpecificDatabaseInteractor>(nameof(Generate), log, database, true, (command) =>
            {
                IGenericDatabaseInteractor gi = database.GetGenericDatabaseInteractor();
                command.CommandText = queries.ForeignKeysStatement;
                command.Parameters.Add(gi.GetParameter(MigrationTableNameParameterName, MigrationBookkeepingTableName));
                List<ForeignKeyInformation> result = new List<ForeignKeyInformation>();
                using DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new ForeignKeyInformation(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                }
                return result;
            })[0]!;

            string plantUml = GeneratePlantUml(title, columns, foreignKeys);
            Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
            File.WriteAllText(targetFile, plantUml, new UTF8Encoding(false));
            return plantUml;
        }

        private static string GeneratePlantUml(string title, IList<ColumnInformation> columns, IList<ForeignKeyInformation> foreignKeys)
        {
            StringBuilder result = new StringBuilder();
            AppendLine(result, "@startuml");
            AppendLine(result, $"title {title}");
            AppendLine(result, "hide circle");
            AppendLine(result, "skinparam linetype ortho");
            AppendLine(result, string.Empty);

            foreach (IGrouping<string, ColumnInformation> table in GetDistinctColumns(columns).GroupBy(column => column.TableName).OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                AppendLine(result, $"entity \"{table.Key}\" {{");
                List<ColumnInformation> primaryKeyColumns = table.Where(column => column.IsPrimaryKey).OrderBy(column => column.ColumnName, StringComparer.Ordinal).ToList();
                List<ColumnInformation> otherColumns = table.Where(column => !column.IsPrimaryKey).OrderBy(column => column.ColumnName, StringComparer.Ordinal).ToList();
                foreach (ColumnInformation column in primaryKeyColumns)
                {
                    AppendLine(result, $"  * {column.ColumnName} : {column.ColumnType} <<PK>>");
                }
                if (primaryKeyColumns.Count > 0 && otherColumns.Count > 0)
                {
                    AppendLine(result, "  --");
                }
                foreach (ColumnInformation column in otherColumns)
                {
                    string notNullSuffix = column.IsNullable ? string.Empty : " <<NOT NULL>>";
                    AppendLine(result, $"  {column.ColumnName} : {column.ColumnType}{notNullSuffix}");
                }
                AppendLine(result, "}");
                AppendLine(result, string.Empty);
            }

            foreach (ForeignKeyInformation foreignKey in GetDistinctForeignKeys(foreignKeys))
            {
                AppendLine(result, $"\"{foreignKey.TableName}\" }}o--|| \"{foreignKey.ReferencedTableName}\" : {foreignKey.ColumnName}");
            }

            AppendLine(result, "@enduml");
            return result.ToString();
        }

        private static void AppendLine(StringBuilder stringBuilder, string line)
        {
            stringBuilder.Append(line).Append(LineSeparator);
        }

        /// <summary>
        /// Returns the given <paramref name="columns"/> without duplicates. A column is identified by its table-name and its column-name.
        /// </summary>
        /// <remarks>
        /// Duplicates must not occur with the statements of <see cref="SchemaQueriesVisitor"/>, but a database-engine which reports the same column more than
        /// once must not result in a diagram which lists that column more than once.
        /// </remarks>
        private static IEnumerable<ColumnInformation> GetDistinctColumns(IList<ColumnInformation> columns)
        {
            return columns
                .GroupBy(column => (column.TableName, column.ColumnName))
                .Select(group => group.OrderBy(column => column.ColumnType, StringComparer.Ordinal).First());
        }

        /// <summary>
        /// Returns the given <paramref name="foreignKeys"/> without duplicates, sorted deterministically. A foreign-key is identified by its table-name, its
        /// column-name, the name of the referenced table and the name of the referenced column.
        /// </summary>
        private static IEnumerable<ForeignKeyInformation> GetDistinctForeignKeys(IList<ForeignKeyInformation> foreignKeys)
        {
            return foreignKeys
                .GroupBy(foreignKey => (foreignKey.TableName, foreignKey.ColumnName, foreignKey.ReferencedTableName, foreignKey.ReferencedColumnName))
                .Select(group => group.First())
                .OrderBy(foreignKey => foreignKey.TableName, StringComparer.Ordinal)
                .ThenBy(foreignKey => foreignKey.ColumnName, StringComparer.Ordinal)
                .ThenBy(foreignKey => foreignKey.ReferencedTableName, StringComparer.Ordinal)
                .ThenBy(foreignKey => foreignKey.ReferencedColumnName, StringComparer.Ordinal);
        }

        /// <summary>
        /// Contains the SQL-statements which read the schema-information of the currently connected database.
        /// <para>
        /// Both statements have exactly one parameter (<see cref="MigrationTableNameParameterName"/>, the name of the migration-bookkeeping-table which must
        /// not be part of the diagram) and both return their columns in the order which is expected by
        /// <see cref="Generate{TProjectSpecificDatabaseInteractor}"/>.
        /// </para>
        /// </summary>
        private sealed class SchemaQueries
        {
            /// <summary>Returns table-name, column-name, column-type, nullability ("YES"/"NO") and primary-key-indicator (<see cref="PrimaryKeyIndicator"/> if the column belongs to the primary-key).</summary>
            public string ColumnsStatement { get; }

            /// <summary>Returns table-name, column-name, referenced table-name and referenced column-name of every foreign-key-column.</summary>
            public string ForeignKeysStatement { get; }

            public SchemaQueries(string columnsStatement, string foreignKeysStatement)
            {
                this.ColumnsStatement = columnsStatement;
                this.ForeignKeysStatement = foreignKeysStatement;
            }
        }

        /// <summary>
        /// Provides the <see cref="SchemaQueries"/> for the concrete database-engine.
        /// <para>
        /// The statements must be engine-specific because the content of <c>information_schema</c> is not the same everywhere: MariaDB names the
        /// primary-key-constraint of every table "PRIMARY", so constraint-names are only unique per table there and joining <c>TABLE_CONSTRAINTS</c> or
        /// <c>REFERENTIAL_CONSTRAINTS</c> to <c>KEY_COLUMN_USAGE</c> by constraint-name alone matches the constraints of all tables of the database. MariaDB
        /// therefore uses its own additional columns (<c>COLUMN_KEY</c> and <c>REFERENCED_TABLE_NAME</c>/<c>REFERENCED_COLUMN_NAME</c>), which do not exist in
        /// other engines but make any join superfluous.
        /// </para>
        /// </summary>
        private sealed class SchemaQueriesVisitor : IGenericDatabaseInteractorVisitor<SchemaQueries>
        {
            public SchemaQueries Handle(MariaDBDatabaseInteractor mariaDBDatabaseInteractor)
            {
                //In MariaDB a connection is always connected to exactly one database (=schema), but information_schema contains the information of all
                //databases of the server. Restricting the result to DATABASE() is therefore required to not mix up equally-named tables of other databases.
                string columnsStatement = @$"SELECT c.TABLE_NAME, c.COLUMN_NAME, c.DATA_TYPE, c.IS_NULLABLE, CASE WHEN c.COLUMN_KEY = '{PrimaryKeyIndicator}' THEN '{PrimaryKeyIndicator}' ELSE '' END AS COLUMN_KEY
FROM information_schema.COLUMNS c
WHERE c.TABLE_SCHEMA = DATABASE() AND c.TABLE_NAME <> @{MigrationTableNameParameterName}
ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION;";
                string foreignKeysStatement = @$"SELECT kcu.TABLE_NAME, kcu.COLUMN_NAME, kcu.REFERENCED_TABLE_NAME, kcu.REFERENCED_COLUMN_NAME
FROM information_schema.KEY_COLUMN_USAGE kcu
WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.REFERENCED_TABLE_NAME IS NOT NULL AND kcu.TABLE_NAME <> @{MigrationTableNameParameterName}
ORDER BY kcu.TABLE_NAME, kcu.COLUMN_NAME, kcu.REFERENCED_TABLE_NAME, kcu.REFERENCED_COLUMN_NAME;";
                return new SchemaQueries(columnsStatement, foreignKeysStatement);
            }

            public SchemaQueries Handle(PostgreSQLDatabaseInteractor postgreSQLDatabaseInteractor)
            {
                return GetStandardInformationSchemaQueries();
            }

            public SchemaQueries Handle(SQLServerDatabaseInteractor sQLServerDatabaseInteractor)
            {
                return GetStandardInformationSchemaQueries();
            }

            public SchemaQueries Handle(OracleDatabaseInteractor oracleDatabaseInteractor)
            {
                throw new NotSupportedException($"Generating a database-structure-diagram is not supported for {nameof(OracleDatabaseInteractor)} because Oracle does not provide an information_schema.");
            }

            /// <summary>
            /// Returns the statements for database-engines which implement <c>information_schema</c> as defined by the SQL-standard and which have
            /// database-wide unique constraint-names (in contrast to MariaDB).
            /// </summary>
            private static SchemaQueries GetStandardInformationSchemaQueries()
            {
                //The system-schemas are compile-time-constants of this class, so they can be embedded in the statement directly.
                string systemSchemas = string.Join(", ", SystemSchemas.Select(schema => $"'{schema}'"));
                string columnsStatement = @$"SELECT c.TABLE_NAME, c.COLUMN_NAME, c.DATA_TYPE, c.IS_NULLABLE, CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN '{PrimaryKeyIndicator}' ELSE '' END AS COLUMN_KEY
FROM information_schema.COLUMNS c
LEFT JOIN (
    SELECT kcu.TABLE_SCHEMA, kcu.TABLE_NAME, kcu.COLUMN_NAME
    FROM information_schema.TABLE_CONSTRAINTS tc
    JOIN information_schema.KEY_COLUMN_USAGE kcu ON kcu.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA AND kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME AND kcu.TABLE_SCHEMA = tc.TABLE_SCHEMA AND kcu.TABLE_NAME = tc.TABLE_NAME
    WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
) pk ON pk.TABLE_SCHEMA = c.TABLE_SCHEMA AND pk.TABLE_NAME = c.TABLE_NAME AND pk.COLUMN_NAME = c.COLUMN_NAME
WHERE c.TABLE_NAME <> @{MigrationTableNameParameterName} AND c.TABLE_SCHEMA NOT IN ({systemSchemas})
ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION;";
                string foreignKeysStatement = @$"SELECT fk.TABLE_NAME, fk.COLUMN_NAME, referencedKey.TABLE_NAME AS REFERENCED_TABLE_NAME, referencedKey.COLUMN_NAME AS REFERENCED_COLUMN_NAME
FROM information_schema.TABLE_CONSTRAINTS tc
JOIN information_schema.KEY_COLUMN_USAGE fk ON fk.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA AND fk.CONSTRAINT_NAME = tc.CONSTRAINT_NAME AND fk.TABLE_SCHEMA = tc.TABLE_SCHEMA AND fk.TABLE_NAME = tc.TABLE_NAME
JOIN information_schema.REFERENTIAL_CONSTRAINTS rc ON rc.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA AND rc.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
JOIN information_schema.TABLE_CONSTRAINTS referencedConstraint ON referencedConstraint.CONSTRAINT_SCHEMA = rc.UNIQUE_CONSTRAINT_SCHEMA AND referencedConstraint.CONSTRAINT_NAME = rc.UNIQUE_CONSTRAINT_NAME
JOIN information_schema.KEY_COLUMN_USAGE referencedKey ON referencedKey.CONSTRAINT_SCHEMA = referencedConstraint.CONSTRAINT_SCHEMA AND referencedKey.CONSTRAINT_NAME = referencedConstraint.CONSTRAINT_NAME AND referencedKey.TABLE_SCHEMA = referencedConstraint.TABLE_SCHEMA AND referencedKey.TABLE_NAME = referencedConstraint.TABLE_NAME AND referencedKey.ORDINAL_POSITION = fk.POSITION_IN_UNIQUE_CONSTRAINT
WHERE tc.CONSTRAINT_TYPE = 'FOREIGN KEY' AND tc.TABLE_NAME <> @{MigrationTableNameParameterName} AND tc.TABLE_SCHEMA NOT IN ({systemSchemas})
ORDER BY fk.TABLE_NAME, fk.COLUMN_NAME, referencedKey.TABLE_NAME, referencedKey.COLUMN_NAME;";
                return new SchemaQueries(columnsStatement, foreignKeysStatement);
            }
        }

        private sealed class ColumnInformation
        {
            public string TableName { get; }
            public string ColumnName { get; }
            public string ColumnType { get; }
            public bool IsNullable { get; }
            public bool IsPrimaryKey { get; }

            public ColumnInformation(string tableName, string columnName, string columnType, bool isNullable, bool isPrimaryKey)
            {
                this.TableName = tableName;
                this.ColumnName = columnName;
                this.ColumnType = columnType;
                this.IsNullable = isNullable;
                this.IsPrimaryKey = isPrimaryKey;
            }
        }

        private sealed class ForeignKeyInformation
        {
            public string TableName { get; }
            public string ColumnName { get; }
            public string ReferencedTableName { get; }
            public string ReferencedColumnName { get; }

            public ForeignKeyInformation(string tableName, string columnName, string referencedTableName, string referencedColumnName)
            {
                this.TableName = tableName;
                this.ColumnName = columnName;
                this.ReferencedTableName = referencedTableName;
                this.ReferencedColumnName = referencedColumnName;
            }
        }
    }
}
