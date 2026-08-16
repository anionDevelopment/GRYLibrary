using System;

namespace GRYLibrary.Core.APIServer.Services
{
    public interface IExternalService : IDisposable
    {
        /// <returns>returns (true, null) if available and (false,reason) if not.</returns>
        public (bool, Exception?) IsAvailable();
    }
}
