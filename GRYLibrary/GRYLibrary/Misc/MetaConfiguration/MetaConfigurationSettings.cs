using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc.MetaConfiguration.ConfigurationFormats;
using GRYLibrary.Core.Misc.MetaConfiguration.Migrations;
using System.Collections.Generic;

namespace GRYLibrary.Core.Misc.MetaConfiguration
{
    public class MetaConfigurationSettings<T, TBase> where T : TBase, new()
    {
        public IConfigurationFormat ConfigurationFormat { get; set; }
        public T InitialValue { get; set; }
        public string File { get; set; }
        /// <summary>
        /// The migrations which will be applied (in this order) to an existing configuration-file before it will be loaded. See <see cref="XMLConfigurationFileMigrator"/>.
        /// </summary>
        /// <remarks>
        /// Migrations are only supported for the configuration-format <see cref="XML"/>.
        /// </remarks>
        public IList<ConfigurationFileMigration> Migrations { get; set; } = [];
        /// <summary>
        /// The log which is used for the information about executed migrations. It is required if <see cref="Migrations"/> is not empty.
        /// </summary>
        public IGeneralLogger? MigrationLog { get; set; }
    }
}
