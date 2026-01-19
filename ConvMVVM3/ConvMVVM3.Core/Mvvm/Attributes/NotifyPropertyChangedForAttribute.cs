using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class NotifyPropertyChangedForAttribute : Attribute
    {
        #region Public Property
        public string[] PropertyNames { get; }

        #endregion

        #region Constructor
        public NotifyPropertyChangedForAttribute(params string[] propertyNames)
        {
            PropertyNames = propertyNames;
        }

        #endregion


    }
}