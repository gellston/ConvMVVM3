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
    [Module("AModule", "1.0.0")]
    public class AModule : IModule
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
            registry.AddSingleton<AModuleView>("/SubContent/AModuleView");

            //ViewModels
            registry.AddSingleton<AModuleViewModel>();

        }
    }
}
