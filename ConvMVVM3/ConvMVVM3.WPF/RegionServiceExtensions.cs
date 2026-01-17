using System;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Navigation.Abstractions;

namespace ConvMVVM3.WPF
{
    /// <summary>
    /// Extension methods for registering region management services.
    /// </summary>
    public static class RegionServiceExtensions
    {
        /// <summary>
        /// Adds the region manager service to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection with region manager registered.</returns>
        /// <exception cref="ArgumentNullException">services is null.</exception>
        public static IServiceRegistry AddRegionManager(this IServiceRegistry services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IRegionManager, ConvMVVM3.Core.Navigation.RegionManager>();
            return services;
        }
    }
}