using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Modules.Abstractions
{
    public interface IModule
    {
        #region Pulibc Functions
        void RegisterServices(IServiceRegistry registry);
        void ConfigureRegions(IRegionManager regionManager);
        void OnInitialized(IServiceResolver serviecResolver);
        #endregion
    }
}
