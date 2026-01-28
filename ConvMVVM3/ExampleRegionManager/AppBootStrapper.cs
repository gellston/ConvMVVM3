using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.WPF;
using ExampleRegionManager.Model;
using ExampleRegionManager.ViewModels;
using ExampleRegionManager.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ExampleRegionManager
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


            //Windows
            container.AddSingleton<MainWindowView>();

            //ViewModel
            container.AddSingleton<MainViewModel>();
            container.AddSingleton<MainWindowViewModel>();

            //Views
            container.AddSingleton<MainView>();


            //SubView
            container.AddTransient<SubView1>("ExampleApp/SubView/1");

            container.AddTransient<SubView2>("ExampleApp/SubView/2");


            //Models
            container.AddTransient<TestModel>("TestModel");
            
        }

        protected override void ConfigureRegion(IRegionManager regionManager)
        {

            regionManager.RegisterViewWithRegion<MainView>("MainView");

            regionManager.RegisterViewWithRegion("SubView1", "ExampleApp/SubView/1", RegionType.MultiView);
            regionManager.RegisterViewWithRegion("SubView1", "ExampleApp/SubView/1", RegionType.MultiView);
            regionManager.RegisterViewWithRegion("SubView1", "ExampleApp/SubView/1", RegionType.MultiView);
            regionManager.RegisterViewWithRegion("SubView1", "ExampleApp/SubView/1", RegionType.MultiView);



            regionManager.RegisterViewWithRegion("SubView2", "ExampleApp/SubView/2", RegionType.MultiView);

        }

        protected override void OnInitialized(IServiceContainer provider)
        {

        }

        protected override void RegisterModules()
        {
          
        }
        #endregion
    }
}
