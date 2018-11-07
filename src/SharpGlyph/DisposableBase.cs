using System;

namespace SharpGlyph
{
    public abstract class DisposableBase : IDisposable
    {
        private bool _disposed;

        ~DisposableBase()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeCore();
            }

            _disposed = true;
        }

        protected virtual void DisposeCore()
        {

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}