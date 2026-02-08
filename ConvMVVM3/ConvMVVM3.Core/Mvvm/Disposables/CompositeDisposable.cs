using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConvMVVM3.Core.Mvvm.Disposables
{
    public sealed class CompositeDisposable : IDisposable
    {
        private readonly object _gate = new object();
        private List<IDisposable> _items = new List<IDisposable>();
        private bool _disposed;

        public bool IsDisposed => Volatile.Read(ref _disposed);

        public void Add(IDisposable disposable)
        {
            if (disposable is null) throw new ArgumentNullException(nameof(disposable));

            lock (_gate)
            {
                if (_disposed)
                {
                    // 이미 폐기된 묶음에 추가되면 즉시 폐기 (누수 방지)
                    disposable.Dispose();
                    return;
                }

                _items.Add(disposable);
            }
        }

        public bool Remove(IDisposable disposable)
        {
            if (disposable is null) return false;

            lock (_gate)
            {
                if (_disposed) return false;
                return _items.Remove(disposable);
            }
        }

        public void Clear()
        {
            List<IDisposable> toDispose;

            lock (_gate)
            {
                if (_disposed) return;
                toDispose = _items;
                _items = new List<IDisposable>();
            }

            // 락 밖에서 Dispose
            foreach (var d in toDispose)
            {
                try { d.Dispose(); }
                catch { /* 필요하면 로깅/집계 */ }
            }
        }

        public void Dispose()
        {
            List<IDisposable> toDispose;

            lock (_gate)
            {
                if (_disposed) return;
                _disposed = true;
                toDispose = _items;
                _items = null;
            }

            if (toDispose is null) return;

            // 역순 Dispose가 더 안전한 케이스가 많음(스택처럼)
            for (int i = toDispose.Count - 1; i >= 0; i--)
            {
                try { toDispose[i].Dispose(); }
                catch { /* 필요하면 로깅/집계 */ }
            }
        }
    }
}
