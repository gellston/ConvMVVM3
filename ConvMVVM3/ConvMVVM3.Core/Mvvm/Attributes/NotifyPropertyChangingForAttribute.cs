using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class NotifyPropertyChangingForAttribute : Attribute
    {
        public string[] PropertyNames { get; }

        public NotifyPropertyChangingForAttribute(params string[] propertyNames)
        {
            PropertyNames = propertyNames;
        }
    }
}