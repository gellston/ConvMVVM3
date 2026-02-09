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

    [Module("MainModule", "1.0.0")]
    public class MainModule : IModule
    {
        public void ConfigureRegions(IRegionManager regionManager)
        {
            regionManager.RegisterViewWithRegion<MainContentView>("MainContent");
        }

        public void OnInitialized(IServiceResolver serviecResolver)
        {

        }

        public void RegisterServices(IServiceRegistry registry)
        {

            //Views
            registry.AddSingleton<MainWindowView>("MainWindowView");

            registry.AddSingleton<MainContentView>();


            //ViewModels
            registry.AddSingleton<MainWindowViewModel>();

            registry.AddSingleton<MainContentViewModel>();
        }

    }
}
