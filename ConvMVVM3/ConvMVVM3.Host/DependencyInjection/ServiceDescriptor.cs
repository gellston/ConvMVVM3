using ConvMVVM3.Core.DependencyInjection.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Host.DependencyInjection
{
    public sealed class ServiceDescriptor
    {
        #region Public Functions
        public Type ServiceType { get; set; }
        public Type ImplementationType { get; set; }
        public Func<IServiceResolver, object> Factory { get; set; }
        public ServiceLifetime Lifetime { get; set; }
        #endregion
    }
}
