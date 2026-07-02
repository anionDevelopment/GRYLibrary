using GRYLibrary.Core.Misc.MetaConfiguration.ConfigurationFormats;
using System.Collections.Generic;
using System;

namespace GRYLibrary.Core.Misc.MetaConfiguration
{
    public class MetaConfigurationManager
    {
        public static T GetConfiguration<T, TBase>(MetaConfigurationSettings<T, TBase> configuration, ISet<Type> knownTypes, out bool fileWasCreatedNew) where T : TBase, new()
        {
            //TODO run migration from MetaConfigurationSettings here if required
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
                T? result = Utilities.CreateOrLoadXMLConfigurationFile<T, TBase>(this._Configuration.File, this._Configuration.InitialValue, this._KnownTypes, out bool fileWasCreatedNew);
                this.FileWasCreatedNew = fileWasCreatedNew;
                return result;
            }

            public T Handle(JSON jSON)
            {
                T? result = Utilities.CreateOrLoadJSONConfigurationFile<T, TBase>(this._Configuration.File, this._Configuration.InitialValue, out bool fileWasCreatedNew);
                this.FileWasCreatedNew = fileWasCreatedNew;
                return result;
            }
        }
    }
}