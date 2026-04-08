using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class NotifyCanExecuteChangedForAttribute : Attribute
    {
        #region Public Property
        public string[] PropertyNames { get; }
        #endregion

        #region Constructor

        public NotifyCanExecuteChangedForAttribute(params string[] propertyNames)
        {
            PropertyNames = propertyNames;
        }
        #endregion
    }
}