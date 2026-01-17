using System;
using System.Collections.Generic;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public interface IRegionManager
    {
        IRegionViewRegistry ViewRegistry { get; }

        IRegion GetOrCreateRegion(string regionName);

        object Navigate(string regionName, Type viewType, IReadOnlyDictionary<string, object> parameters = null);
    }
}
