using System;

namespace ConvMVVM3.Core.Abstractions
{
    public interface IServiceScope : IServiceResolver, IDisposable
    {
        IServiceResolver ServiceProvider { get; }
    }
}
