using System;
using System.Threading.Tasks;
using ConvMVVM3.Core.Mvvm.Abstractions;
using ConvMVVM3.Core.DependencyInjection.Abstractions;

namespace ConvMVVM3.WPF
{
    /// <summary>
    /// Extension methods for registering WPF UI dispatcher services.
    /// </summary>
    public static class UIDispatcherExtensions
    {
        /// <summary>
        /// Adds the WPF UI dispatcher service to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection with WPF UI dispatcher registered.</returns>
        /// <exception cref="ArgumentNullException">services is null.</exception>
        public static IServiceRegistry AddWPFUIDispatcher(this IServiceRegistry services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IUIDispatcher>(sp => 
            {
                var dispatcher = System.Windows.Application.Current?.Dispatcher;
                
                if (dispatcher == null)
                {
                    // WPF Application이 없으면 CurrentDispatcher 사용
                    dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                }
                
                return new WPFUIDispatcher(dispatcher);
            });
            return services;
        }
    }
}