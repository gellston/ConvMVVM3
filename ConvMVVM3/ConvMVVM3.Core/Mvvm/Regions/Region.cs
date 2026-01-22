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
        private ObservableCollection<object> _Views = new ObservableCollection<object>();
        private object _SelectedItem = null;
        private object _Content = null;
        private NavigationContext _NavigationContext = null;
        private RegionType _RegionType = RegionType.SingleView;
        #endregion

        #region Collection
        public ObservableCollection<object> Views
        {
            get => _Views;
        }
        #endregion


        #region Public Property
        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
            }
        }

        public bool IsAttaced
        {
            get => _IsAttached;
            set => _IsAttached = value;
        }

        public object SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                this.OnPropertyChaned();
            }
        }

        public NavigationContext NavigationContext
        {
            get => _NavigationContext;
            set
            {
                _NavigationContext = value;
            }
        }

        public object Content
        {
            get => _Content;
            set
            {
                _Content = value;
                this.OnPropertyChaned();
            }
        }

        public RegionType RegionType
        {
            get => _RegionType;
            set
            {
                _RegionType = value;
            }
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
