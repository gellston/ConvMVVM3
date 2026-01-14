using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ConvMVVM3.Core.Mvvm.Commands.Abstractions
{
    public interface IRelayCommand : ICommand
    {
        void NotifyCanExecuteChanged();
    }

    public interface IRelayCommand<T> : ICommand
    {
        void NotifyCanExecuteChanged();
    }
}
