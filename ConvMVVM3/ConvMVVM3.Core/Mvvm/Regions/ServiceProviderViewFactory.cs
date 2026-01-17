using System;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public sealed class ServiceProviderViewFactory : IViewFactory
    {
        private readonly IServiceProvider _services;

        public ServiceProviderViewFactory(IServiceProvider services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            _services = services;
        }

        public object Create(Type viewType)
        {
            if (viewType == null) throw new ArgumentNullException(nameof(viewType));

            var obj = _services.GetService(viewType);
            if (obj != null) return obj;

            var created = Activator.CreateInstance(viewType);
            if (created == null)
                throw new InvalidOperationException("Could not create instance of " + viewType.FullName);

            return created;
        }
    }
}
