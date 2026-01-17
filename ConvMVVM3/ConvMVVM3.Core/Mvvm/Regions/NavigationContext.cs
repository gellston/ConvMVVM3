using System;
using System.Collections.Generic;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public sealed class NavigationContext : INavigationContext
    {
        public NavigationContext(string regionName, IReadOnlyDictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(regionName))
                throw new ArgumentException("Region name is required.", nameof(regionName));

            RegionName = regionName;
            Parameters = parameters ?? new Dictionary<string, object>();
        }

        public string RegionName { get; private set; }

        public IReadOnlyDictionary<string, object> Parameters { get; private set; }
    }
}
