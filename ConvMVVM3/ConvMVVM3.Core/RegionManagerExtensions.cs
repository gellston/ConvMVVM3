using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.Core.Mvvm.UIDispatcher.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core
{
    public static class RegionManagerExtensions
    {
        public static IServiceRegistry AddRegionManager(this IServiceRegistry services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IRegionManager, RegionManager>();
            return services;
        }
    }
}
