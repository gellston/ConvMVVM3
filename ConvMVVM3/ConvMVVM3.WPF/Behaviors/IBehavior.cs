using System.Windows;
using System.Windows.Controls;

namespace ConvMVVM3.WPF.Behaviors
{
    /// <summary>
    /// 행동 인터페이스
    /// </summary>
    public interface IBehavior
    {
        void Attach(DependencyObject associatedObject);
        void Detach();
    }

    /// <summary>
    /// 기본 행동 클래스
    /// </summary>
    public abstract class Behavior<T> : IBehavior where T : DependencyObject
    {
        private T _associatedObject;

        public T AssociatedObject
        {
            get => _associatedObject;
            private set
            {
                if (_associatedObject != null)
                {
                    Detach();
                }
                _associatedObject = value;
                Attach(_associatedObject);
            }
        }

        void IBehavior.Attach(DependencyObject associatedObject)
        {
            Attach((T)associatedObject);
        }

        public abstract void Attach(T associatedObject);
        public abstract void Detach();

        protected virtual void OnAttached() { }
        protected virtual void OnDetaching() { }
    }
}