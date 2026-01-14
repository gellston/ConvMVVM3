using System;

namespace ConvMVVM3.Core.DependencyInjection.Abstractions
{
    public interface IServiceScope : IServiceResolver, IDisposable
    {
        #region Public Functions
        IServiceResolver ServiceProvider { get; }
        #endregion
    }
}
