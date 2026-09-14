using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.Logging.GRYLogger;
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
    /// </summary>
    public static class DatabaseStructureDiagramGenerator
    {
        private const string MigrationBookkeepingTableName = "GRYMigrationInformation";
        private static readonly string[] SystemSchemas = { "information_schema", "pg_catalog", "mysql", "performance_schema", "sys" };

        /// <summary>
        /// Reads the schema of the already-migrated <paramref name="database"/> from <c>information_schema</c>, renders it as a PlantUML
        /// entity-relationship-diagram titled <paramref name="title"/>, writes it to <paramref name="targetFile"/> and returns the generated PlantUML text.
        /// </summary>
        public static string Generate<TProjectSpecificDatabaseInteractor>(TProjectSpecificDatabaseInteractor database, IGRYLog log, string title, string targetFile) where TProjectSpecificDatabaseInteractor : IProjectSpecificDatabaseInteractor
        {
            IList<ColumnInformation> columns = DBUtilities.RunTransaction<IList<ColumnInformation>, TProjectSpecificDatabaseInteractor>(nameof(Generate), log, database, true, (command) =>
            {
                IGenericDatabaseInteractor gi = database.GetGenericDatabaseInteractor();
                command.CommandText = @"SELECT c.TABLE_NAME, c.COLUMN_NAME, c.DATA_TYPE, c.IS_NULLABLE, CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 'PRI' ELSE '' END AS COLUMN_KEY
FROM information_schema.COLUMNS c
LEFT JOIN (
    SELECT kcu.TABLE_SCHEMA, kcu.TABLE_NAME, kcu.COLUMN_NAME
    FROM information_schema.TABLE_CONSTRAINTS tc
    JOIN information_schema.KEY_COLUMN_USAGE kcu ON kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME AND kcu.TABLE_SCHEMA = tc.TABLE_SCHEMA
    WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
) pk ON pk.TABLE_SCHEMA = c.TABLE_SCHEMA AND pk.TABLE_NAME = c.TABLE_NAME AND pk.COLUMN_NAME = c.COLUMN_NAME
WHERE c.TABLE_NAME <> @MigrationTableName AND c.TABLE_SCHEMA NOT IN (@SystemSchema0, @SystemSchema1, @SystemSchema2, @SystemSchema3, @SystemSchema4)
ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION;";
                command.Parameters.Add(gi.GetParameter("MigrationTableName", MigrationBookkeepingTableName));
                for (int i = 0; i < SystemSchemas.Length; i++)
                {
                    command.Parameters.Add(gi.GetParameter($"SystemSchema{i}", SystemSchemas[i]));
                }
                List<ColumnInformation> result = new List<ColumnInformation>();
                using DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string tableName = reader.GetString(0);
                    string columnName = reader.GetString(1);
                    string columnType = reader.GetString(2);
                    bool isNullable = reader.GetString(3) == "YES";
                    bool isPrimaryKey = reader.GetString(4) == "PRI";
                    result.Add(new ColumnInformation(tableName, columnName, columnType, isNullable, isPrimaryKey));
                }
                return result;
            })[0]!;

            IList<ForeignKeyInformation> foreignKeys = DBUtilities.RunTransaction<IList<ForeignKeyInformation>, TProjectSpecificDatabaseInteractor>(nameof(Generate), log, database, true, (command) =>
            {
                IGenericDatabaseInteractor gi = database.GetGenericDatabaseInteractor();
                command.CommandText = @"SELECT kcu.TABLE_NAME, kcu.COLUMN_NAME, ccu.TABLE_NAME AS REFERENCED_TABLE_NAME, ccu.COLUMN_NAME AS REFERENCED_COLUMN_NAME
FROM information_schema.KEY_COLUMN_USAGE kcu
JOIN information_schema.REFERENTIAL_CONSTRAINTS rc ON rc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME AND rc.CONSTRAINT_SCHEMA = kcu.TABLE_SCHEMA
JOIN information_schema.KEY_COLUMN_USAGE ccu ON ccu.CONSTRAINT_NAME = rc.UNIQUE_CONSTRAINT_NAME AND ccu.TABLE_SCHEMA = rc.UNIQUE_CONSTRAINT_SCHEMA AND ccu.ORDINAL_POSITION = kcu.ORDINAL_POSITION
WHERE kcu.TABLE_SCHEMA NOT IN (@SystemSchema0, @SystemSchema1, @SystemSchema2, @SystemSchema3, @SystemSchema4)
ORDER BY kcu.TABLE_NAME, kcu.COLUMN_NAME;";
                for (int i = 0; i < SystemSchemas.Length; i++)
                {
                    command.Parameters.Add(gi.GetParameter($"SystemSchema{i}", SystemSchemas[i]));
                }
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
            result.AppendLine("@startuml");
            result.AppendLine($"title {title}");
            result.AppendLine("hide circle");
            result.AppendLine("skinparam linetype ortho");
            result.AppendLine();

            foreach (IGrouping<string, ColumnInformation> table in columns.GroupBy(column => column.TableName).OrderBy(g => g.Key))
            {
                result.AppendLine($"entity \"{table.Key}\" {{");
                List<ColumnInformation> primaryKeyColumns = table.Where(column => column.IsPrimaryKey).OrderBy(c => c.ColumnName).ToList();
                List<ColumnInformation> otherColumns = table.Where(column => !column.IsPrimaryKey).OrderBy(c => c.ColumnName).ToList();
                foreach (ColumnInformation column in primaryKeyColumns)
                {
                    result.AppendLine($"  * {column.ColumnName} : {column.ColumnType} <<PK>>");
                }
                if (primaryKeyColumns.Count > 0 && otherColumns.Count > 0)
                {
                    result.AppendLine("  --");
                }
                foreach (ColumnInformation column in otherColumns)
                {
                    string notNullSuffix = column.IsNullable ? string.Empty : " <<NOT NULL>>";
                    result.AppendLine($"  {column.ColumnName} : {column.ColumnType}{notNullSuffix}");
                }
                result.AppendLine("}");
                result.AppendLine();
            }

            foreach (ForeignKeyInformation foreignKey in foreignKeys.OrderBy(fk => fk.TableName).ThenBy(fk => fk.ColumnName).ThenBy(fk => fk.ReferencedTableName).ThenBy(fk => fk.ReferencedColumnName))
            {
                result.AppendLine($"\"{foreignKey.TableName}\" }}o--|| \"{foreignKey.ReferencedTableName}\" : {foreignKey.ColumnName}");
            }

            result.AppendLine("@enduml");
            return result.ToString();
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
