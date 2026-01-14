using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class NotifyPropertyChangedForAttribute : Attribute
    {
        public string[] PropertyNames { get; }

        public NotifyPropertyChangedForAttribute(params string[] propertyNames)
        {
            PropertyNames = propertyNames ?? throw new ArgumentNullException(nameof(propertyNames));
        }
    }
}
