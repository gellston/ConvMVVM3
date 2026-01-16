using ConvMVVM3.Core.Mvvm.Commands.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConvMVVM3.Core.Mvvm.Commands
{
    public sealed class AsyncRelayCommand : IAsyncRelayCommand
    {
        private readonly Func<CancellationToken, Task> _execute;
        private readonly Func<bool> _canExecute;

        private readonly bool _allowConcurrent;
        private readonly bool _disableWhileRunning;

        private CancellationTokenSource _cts;
        private int _running; // 0/1

        public event EventHandler CanExecuteChanged;

        public bool IsRunning
        {
            get { return _running == 1; }
        }

        public bool IsExecuting
        {
            get { return IsRunning; }
        }

        public AsyncRelayCommand(
            Func<CancellationToken, Task> execute,
            Func<bool> canExecute = null,
            bool allowConcurrentExecutions = false,
            bool disableWhileRunning = true)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
            _allowConcurrent = allowConcurrentExecutions;
            _disableWhileRunning = disableWhileRunning;
        }

        public AsyncRelayCommand(
            Func<Task> execute,
            Func<bool> canExecute = null,
            bool allowConcurrentExecutions = false,
            bool disableWhileRunning = true)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            // Convert Func<Task> to Func<CancellationToken, Task> that ignores the token
            _execute = (ct) => execute();
            _canExecute = canExecute;
            _allowConcurrent = allowConcurrentExecutions;
            _disableWhileRunning = disableWhileRunning;
        }

        public bool CanExecute(object parameter)
        {
            if (_disableWhileRunning && IsRunning)
                return false;

            return _canExecute == null ? true : _canExecute();
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter).ConfigureAwait(false);
        }

        public Task ExecuteAsync(object parameter)
        {
            return ExecuteInternalAsync();
        }

        public void Cancel()
        {
            var cts = _cts;
            if (cts != null)
            {
                try { cts.Cancel(); } catch { }
            }
        }

        public void NotifyCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private async Task ExecuteInternalAsync()
        {
            if (!_allowConcurrent)
            {
                if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
                    return;
            }
            else
            {
                Interlocked.Exchange(ref _running, 1);
            }

            if (_disableWhileRunning)
                NotifyCanExecuteChanged();

            var localCts = new CancellationTokenSource();
            _cts = localCts;

            try
            {
                await _execute(localCts.Token).ConfigureAwait(false);
            }
            finally
            {
                _cts = null;
                localCts.Dispose();
                Interlocked.Exchange(ref _running, 0);

                if (_disableWhileRunning)
                    NotifyCanExecuteChanged();
            }
        }
    }

    public sealed class AsyncRelayCommand<T> : IAsyncRelayCommand<T>
    {
        private readonly Func<T, CancellationToken, Task> _execute;
        private readonly Func<T, bool> _canExecute;

        private readonly bool _allowConcurrent;
        private readonly bool _disableWhileRunning;

        private CancellationTokenSource _cts;
        private int _running;

        public event EventHandler CanExecuteChanged;

        public bool IsRunning
        {
            get { return _running == 1; }
        }

        public AsyncRelayCommand(
            Func<T, CancellationToken, Task> execute,
            Func<T, bool> canExecute = null,
            bool allowConcurrentExecutions = false,
            bool disableWhileRunning = true)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
            _allowConcurrent = allowConcurrentExecutions;
            _disableWhileRunning = disableWhileRunning;
        }

        public bool CanExecute(object parameter)
        {
            if (_disableWhileRunning && IsRunning)
                return false;

            T value = default(T);

            if (parameter != null)
            {
                if (!(parameter is T))
                    return false;

                value = (T)parameter;
            }

            return _canExecute == null ? true : _canExecute(value);
        }

        public async void Execute(object parameter)
        {
            T value = default(T);
            if (parameter != null) value = (T)parameter;

            await ExecuteAsync(value).ConfigureAwait(false);
        }

        public Task ExecuteAsync(T parameter)
        {
            return ExecuteInternalAsync(parameter);
        }

        public void Cancel()
        {
            var cts = _cts;
            if (cts != null)
            {
                try { cts.Cancel(); } catch { }
            }
        }

        public void NotifyCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private async Task ExecuteInternalAsync(T parameter)
        {
            if (!_allowConcurrent)
            {
                if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
                    return;
            }
            else
            {
                Interlocked.Exchange(ref _running, 1);
            }

            if (_disableWhileRunning)
                NotifyCanExecuteChanged();

            var localCts = new CancellationTokenSource();
            _cts = localCts;

            try
            {
                await _execute(parameter, localCts.Token).ConfigureAwait(false);
            }
            finally
            {
                _cts = null;
                localCts.Dispose();
                Interlocked.Exchange(ref _running, 0);

                if (_disableWhileRunning)
                    NotifyCanExecuteChanged();
            }
        }
    }
}
