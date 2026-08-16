using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Services.Res
{
    public interface IGeneralResourceLoader
    {
        public byte[] GetResource(string resourceName);
        public string GetResourceAsString(string resourceName);
        public ISet<string> GetAllResourceNames();
    }
}
