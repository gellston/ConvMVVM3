using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public class Region : IRegion
    {
        #region Private Property
        private string _Name = "";
        private bool _IsAttached = false;
        private RegionKind _RegionKind = RegionKind.Unknown;
        private ObservableCollection<object> _Items = new ObservableCollection<object>();
        #endregion

        #region Collection
        public ObservableCollection<object> Items
        {
            get => _Items;
        }
        #endregion


        #region Public Property
        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                this.OnPropertyChaned();
            }
        }

        public RegionKind RegionKind
        {
            get => _RegionKind;
            set => _RegionKind = value;
        }

        public bool IsAttaced
        {
            get => _IsAttached;
            set => _IsAttached = value;
        }
        #endregion

        #region Event

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Protected Functions
        protected void OnPropertyChaned([CallerMemberName]string propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        } 
        #endregion
    }
}
