using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConvMVVM3.Core.Mvvm.Commands.Abstractions
{
    public interface IAsyncRelayCommand : ICommand
    {
        bool IsRunning { get; }
        void Cancel();
        Task ExecuteAsync(object parameter);
        void NotifyCanExecuteChanged();
    }

    public interface IAsyncRelayCommand<T> : ICommand
    {
        bool IsRunning { get; }
        void Cancel();
        Task ExecuteAsync(T parameter);
        void NotifyCanExecuteChanged();
    }
}
