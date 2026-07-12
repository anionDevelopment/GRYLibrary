using GRYLibrary.Core.Exceptions;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.Misc
{
    public class Semaphore
    {
        private readonly object _LockObject = new object();
        private bool _Semaphore = true;//true=up=usable, false=down=locked
        /// <summary>
        /// Waits until the semaphore is open again
        /// </summary>
        public void Lock()
        {
            lock (this._LockObject)
            {
                GUtilities.WaitUntilConditionIsTrue(() => this._Semaphore, "Wait-until-semaphore-opens");
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

                }
            }
        }
    }
}
