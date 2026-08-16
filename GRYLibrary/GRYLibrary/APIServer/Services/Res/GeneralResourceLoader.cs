using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace GRYLibrary.Core.APIServer.Services.Res
{
    public class GeneralResourceLoader : IGeneralResourceLoader
    {
        /// <remarks>
        /// This cache is shared by all threads which handle requests, because this service is registered as a
        /// singleton. It must therefore be thread-safe: with a plain dictionary two concurrent cache-misses
        /// corrupt its internal state, and after that every further access to it throws an
        /// <see cref="System.InvalidOperationException"/>, so the application answers every request with an
        /// error until it is restarted.
        /// </remarks>
        private readonly ConcurrentDictionary<string, byte[]> _Cache = new ConcurrentDictionary<string, byte[]>();
        private readonly string _BaseNamespace;
        protected readonly Assembly _Assembly;
        private static readonly Encoding UTF8EncodingInstance = new UTF8Encoding(false);
        public GeneralResourceLoader(string baseNamespace, Assembly assembly)
        {
            this._BaseNamespace = baseNamespace;
            this._Assembly = assembly;
        }
        public byte[] GetResource(string resourceName)
        {
            // Two threads which request the same resource at the same time can both load it, but only one of the
            // results is put into the cache and both get that one. Loading a resource twice is harmless because
            // it only reads from the assembly, and an exception of the loading is not cached.
            return this._Cache.GetOrAdd(resourceName, this.LoadResource);
        }

        private byte[] LoadResource(string resourceName)
        {
            using Stream? resFilestream = this._Assembly.GetManifestResourceStream(this._BaseNamespace + "." + resourceName);
            if (resFilestream == null)
            {
                throw new KeyNotFoundException($"No resource available with name \"{resourceName}\".");
            }
            byte[] content = new byte[resFilestream.Length];
            //ReadExactly instead of Read because Read is allowed to return less bytes than requested.
            resFilestream.ReadExactly(content);
            return content;
        }

        public string GetResourceAsString(string resourceName)
        {
            return UTF8EncodingInstance.GetString(this.GetResource(resourceName));
        }

        public ISet<string> GetAllResourceNames()
        {
            return new HashSet<string>(this._Assembly.GetManifestResourceNames());
        }
    }
}
