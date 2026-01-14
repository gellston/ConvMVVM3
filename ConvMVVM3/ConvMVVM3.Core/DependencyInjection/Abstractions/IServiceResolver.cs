using System;
using System.Collections.Generic;

namespace ConvMVVM3.Core.DependencyInjection.Abstractions
{
    public interface IServiceResolver
    {
        #region Public Functions
        object GetRequiredService(Type serviceType);
        T GetRequiredService<T>();

        // 관례: 없으면 null 반환 (reference type 기준)
        object GetService(Type serviceType);
        T GetService<T>();

        // 관례: 등록이 0개면 빈 IEnumerable 반환
        IEnumerable<T> GetServices<T>();
        IEnumerable<object> GetServices(Type serviceType);
        #endregion
    }
}
