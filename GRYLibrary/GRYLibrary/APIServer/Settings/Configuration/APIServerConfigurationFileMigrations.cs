using GRYLibrary.Core.Misc.MetaConfiguration.Migrations;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Settings.Configuration
{
    /// <summary>
    /// Provides the migrations of the configuration-file of an api-server.
    /// </summary>
    public static class APIServerConfigurationFileMigrations
    {
        /// <summary>
        /// The prefix of the names of the embedded xslt-resources which contain the internal migrations of GRYLibrary.
        /// </summary>
        public const string InternalMigrationsResourceNamePrefix = "GRYLibrary.Core.APIServer.ConfigurationFileMigrations.";

        /// <summary>
        /// Returns the migrations which GRYLibrary brings for the parts of the configuration-file which are defined by GRYLibrary.
        /// </summary>
        public static IList<ConfigurationFileMigration> GetInternalMigrations()
        {
            return ConfigurationFileMigration.LoadFromEmbeddedResources(typeof(APIServerConfigurationFileMigrations).Assembly, InternalMigrationsResourceNamePrefix);
        }

        /// <summary>
        /// Returns all migrations which will be applied to the configuration-file of an api-server: First the internal migrations of GRYLibrary, then the custom migrations of the application.
        /// </summary>
        public static IList<ConfigurationFileMigration> GetAllMigrations(IList<ConfigurationFileMigration> customMigrations)
        {
            return [.. GetInternalMigrations(), .. customMigrations];
        }
    }
}
