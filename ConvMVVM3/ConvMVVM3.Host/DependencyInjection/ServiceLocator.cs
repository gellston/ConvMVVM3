using ConvMVVM3.Core.DependencyInjection.Abstractions;
using System;

namespace ConvMVVM3.Host.DependencyInjection
{
    /// <summary>
    /// Singleton service locator for global DI container access.
    /// Provides access to the DI container from anywhere in the application.
    /// Similar to Prism's ContainerLocator pattern.
    /// </summary>
    public static class ServiceLocator
    {
        private static IServiceContainer _container;
        private static readonly object _lock = new object();

        /// <summary>
        /// Initializes the service locator with a DI container.
        /// This should be called once at application startup.
        /// </summary>
        /// <param name="container">The DI container to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when container is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when already initialized.</exception>
        public static void Initialize(IServiceContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            lock (_lock)
            {
                if (_container != null)
                    throw new InvalidOperationException("ServiceLocator is already initialized.");

                _container = container;
            }
        }

        /// <summary>
        /// Gets the DI container instance.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when not initialized.</exception>
        public static IServiceContainer Container
        {
            get
            {
                if (_container == null)
                    throw new InvalidOperationException("ServiceLocator is not initialized. Call Initialize() first.");

                return _container;
            }
        }

        /// <summary>
        /// Gets a service from the DI container.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <returns>The service instance.</returns>
        public static T GetService<T>() => Container.GetRequiredService<T>();

        /// <summary>
        /// Gets a service from the DI container by type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The service instance, or null if not found.</returns>
        public static object GetService(Type serviceType) => Container.GetService(serviceType);
    }
}