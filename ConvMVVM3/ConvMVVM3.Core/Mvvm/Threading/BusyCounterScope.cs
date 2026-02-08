using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConvMVVM3.Core.Mvvm.Threading
{
    public sealed class BusyCounterScope : IDisposable
    {
        private readonly Action<bool> _setBusy;
        private int _disposed;

        // “스코프 카운트”를 외부에서 보관(보통 VM 필드)
        private readonly Func<int> _getCount;
        private readonly Action<int> _setCount;

        private BusyCounterScope(Func<int> getCount, Action<int> setCount, Action<bool> setBusy)
        {
            _getCount = getCount;
            _setCount = setCount;
            _setBusy = setBusy;

            var c = _getCount() + 1;
            _setCount(c);
            if (c == 1) _setBusy(true);
        }

        public static BusyCounterScope Enter(
            Func<int> getCount,
            Action<int> setCount,
            Action<bool> setBusy)
            => new BusyCounterScope(getCount, setCount, setBusy);

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

            var c = _getCount() - 1;
            if (c < 0) c = 0;
            _setCount(c);
            if (c == 0) _setBusy(false);
        }
    }
}
