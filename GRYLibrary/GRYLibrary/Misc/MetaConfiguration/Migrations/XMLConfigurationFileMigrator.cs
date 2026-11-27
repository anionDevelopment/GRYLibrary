using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.Misc.MetaConfiguration.Migrations
{
    /// <summary>
    /// Applies <see cref="ConfigurationFileMigration"/>s to a xml-configuration-file.
    /// </summary>
    /// <remarks>
    /// The names of the executed migrations are stored in the migration-information-file "&lt;configuration-file&gt;.Migrations.xml" next to the configuration-file, so every migration is executed only once for a configuration-file.
    /// If a configuration-file exists but its migration-information-file does not exist (for example because the configuration-file was created by a version which did not support migrations yet), then all migrations will be executed.
    /// So every migration must be idempotent: Applied to a configuration-file which already has the new structure it must not change anything.
    /// Before the first migration is applied to a configuration-file a backup "&lt;configuration-file&gt;.bak-&lt;yyyyMMddHHmmss&gt;" of it will be created.
    /// </remarks>
    public class XMLConfigurationFileMigrator
    {
        public const string MigrationInformationFileSuffix = ".Migrations.xml";
        private const string RootElementName = "ExecutedConfigurationFileMigrations";
        private const string MigrationElementName = "ExecutedConfigurationFileMigration";
        private const string NameAttributeName = "Name";
        private const string ExecutionTimestampAttributeName = "ExecutionTimestamp";
        private readonly ITimeService _TimeService;
        private readonly IGeneralLogger _Log;

        public XMLConfigurationFileMigrator(ITimeService timeService, IGeneralLogger log)
        {
            this._TimeService = GUtilities.AssertNotNull(timeService, nameof(timeService), true);
            this._Log = GUtilities.AssertNotNull(log, nameof(log), true);
        }

        public static string GetMigrationInformationFile(string configurationFile)
        {
            return configurationFile + MigrationInformationFileSuffix;
        }

        /// <summary>
        /// Applies all migrations of <paramref name="migrations"/> (in the given order) which were not already applied to <paramref name="configurationFile"/>.
        /// </summary>
        /// <returns>The names of the migrations which were applied.</returns>
        public IList<string> MigrateIfRequired(string configurationFile, IList<ConfigurationFileMigration> migrations)
        {
            GUtilities.AssertCondition(File.Exists(configurationFile), $"Configuration-file '{configurationFile}' does not exist.");
            AssertNamesAreUnique(migrations);
            string migrationInformationFile = GetMigrationInformationFile(configurationFile);
            IList<string> namesOfExecutedMigrations = LoadNamesOfExecutedMigrations(migrationInformationFile);
            IList<ConfigurationFileMigration> migrationsToRun = [.. migrations.Where(migration => !namesOfExecutedMigrations.Contains(migration.Name))];
            if (migrationsToRun.Count == 0)
            {
                this._Log.Log($"No configuration-file-migrations for '{configurationFile}' found which were not already executed.", LogLevel.Debug);
                return [];
            }
            this._Log.Log($"{migrationsToRun.Count} configuration-file-migration(s) to run for '{configurationFile}' found.", LogLevel.Information);
            string backupFile = $"{configurationFile}.bak-{this._TimeService.GetCurrentLocalTimeAsDateTimeOffset().ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)}";
            File.Copy(configurationFile, backupFile, false);
            this._Log.Log($"Created backup '{backupFile}' of the configuration-file.", LogLevel.Information);
            IList<string> result = [];
            foreach (ConfigurationFileMigration migration in migrationsToRun)
            {
                this._Log.Log($"Run configuration-file-migration '{migration.Name}'.", LogLevel.Information);
                XmlDocument configuration = new XmlDocument();
                configuration.Load(configurationFile);
                XmlDocument migratedConfiguration = GUtilities.ApplyXSLTToXML(configuration, migration.XSLT);
                SaveXMLDocument(migratedConfiguration, configurationFile);
                this.AddExecutedMigration(migrationInformationFile, migration.Name);
                result.Add(migration.Name);
            }
            this._Log.Log($"Finished configuration-file-migrations for '{configurationFile}'.", LogLevel.Information);
            return result;
        }

        /// <summary>
        /// Marks all migrations of <paramref name="migrations"/> as executed for <paramref name="configurationFile"/> without applying them.
        /// </summary>
        /// <remarks>
        /// This is intended to be called after a new configuration-file was created, because a new configuration-file already has the current structure.
        /// </remarks>
        public void MarkAsExecuted(string configurationFile, IList<ConfigurationFileMigration> migrations)
        {
            AssertNamesAreUnique(migrations);
            string migrationInformationFile = GetMigrationInformationFile(configurationFile);
            IList<string> namesOfExecutedMigrations = LoadNamesOfExecutedMigrations(migrationInformationFile);
            foreach (ConfigurationFileMigration migration in migrations.Where(migration => !namesOfExecutedMigrations.Contains(migration.Name)))
            {
                this.AddExecutedMigration(migrationInformationFile, migration.Name);
            }
        }

        public static IList<string> LoadNamesOfExecutedMigrations(string migrationInformationFile)
        {
            if (!File.Exists(migrationInformationFile))
            {
                return [];
            }
            XDocument document = XDocument.Load(migrationInformationFile);
            GUtilities.AssertCondition(document.Root?.Name.LocalName == RootElementName, $"Migration-information-file '{migrationInformationFile}' does not have the root-element '{RootElementName}'.");
            return [.. document.Root!.Elements(MigrationElementName).Select(element => element.Attribute(NameAttributeName)!.Value)];
        }

        private void AddExecutedMigration(string migrationInformationFile, string migrationName)
        {
            XDocument document = File.Exists(migrationInformationFile) ? XDocument.Load(migrationInformationFile) : new XDocument(new XElement(RootElementName));
            document.Root!.Add(new XElement(MigrationElementName,
                new XAttribute(NameAttributeName, migrationName),
                new XAttribute(ExecutionTimestampAttributeName, this._TimeService.GetCurrentLocalTimeAsDateTimeOffset().ToString("o", CultureInfo.InvariantCulture))));
            using XmlWriter writer = XmlWriter.Create(migrationInformationFile, GUtilities.XMLWriterDefaultSettings);
            document.Save(writer);
        }

        private static void SaveXMLDocument(XmlDocument document, string file)
        {
            // The declaration of the transformed document states the encoding of the string it was transformed into, so it is removed and the writer writes a declaration which matches the encoding of the file.
            if (document.FirstChild is XmlDeclaration declaration)
            {
                document.RemoveChild(declaration);
            }
            using XmlWriter writer = XmlWriter.Create(file, GUtilities.XMLWriterDefaultSettings);
            document.Save(writer);
        }

        private static void AssertNamesAreUnique(IList<ConfigurationFileMigration> migrations)
        {
            GUtilities.AssertNotNull(migrations, nameof(migrations), true);
            IEnumerable<string> duplicatedNames = migrations.GroupBy(migration => migration.Name).Where(group => 1 < group.Count()).Select(group => group.Key);
            GUtilities.AssertCondition(!duplicatedNames.Any(), $"The names of configuration-file-migrations must be unique, but the following names occur more than once: {string.Join(", ", duplicatedNames)}");
        }
    }
}
