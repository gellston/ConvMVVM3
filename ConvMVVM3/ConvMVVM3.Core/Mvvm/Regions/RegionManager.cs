using System;
using System.Collections.Generic;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public sealed class RegionManager : IRegionManager
    {
        private readonly Dictionary<string, IRegion> _regions = new Dictionary<string, IRegion>(StringComparer.OrdinalIgnoreCase);
        private readonly object _gate = new object();
        private readonly IViewFactory _viewFactory;
        private readonly bool _defaultSingleActive;

        public RegionManager(IViewFactory viewFactory, bool defaultSingleActive = false)
        {
            if (viewFactory == null) throw new ArgumentNullException(nameof(viewFactory));

            _viewFactory = viewFactory;
            _defaultSingleActive = defaultSingleActive;
            ViewRegistry = new RegionViewRegistry();

            ViewRegistry.ViewRegistered += OnViewRegistered;
        }

        public IRegionViewRegistry ViewRegistry { get; private set; }

        public IRegion GetOrCreateRegion(string regionName)
        {
            if (string.IsNullOrWhiteSpace(regionName)) throw new ArgumentException("Region name is required.", nameof(regionName));

            IRegion region;
            bool created = false;

            lock (_gate)
            {
                if (_regions.TryGetValue(regionName, out region))
                    return region;

                region = new Region(regionName, _defaultSingleActive);
                _regions[regionName] = region;
                created = true;
            }

            if (created)
            {
                foreach (var vt in ViewRegistry.GetViewTypes(regionName))
                {
                    var view = _viewFactory.Create(vt);
                    region.Add(view);
                }
            }

            return region;
        }

        public object Navigate(string regionName, Type viewType, IReadOnlyDictionary<string, object> parameters = null)
        {
            var region = GetOrCreateRegion(regionName);
            var view = _viewFactory.Create(viewType);

            var ctx = new NavigationContext(regionName, parameters);
            region.Activate(view, ctx);

            return view;
        }

        private void OnViewRegistered(object sender, RegionViewRegisteredEventArgs e)
        {
            IRegion region;
            lock (_gate)
            {
                if (!_regions.TryGetValue(e.RegionName, out region))
                    return;
            }

            var view = _viewFactory.Create(e.ViewType);
            region.Add(view);
        }
    }
}
