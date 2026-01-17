using System;
using System.Collections.Generic;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public interface IRegionViewRegistry
    {
        event EventHandler<RegionViewRegisteredEventArgs> ViewRegistered;

        void RegisterViewWithRegion(string regionName, Type viewType);

        IReadOnlyList<Type> GetViewTypes(string regionName);
    }
}
