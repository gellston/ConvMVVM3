using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Regions.Abstractions
{
    public interface IRegion : INotifyPropertyChanged
    {
        #region Public Property
        string Name { get; set; }

        bool IsAttaced { get; set; }

        object SelectedItem { get; set; }

        object Content {  get; set; }

        NavigationContext NavigationContext { get; set; }

        RegionType RegionType { get; set; }
        #endregion

        #region Collection
        ObservableCollection<object> Views
        {
            get;
        }
        #endregion
    }
}
