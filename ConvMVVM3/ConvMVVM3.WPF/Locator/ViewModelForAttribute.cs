using System;

namespace ConvMVVM3.WPF.Locator
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ViewModelForAttribute : Attribute
    {
        public ViewModelForAttribute(Type viewModelType)
        {
            if (viewModelType == null) throw new ArgumentNullException(nameof(viewModelType));
            ViewModelType = viewModelType;
        }

        public Type ViewModelType { get; private set; }
    }
}
