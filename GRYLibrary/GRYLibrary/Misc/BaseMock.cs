using System;

namespace GRYLibrary.Core.Misc
{
    public abstract class BaseMock : IDisposable
    {
        private bool _Disposed;
        public bool VerifyCallsOnDispose { get; }
        protected abstract void VerifyCalls();
        public BaseMock(bool verifyCallsOnDispose)
        {
            this.VerifyCallsOnDispose = verifyCallsOnDispose;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                    if (this.VerifyCallsOnDispose)
                    {
                        this.VerifyCalls();
                    }
                }
            }
            //dispose unmanaged resources
            this._Disposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
