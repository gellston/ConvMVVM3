using System;
using System.Collections.Generic;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public sealed class RegionViewRegistry : IRegionViewRegistry
    {
        private readonly Dictionary<string, List<Type>> _map = new Dictionary<string, List<Type>>(StringComparer.OrdinalIgnoreCase);
        private readonly object _gate = new object();

        public event EventHandler<RegionViewRegisteredEventArgs> ViewRegistered;

        public void RegisterViewWithRegion(string regionName, Type viewType)
        {
            if (string.IsNullOrWhiteSpace(regionName)) throw new ArgumentException("Region name is required.", nameof(regionName));
            if (viewType == null) throw new ArgumentNullException(nameof(viewType));

            lock (_gate)
            {
                List<Type> list;
                if (!_map.TryGetValue(regionName, out list))
                {
                    list = new List<Type>();
                    _map[regionName] = list;
                }
                if (!list.Contains(viewType))
                    list.Add(viewType);
            }

            var handler = ViewRegistered;
            if (handler != null)
                handler(this, new RegionViewRegisteredEventArgs(regionName, viewType));
        }

        public IReadOnlyList<Type> GetViewTypes(string regionName)
        {
            if (string.IsNullOrWhiteSpace(regionName)) return new Type[0];

            lock (_gate)
            {
                List<Type> list;
                if (_map.TryGetValue(regionName, out list))
                    return list.ToArray();
                return new Type[0];
            }
        }
    }
}
