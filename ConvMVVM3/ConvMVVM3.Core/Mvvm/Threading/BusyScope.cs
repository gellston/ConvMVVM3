using ConvMVVM3.Core.Mvvm.Threading.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConvMVVM3.Core.Mvvm.Threading
{
    public sealed class BusyScope : IDisposable
    {
        private readonly IBusyState _target;
        private readonly Action<bool> _setBusy;   // VM이 IBusyState 안 쓰면 이걸로도 가능
        private int _disposed;

        private BusyScope(IBusyState target)
        {
            _target = target;
            _target.IsBusy = true;
        }

        private BusyScope(Action<bool> setBusy)
        {
            _setBusy = setBusy;
            _setBusy(true);
        }

        public static BusyScope Enter(IBusyState target)
            => new BusyScope(target);

        public static BusyScope Enter(Action<bool> setBusy)
            => new BusyScope(setBusy);

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;
            if (_setBusy != null) _setBusy(false);
            else _target.IsBusy = false;
        }
    }
}
