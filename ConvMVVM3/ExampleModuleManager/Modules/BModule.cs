using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Modules.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ExampleModuleManager.ViewModels;
using ExampleModuleManager.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExampleModuleManager.Modules
{
    [Module("BModule", "1.0.0")]
    public class BModule : IModule
    {
        public void ConfigureRegions(IRegionManager regionManager)
        {

        }

        public void OnInitialized(IServiceResolver serviecResolver)
        {

        }

        public void RegisterServices(IServiceRegistry registry)
        {
            //Views
            registry.AddSingleton<BModuleView>("/SubContent/BModuleView");

            //ViewModels
            registry.AddSingleton<BModuleViewModel>();
        }
    }
}
