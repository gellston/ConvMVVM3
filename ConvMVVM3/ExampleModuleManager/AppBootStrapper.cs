using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.WPF;
using ExampleModuleManager.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ExampleModuleManager
{
    public class AppBootStrapper : Bootstrapper
    {
        protected override void ConfigureRegion(IRegionManager regionManager)
        {
        }

        protected override Window CreateShell(IServiceContainer provider)
        {
            return (Window)provider.GetService("MainWindowView");
        }

        protected override void OnInitialized(IServiceContainer provider)
        {
        }

        protected override void RegisterModules()
        {
            this.RegisterModule<AModule>();
            this.RegisterModule<BModule>();
            this.RegisterModule<CModule>();
            this.RegisterModule<MainModule>();
        }

        protected override void RegisterServices(IServiceRegistry container)
        {
        }
    }
}
