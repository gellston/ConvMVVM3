using System;

namespace ConvMVVM3.Core.DependencyInjection.Abstractions
{
    public interface IServiceContainer : IServiceResolver, IDisposable
    {
        #region  Public Functions
        IServiceScope CreateScope();
        #endregion
    }
}
