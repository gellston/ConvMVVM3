using ConvMVVM3.Core;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.WPF.Regions;
using System;

namespace ConvMVVM3.WPF
{
    public static class WPFRegionManagerExtensions
    {
        /// <summary>Registers region management with WPF View/DataContext navigation participation.</summary>
        public static IServiceRegistry AddWPFRegionManager(this IServiceRegistry services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.AddSingleton<INavigationAwareResolver, WPFNavigationAwareResolver>();
            return services.AddRegionManager();
        }
    }
}
