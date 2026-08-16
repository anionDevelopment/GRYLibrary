using GRYLibrary.Core.APIServer.Services;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using Microsoft.Extensions.Logging;
using System;

namespace GRYLibrary.Core.APIServer.BaseServices
{
    public abstract class ExternalService : IExternalService
    {
        protected readonly IGeneralLogger _Logger;
        public bool IsConnected { get; private set; }
        public string ExternalServiceName { get; private set; }
        public string ServiceNotAvailableMessage { get; private set; }
        public abstract (bool, Exception?) IsAvailable();

        protected ExternalService(string externalServiceName, IGeneralLogger logger)
        {
            this.ExternalServiceName = externalServiceName;
            this._Logger = logger;
            this.IsConnected = false;
            this.ServiceNotAvailableMessage = $"Service {this.ExternalServiceName} is not available.";
        }

        public void EnsureServiceIsConnected()
        {
            if (!this.TryConnect(out Exception exception))
            {
                throw new ServiceNotAvailableException(this.ServiceNotAvailableMessage, exception);
            }
        }
        public void EnsureServiceIsConnected(Action action)
        {
            if (!this.TryConnect(out Exception exception))
            {
                throw new ServiceNotAvailableException(this.ServiceNotAvailableMessage, exception);
            }
            action();
        }
        public T EnsureServiceIsConnected<T>(Func<T> action)
        {
            if (!this.TryConnect(out Exception exception))
            {
                throw new ServiceNotAvailableException(this.ServiceNotAvailableMessage, exception);
            }
            return action();
        }
        public void EnsureServiceIsDisconnected()
        {
            if (this.IsConnected)
            {
                this.Disconnect();
            }
        }
        public abstract bool TryConnectImplementation(out Exception exception);
        public abstract void DisconnectImplementation();
        public bool TryConnect(out Exception exception)
        {
            if (this.IsConnected)
            {
                exception = null;
                return true;
            }
            else
            {
                bool result = this.TryConnectImplementation(out exception);
                if (result)
                {
                    this.IsConnected = true;
                }
                else
                {
                    this._Logger.Log(this.ServiceNotAvailableMessage, LogLevel.Warning);
                }
                return result;
            }
        }
        private void Disconnect()
        {
            this.DisconnectImplementation();
            this.IsConnected = false;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.EnsureServiceIsDisconnected();
        }
    }
}
