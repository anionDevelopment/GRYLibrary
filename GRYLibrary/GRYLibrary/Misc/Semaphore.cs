using System.Threading;
using GRYLibrary.Core.Exceptions;

namespace GRYLibrary.Core.Misc
{
    public class Semaphore
    {
        private readonly object _LockObject = new object();
        private bool _Semaphore = true;//true=up=usable, false=down=locked
        /// <summary>
        /// Waits until the semaphore is open again
        /// </summary>
        /// <remarks>
        /// The waiting is done using <see cref="Monitor.Wait(object)"/> because that releases the monitor while waiting.
        /// Busy-waiting inside the lock-block would block <see cref="Unlock"/> (which needs the same monitor) and therefore deadlock.
        /// </remarks>
        public void Lock()
        {
            lock (this._LockObject)
            {
                while (!this._Semaphore)
                {
                    Monitor.Wait(this._LockObject);
                }
                this._Semaphore = false;
            }
        }
        public bool IsLocked()
        {
            lock (this._LockObject)
            {
                return !this._Semaphore;
            }

        }
        public void Unlock()
        {
            lock (this._LockObject)
            {
                if (this._Semaphore)
                {
                    throw new InternalAlgorithmException("Can not unlock an unlocked semaphore.");
                }
                else
                {
                    this._Semaphore = true;
                    Monitor.Pulse(this._LockObject);
                }
            }
        }
    }
}
