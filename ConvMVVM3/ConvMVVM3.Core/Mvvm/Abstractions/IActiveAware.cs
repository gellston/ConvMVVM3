using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Abstractions
{
    public interface IPrimaryAware
    {
        #region Public Property
        bool IsPrimary { get; set; }
        #endregion

        #region Event
        event EventHandler IsPrimaryChanged;
        #endregion
    }
}
