using GRYLibrary.Core.APIServer.Utilities;
using System;

namespace GRYLibrary.Core.APIServer.Services.Interfaces
{
    public interface IInitializable
    {
        public InitializationState InitializationState { get; }
        public void Initialize();
        public void WaitUntilAvailable(TimeSpan timeSpan);
    }
}
