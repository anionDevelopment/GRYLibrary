using System;
using System.Collections.Generic;
using System.Threading;

namespace GRYLibrary.Core.Misc
{
    public sealed class MicroTaskExecutorService<T> : IDisposable
    {
        public bool IsRunning { get; private set; }
        private bool _Enabled;
        private Thread _Thread = null;
        private readonly Action<T> _Action;
        private readonly Func<IEnumerable<T>> _GetActions;
        private readonly TimeSpan _WaitInterval = TimeSpan.FromMilliseconds(50);
        private readonly object _Lock = new object();
        public MicroTaskExecutorService(Func<IEnumerable<T>> getActions, Action<T> action)
        {
            this._Action = action;
            this._GetActions = getActions;
            this.IsRunning = false;
            this._Enabled = false;
        }

        public void Start()
        {
            lock (this._Lock)
            {
                if (!this.IsRunning)
                {
                    this._Enabled = true;
                    this.IsRunning = true;
                    this._Thread = new Thread(this.Do);
                    this._Thread.Start();
                }
            }
        }
        private void Do()
        {
            while (this.IsRunning)
            {
                Thread.Sleep(50);
                lock (this._Lock)
                {
                    if (!this.IsStillEnabled())
                    {
                        return;
                    }

                    IEnumerable<T> actions = this._GetActions();
                    if (actions.Count() == 0)
                    {
                        Thread.Sleep(this._WaitInterval);
                    }
                    else
                    {
                        foreach (T microaction in actions)
                        {
                            if (this.IsStillEnabled())
                            {
                                this._Action(microaction);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        private bool IsStillEnabled()
        {
            lock (this._Lock)
            {
                if (this._Enabled)
                {
                    return true;
                }
                else
                {
                    this.IsRunning = false;
                    return false;
                }
            }
        }

        /// <remarks>
        /// This method is threadsafe.
        /// </remarks>
        public void Stop()
        {
            lock (this._Lock)
            {
                this._Enabled = false;
                while (this.IsRunning)
                {
                    Thread.Sleep(this._WaitInterval);
                }
                this._Thread = null;
            }
        }
        public void Dispose()
        {
            this.Stop();
        }
    }
}
