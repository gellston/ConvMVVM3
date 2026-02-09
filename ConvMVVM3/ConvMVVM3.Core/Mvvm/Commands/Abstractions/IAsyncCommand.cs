using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConvMVVM3.Core.Mvvm.Commands.Abstractions
{
    public interface IAsyncRelayCommand : ICommand
    {
        #region Public Property
        bool IsRunning { get; }
        #endregion

        #region Public Functions
        void Cancel();
        Task ExecuteAsync(object parameter);
        void NotifyCanExecuteChanged();
        #endregion
    }

    public interface IAsyncRelayCommand<T> : ICommand
    {
        #region Public Property
        bool IsRunning { get; }

        #endregion

        #region Public Functions
        void Cancel();
        Task ExecuteAsync(T parameter);
        void NotifyCanExecuteChanged();

        #endregion
    }
}
