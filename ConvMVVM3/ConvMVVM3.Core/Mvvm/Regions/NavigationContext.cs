using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public class NavigationContext
    {
        #region Public Property

        public NavigationParameters Parameters { get; set; } = new NavigationParameters();
        #endregion
    }
}
