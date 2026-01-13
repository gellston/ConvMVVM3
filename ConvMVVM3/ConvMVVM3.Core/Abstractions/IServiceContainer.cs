using System;

namespace ConvMVVM3.Core.Abstractions
{
    public interface IServiceContainer : IServiceResolver, IDisposable
    {
        IServiceScope CreateScope();
    }
}
