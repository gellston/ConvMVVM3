using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ConvMVVM3.Core.Mvvm
{
    public abstract class ObservableObject : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanging;
            if (handler != null)
                handler(this, new PropertyChangingEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(
            ref T storage,
            T value,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            OnPropertyChanging(propertyName);
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<T>(
            ref T storage,
            T value,
            Action onChanged,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            OnPropertyChanging(propertyName);
            storage = value;

            if (onChanged != null)
                onChanged();

            OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<T>(
            ref T storage,
            T value,
            IEqualityComparer<T> comparer,
            [CallerMemberName] string propertyName = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default;

            if (comparer.Equals(storage, value))
                return false;

            OnPropertyChanging(propertyName);
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void NotifyPropertiesChanging(params string[] propertyNames)
        {
            if (propertyNames == null) return;

            for (int i = 0; i < propertyNames.Length; i++)
            {
                var name = propertyNames[i];
                if (!string.IsNullOrWhiteSpace(name))
                    OnPropertyChanging(name);
            }
        }

        protected void NotifyPropertiesChanged(params string[] propertyNames)
        {
            if (propertyNames == null) return;

            for (int i = 0; i < propertyNames.Length; i++)
            {
                var name = propertyNames[i];
                if (!string.IsNullOrWhiteSpace(name))
                    OnPropertyChanged(name);
            }
        }
    }
}
