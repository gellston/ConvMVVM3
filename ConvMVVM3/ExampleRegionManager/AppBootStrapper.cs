using System.Windows;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.WPF;
using ExampleRegionManager.Services;
using ExampleRegionManager.ViewModels;
using ExampleRegionManager.Views;

namespace ExampleRegionManager;

public class AppBootStrapper : Bootstrapper
{
    protected override Window CreateShell(IServiceContainer provider) => provider.GetRequiredService<MainWindowView>();

    protected override void RegisterServices(IServiceRegistry services)
    {
        services.AddSingleton<NavigationLog>();
        services.AddSingleton<MainWindowView>();
        services.AddSingleton<MainWindowViewModel>();
        // Singleton lifetime, not IsNavigationTarget, supplies instance reuse in this example.
        services.AddSingleton<MainView>();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<SubView1>("ExampleApp/SubView/1");
        services.AddTransient<SubView1ViewModel>();
        services.AddTransient<SubView2>("ExampleApp/SubView/2");
        services.AddTransient<SubView2ViewModel>();
    }

    protected override void ConfigureRegion(IRegionManager regions)
    {
        // Match the RegionName attached properties in MainWindowView.xaml exactly.
        regions.RegisterViewWithRegion("ContentView", RegionType.SingleView);
        regions.RegisterViewWithRegion("SubView1", RegionType.MultiView);
        regions.RegisterViewWithRegion("SubView2", RegionType.MultiView);
    }

    protected override void OnInitialized(IServiceContainer provider)
    {
        var vm = provider.GetRequiredService<MainWindowViewModel>();
        vm.SwitchContentCommand.Execute(null);
        vm.AddItemsCommand.Execute(null);
        vm.AddListCommand.Execute(null);
    }

    protected override void RegisterModules() { }
}
