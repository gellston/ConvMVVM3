using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConvMVVM3.Core.Mvvm.Disposables
{
    public static class Disposable
    {
        public static IDisposable Create(Action action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            return new AnonymousDisposable(action);
        }

        public static IDisposable Empty { get; } = new EmptyDisposable();

        private sealed class EmptyDisposable : IDisposable
        {
            public void Dispose() { }
        }

        private sealed class AnonymousDisposable : IDisposable
        {
            private Action _action;

            public AnonymousDisposable(Action action) => _action = action;

            public void Dispose()
            {
                // 여러 번 Dispose돼도 action은 1회만 실행
                var a = Interlocked.Exchange(ref _action, null);
                a?.Invoke();
            }
        }
    }
}
