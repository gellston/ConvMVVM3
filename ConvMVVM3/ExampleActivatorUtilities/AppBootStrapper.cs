using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.WPF;
using ExampleActivatorUtilities.Models;
using ExampleActivatorUtilities.ViewModels;
using ExampleActivatorUtilities.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ExampleActivatorUtilities
{
    public class AppBootStrapper : Bootstrapper
    {

        #region Protected Functions
        protected override Window CreateShell(IServiceContainer provider)
        {
            return provider.GetService<MainWindowView>();
        }

        protected override void RegisterServices(IServiceRegistry container)
        {
            container.AddSingleton<MainWindowView>();

            container.AddSingleton<MainWindowViewModel>();


            container.AddTransient<TestModel>();

        }
        protected override void ConfigureRegion(IRegionManager regionManager)
        {

        }

        protected override void OnInitialized(IServiceContainer provider)
        {

        }


        #endregion


    }
}
