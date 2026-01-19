using ConvMVVM3.Core.Mvvm.Commands.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Commands
{
    public sealed class RelayCommand : IRelayCommand
    {
        #region Private Property
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        #endregion



        #region Event

        public event EventHandler CanExecuteChanged;
        #endregion


        #region Constructor

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion


        #region Public Functions

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _execute();
            }
        }

        public void NotifyCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        #endregion
    }

    public sealed class RelayCommand<T> : IRelayCommand<T>
    {
        #region Private Property
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;
        #endregion

        #region Event

        public event EventHandler CanExecuteChanged;
        #endregion

        #region Constructor
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion

        #region Public Functions
        public bool CanExecute(object parameter)
        {
            // parameter가 null일 수 있음. value type이면 default(T)로 처리.
            T value = default(T);

            if (parameter != null)
            {
                if (!(parameter is T))
                    return false;

                value = (T)parameter;
            }

            return _canExecute == null ? true : _canExecute(value);
        }

        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            T value = default(T);
            if (parameter != null) value = (T)parameter;

            _execute(value);
        }

        public void NotifyCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        #endregion
    }
}
