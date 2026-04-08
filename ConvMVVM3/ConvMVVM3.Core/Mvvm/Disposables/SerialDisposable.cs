using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConvMVVM3.Core.Mvvm.Disposables
{
    public sealed class SerialDisposable : IDisposable
    {
        private readonly object _gate = new object();
        private IDisposable _current;
        private bool _disposed;

        public bool IsDisposed => Volatile.Read(ref _disposed);

        public IDisposable Disposable
        {
            get { lock (_gate) return _current; }
            set
            {
                IDisposable old;

                lock (_gate)
                {
                    if (_disposed)
                    {
                        // 이미 폐기된 상태면 새로 들어온 것도 즉시 폐기
                        value?.Dispose();
                        return;
                    }

                    old = _current;
                    _current = value;
                }

                old?.Dispose();
            }
        }

        public void Dispose()
        {
            IDisposable old;

            lock (_gate)
            {
                if (_disposed) return;
                _disposed = true;
                old = _current;
                _current = null;
            }

            old?.Dispose();
        }
    }
}
