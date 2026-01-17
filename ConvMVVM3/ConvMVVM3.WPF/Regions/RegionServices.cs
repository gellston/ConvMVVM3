using ConvMVVM3.Core.DependencyInjection.Abstractions;
using System;

namespace ConvMVVM3.WPF.Regions
{
    /// <summary>
    /// Minimal service locator for regions.
    /// Set this once during app bootstrap to enable IoC-based adapter resolution without window-level wiring.
    /// </summary>
    public static class RegionServices
    {
        public static IServiceContainer Services { get; private set; }

        public static void SetServices(IServiceContainer services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            Services = services;
        }
    }
}
