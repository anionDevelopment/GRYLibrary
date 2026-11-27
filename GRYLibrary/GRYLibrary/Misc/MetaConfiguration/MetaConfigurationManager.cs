using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.Misc.MetaConfiguration.ConfigurationFormats;
using GRYLibrary.Core.Misc.MetaConfiguration.Migrations;
using System.Collections.Generic;
using System;
using System.IO;

namespace GRYLibrary.Core.Misc.MetaConfiguration
{
    public class MetaConfigurationManager
    {
        public static T GetConfiguration<T, TBase>(MetaConfigurationSettings<T, TBase> configuration, ISet<Type> knownTypes, out bool fileWasCreatedNew) where T : TBase, new()
        {
            HandleConfigurationVisitor<T, TBase> visitor = new HandleConfigurationVisitor<T, TBase>(configuration, knownTypes);
            T? result = configuration.ConfigurationFormat.Accept(visitor);
            fileWasCreatedNew = visitor.FileWasCreatedNew;
            return result;
        }

        private class HandleConfigurationVisitor<T, TBase> : IConfigurationFormatVisitor<T> where T : TBase, new()
        {
            private readonly MetaConfigurationSettings<T, TBase> _Configuration;
            private readonly ISet<Type> _KnownTypes;
            public bool FileWasCreatedNew { get; private set; }
            public HandleConfigurationVisitor(MetaConfigurationSettings<T, TBase> configuration, ISet<Type> knownTypes)
            {
                this._Configuration = configuration;
                this._KnownTypes = knownTypes;
            }

            public T Handle(XML xML)
            {
                XMLConfigurationFileMigrator? migrator = null;
                if (0 < this._Configuration.Migrations.Count)
                {
                    Utilities.AssertCondition(this._Configuration.MigrationLog != null, $"{nameof(MetaConfigurationSettings<T, TBase>.MigrationLog)} must be set if configuration-file-migrations are defined.");
                    migrator = new XMLConfigurationFileMigrator(new TimeService(), this._Configuration.MigrationLog!);
                    if (File.Exists(this._Configuration.File))
                    {
                        migrator.MigrateIfRequired(this._Configuration.File, this._Configuration.Migrations);
                    }
                }
                T? result = Utilities.CreateOrLoadXMLConfigurationFile<T, TBase>(this._Configuration.File, this._Configuration.InitialValue, this._KnownTypes, out bool fileWasCreatedNew);
                this.FileWasCreatedNew = fileWasCreatedNew;
                if (fileWasCreatedNew && migrator != null)
                {
                    // A new configuration-file already has the current structure, so no migration must ever be applied to it.
                    migrator.MarkAsExecuted(this._Configuration.File, this._Configuration.Migrations);
                }
                return result;
            }

            public T Handle(JSON jSON)
            {
                Utilities.AssertCondition(this._Configuration.Migrations.Count == 0, "Configuration-file-migrations are only supported for xml-configuration-files.");
                T? result = Utilities.CreateOrLoadJSONConfigurationFile<T, TBase>(this._Configuration.File, this._Configuration.InitialValue, out bool fileWasCreatedNew);
                this.FileWasCreatedNew = fileWasCreatedNew;
                return result;
            }
        }
    }
}
