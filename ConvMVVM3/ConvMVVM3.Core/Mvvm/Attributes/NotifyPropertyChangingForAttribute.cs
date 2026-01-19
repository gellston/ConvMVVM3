using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class NotifyPropertyChangingForAttribute : Attribute
    {
        #region Public Proerty
        public string[] PropertyNames { get; }

        #endregion


        #region Constructor

        public NotifyPropertyChangingForAttribute(params string[] propertyNames)
        {
            PropertyNames = propertyNames;
        }
        #endregion
    }
}