using GRYLibrary.Core.Misc.MetaConfiguration.ConfigurationFormats;

namespace GRYLibrary.Core.Misc.MetaConfiguration
{
    public class MetaConfigurationSettings<T, TBase> where T : TBase, new()
    {
        //TODO add possibility to define config-file-migration here
        public IConfigurationFormat ConfigurationFormat { get; set; }
        public T InitialValue { get; set; }
        public string File { get; set; }
    }
}