using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GUtilities = GRYLibrary.Core.Misc.Utilities;
using System.Threading.Tasks;
using System.Threading;
using System;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.Extensions.Logging;

namespace GRYLibrary.Core.APIServer.BaseServices
{
    public abstract class IteratingBackgroundService : IDisposable
    {
        public bool Enabled { get; set; }
        private bool _Running;
        private readonly object _Lock = new object();
        private bool _Disposed = false;

        protected bool Running
        {
            get
            {
                lock (this._Lock)
                {
                    return this._Running;
                }
            }
            private set
            {
                lock (this._Lock)
                {
                    this._Running = value;
                }
            }
        }
        public TimeSpan AdditionalDelay { get; set; } = TimeSpan.FromSeconds(0);
        protected readonly IGRYLog _Logger;
        private readonly ExecutionMode _ExecutionMode;
        protected abstract void Run();
        public IteratingBackgroundService(ExecutionMode executionMode, IGRYLog logger)
        {
            this._ExecutionMode = executionMode;
            this._Logger = logger;
        }
        public void StartAsync()
        {
            lock (this._Lock)
            {
                if (this.Running)
                {
                    this._Logger.Log($"Background-service {this.GetType().Name} was already started.", LogLevel.Information);
                    return;
                }

                this.Running = true;
                if (this.ShouldBeExecuted())
                {
                    this._Logger.Log($"Background-service {this.GetType().Name} will be started.", LogLevel.Information);
                    Task task = Task.Run(() =>
                    {
                        Thread.CurrentThread.Name = this.GetType().Name;
                        while (this.Enabled)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(1));
                            Thread.Sleep(this.AdditionalDelay);//TODO make this interrupt if Enabled==false
                            if (this.Enabled)
                            {
                                this._Logger.Log($"Execute {this.GetType().Name}", LogLevel.Debug, false, false, true, false, false, this.Run);
                            }
                        }
                        this.Running = false;
                    });
                }
                else
                {
                    this._Logger.Log($"Background-service {this.GetType().Name} will not be started.", LogLevel.Information);
                }
            }
        }
        public async Task Stop()
        {
            if (this.Running)
            {
                this._Logger.Log($"Background-service {this.GetType().Name} will be stopped.", LogLevel.Information);
                this.Enabled = false;
                await GUtilities.WaitUntilConditionIsTrueAsync(() => !this.Running, $"Stop backgroundservice {this.GetType().Name}");
                this.Dispose();
                this._Logger.Log($"Background-service {this.GetType().Name} is now stopped.", LogLevel.Information);
            }
        }
        public void StopAndWait()
        {
            this.Stop().Wait();
        }
        public bool ShouldBeExecuted()
        {
            if (this._ExecutionMode is not RunProgram)
            {
                return false;
            }
            if (!this.Enabled)
            {
                return false;
            }
            return true;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (this._Disposed)
            {
                return;
            }

            if (disposing)
            {
                this.Stop();
            }

            this._Disposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~IteratingBackgroundService()
        {
            this.Dispose(false);
        }
    }
}
