using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConvMVVM3.Core.Mvvm.Threading
{
    public sealed class OperationGuard
    {
        private int _running; // 0=idle, 1=running

        public bool IsRunning => Volatile.Read(ref _running) == 1;

        public async Task RunAsync(Func<Task> operation)
        {
            if (operation is null) throw new ArgumentNullException(nameof(operation));
            if (Interlocked.CompareExchange(ref _running, 1, 0) != 0) return;

            try
            {
                await operation().ConfigureAwait(true);
            }
            finally
            {
                Volatile.Write(ref _running, 0);
            }
        }

        public async Task<T> RunAsync<T>(Func<Task<T>> operation)
        {
            if (operation is null) throw new ArgumentNullException(nameof(operation));
            if (Interlocked.CompareExchange(ref _running, 1, 0) != 0) return default;

            try
            {
                return await operation().ConfigureAwait(true);
            }
            finally
            {
                Volatile.Write(ref _running, 0);
            }
        }
    }
}
