using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Regions.Abstractions
{
    public interface IRegionManager
    {

        #region Public Property
        ReadOnlyDictionary<string, IRegion> Regions { get;  }
        #endregion


        #region Public Functions
        void RegisterViewWithRegion(string name, RegionType regionType = RegionType.SingleView);
        void RegisterViewWithRegion(string name, Type viewType, RegionType regionType = RegionType.SingleView);
        void RegisterViewWithRegion<T>(string name, RegionType regionType = RegionType.SingleView);
        void RegisterViewWithRegion(string name, params Type[] types);

        void RegisterViewWithRegion(string name, string typeName, RegionType regionType = RegionType.SingleView);
        void RegisterViewWithRegion(string name, params string[] typeNames);



        void RequestNavigate(string name, Type viewType, NavigationParameters parameters = null, Action<NavigationContext> result = null);

        void RequestNavigate<T>(string name, NavigationParameters parameters = null, Action<NavigationContext> result = null);

        void RequestNavigate(string name, string typeName, NavigationParameters parameters = null, Action<NavigationContext> result = null);
        void RequestNavigate(string name, string[] typeNames);
        void RequestNavigate(string name, string typeName, int repeat);

        void ClearRegion(string name);

        void Active(string name, object view);

        void Active(string name, string viewName);
        #endregion
    }
}
