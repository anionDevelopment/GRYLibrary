using System;
using System.Threading;

namespace GRYLibrary.Core.Misc
{
    /// <summary>
    /// Grants exclusive access to a resource for the entire duration of an operation: whoever needs the resource waits until the current user is finished with it.
    /// </summary>
    /// <remarks>
    /// The difference to a plain <see cref="SemaphoreSlim"/> is that this class is re-entrant for the owning thread. A thread which already has the access can
    /// enter again without deadlocking itself, which is required whenever an operation on the protected resource calls another operation on the same resource.
    /// The difference to a <see langword="lock"/>-statement is that the protected resource can be replaced while it is protected, because the access is bound to
    /// this object and not to the resource-instance.
    /// </remarks>
    public sealed class ExclusiveAccess : IDisposable
    {
        private readonly SemaphoreSlim _Semaphore = new SemaphoreSlim(1, 1);
        private readonly object _StateLock = new object();
        private int _OwnerThreadId = NoOwner;
        private uint _RecursionCount = 0;
        private const int NoOwner = 0;

        /// <summary>Runs <paramref name="action"/> with exclusive access. Waits until the access is available.</summary>
        public void Run(Action action)
        {
            this.Run<object?>(() =>
            {
                action();
                return null;
            });
        }

        /// <summary>Runs <paramref name="function"/> with exclusive access and returns its result. Waits until the access is available.</summary>
        public T Run<T>(Func<T> function)
        {
            this.Enter();
            try
            {
                return function();
            }
            finally
            {
                this.Exit();
            }
        }

        /// <summary>Indicates whether the current thread currently has the exclusive access.</summary>
        public bool IsOwnedByCurrentThread()
        {
            lock (this._StateLock)
            {
                return this._OwnerThreadId == Environment.CurrentManagedThreadId;
            }
        }

        private void Enter()
        {
            lock (this._StateLock)
            {
                if (this._OwnerThreadId == Environment.CurrentManagedThreadId)
                {
                    // This thread already has the access, so it must not wait for itself.
                    this._RecursionCount = this._RecursionCount + 1;
                    return;
                }
            }
            this._Semaphore.Wait();
            lock (this._StateLock)
            {
                // No other thread can be here because the semaphore is taken, and this thread can not enter concurrently with itself.
                this._OwnerThreadId = Environment.CurrentManagedThreadId;
                this._RecursionCount = 1;
            }
        }

        private void Exit()
        {
            bool releaseRequired;
            lock (this._StateLock)
            {
                this._RecursionCount = this._RecursionCount - 1;
                releaseRequired = this._RecursionCount == 0;
                if (releaseRequired)
                {
                    this._OwnerThreadId = NoOwner;
                }
            }
            if (releaseRequired)
            {
                this._Semaphore.Release();
            }
        }

        public void Dispose()
        {
            this._Semaphore.Dispose();
        }
    }
}
