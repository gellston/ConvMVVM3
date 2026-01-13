using System;

namespace ConvMVVM3.Core.Abstractions
{
    public interface IServiceResolver
    {
        object GetRequiredService(Type serviceType);
        T GetRequiredService<T>();
        object GetService(Type serviceType);
        T GetService<T>();
    }
}
