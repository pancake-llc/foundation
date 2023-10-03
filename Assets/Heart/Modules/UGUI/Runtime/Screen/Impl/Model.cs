using System;

namespace Pancake.UI
{
    public abstract class Model : IDisposable
    {
        private bool _isDisposed;

        public void Dispose()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(Model));

            DisposeInternal();
            _isDisposed = true;
        }

        protected abstract void DisposeInternal();
    }
}