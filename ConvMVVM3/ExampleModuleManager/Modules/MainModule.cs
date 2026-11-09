using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Modules.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ExampleModuleManager.ViewModels;
using ExampleModuleManager.Views;

namespace ExampleModuleManager.Modules;

[Module("MainModule", "1.0.0")]
public class MainModule : IModule
{
    public void RegisterServices(IServiceRegistry services)
    {
        services.AddSingleton<MainWindowView>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainContentView>();
        services.AddSingleton<MainContentViewModel>();
    }

    public void ConfigureRegions(IRegionManager regions)
    {
        regions.RegisterViewWithRegion("SubContent", RegionType.SingleView);
        regions.RegisterViewWithRegion<MainContentView>("MainContent");
    }

    public void OnInitialized(IServiceResolver serviceResolver) { }
}
