using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Regions.Abstractions
{
    public interface IRegion : INotifyPropertyChanged
    {
        #region Public Property
        string Name { get; set; }

        RegionKind RegionKind {  get; set; }

        bool IsAttaced { get; set; }
        #endregion
    }
}
