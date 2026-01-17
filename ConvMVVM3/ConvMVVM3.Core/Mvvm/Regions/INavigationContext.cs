using System.Collections.Generic;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public interface INavigationContext
    {
        string RegionName { get; }
        IReadOnlyDictionary<string, object> Parameters { get; }
    }
}
