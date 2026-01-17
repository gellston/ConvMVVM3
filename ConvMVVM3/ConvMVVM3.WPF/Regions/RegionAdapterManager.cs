using ConvMVVM3.WPF.Abstractions;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ConvMVVM3.WPF.Regions
{
    /// <summary>
    /// Resolves an IRegionAdapter for a given WPF control.
    /// Typically registered as a Singleton in IoC, so you can add adapters at runtime.
    /// </summary>
    public sealed class RegionAdapterManager
    {
        private readonly List<IRegionAdapter> _adapters = new List<IRegionAdapter>();

        public RegionAdapterManager()
        {
        }

        public RegionAdapterManager(IEnumerable<IRegionAdapter> adapters)
        {
            if (adapters == null) throw new ArgumentNullException(nameof(adapters));
            _adapters.AddRange(adapters);
        }

        public void RegisterAdapter(IRegionAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            _adapters.Add(adapter);
        }

        public void RegisterAdapterFirst(IRegionAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            _adapters.Insert(0, adapter);
        }

        public IRegionAdapter GetAdapter(DependencyObject target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var t = target.GetType();

            for (int i = 0; i < _adapters.Count; i++)
            {
                var a = _adapters[i];
                if (a.TargetType.IsAssignableFrom(t))
                    return a;
            }

            throw new InvalidOperationException("No region adapter registered for target type '" + t.FullName + "'.");
        }
    }
}
