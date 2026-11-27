using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.Misc.MetaConfiguration.Migrations
{
    /// <summary>
    /// Represents one migration of a xml-configuration-file. A migration is always a xslt-transformation which transforms the content of the configuration-file from its old structure to its new structure.
    /// </summary>
    /// <remarks>
    /// The <see cref="Name"/> identifies the migration. It is stored in the migration-information-file (see <see cref="XMLConfigurationFileMigrator"/>) after the migration was executed, so the name of a migration must never be changed after it was released.
    /// </remarks>
    public class ConfigurationFileMigration
    {
        public const string MigrationFileExtension = ".xslt";
        public string Name { get; }
        public string XSLT { get; }

        public ConfigurationFileMigration(string name, string xslt)
        {
            this.Name = GUtilities.AssertNotNull(name, nameof(name), true);
            this.XSLT = GUtilities.AssertNotNull(xslt, nameof(xslt), true);
            GUtilities.AssertCondition(!string.IsNullOrWhiteSpace(this.Name), "The name of a configuration-file-migration must not be empty.");
        }

        /// <summary>
        /// Loads all embedded resources of <paramref name="assembly"/> whose name starts with <paramref name="resourceNamePrefix"/> and ends with ".xslt" as migrations.
        /// </summary>
        /// <returns>
        /// The migrations ordered by their resource-name (ordinal). The name of a migration is its resource-name without the extension ".xslt".
        /// </returns>
        public static IList<ConfigurationFileMigration> LoadFromEmbeddedResources(Assembly assembly, string resourceNamePrefix)
        {
            GUtilities.AssertNotNull(assembly, nameof(assembly), true);
            GUtilities.AssertNotNull(resourceNamePrefix, nameof(resourceNamePrefix), true);
            IList<ConfigurationFileMigration> result = [];
            IEnumerable<string> resourceNames = assembly.GetManifestResourceNames()
                .Where(resourceName => resourceName.StartsWith(resourceNamePrefix, StringComparison.Ordinal) && resourceName.EndsWith(MigrationFileExtension, StringComparison.Ordinal))
                .Order(StringComparer.Ordinal);
            foreach (string resourceName in resourceNames)
            {
                using Stream? stream = assembly.GetManifestResourceStream(resourceName);
                GUtilities.AssertCondition(stream != null, $"Configuration-file-migration-resource '{resourceName}' could not be loaded.");
                using StreamReader reader = new StreamReader(stream!);
                result.Add(new ConfigurationFileMigration(resourceName[..^MigrationFileExtension.Length], reader.ReadToEnd()));
            }
            return result;
        }
    }
}
