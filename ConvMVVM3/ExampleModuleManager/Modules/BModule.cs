using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Modules.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ExampleModuleManager.ViewModels;
using ExampleModuleManager.Views;

namespace ExampleModuleManager.Modules;

[Module("BModule", "1.0.0")]
public class BModule : IModule
{
    public void RegisterServices(IServiceRegistry services)
    {
        services.AddSingleton<BModuleView>("/SubContent/BModuleView");
        services.AddSingleton<BModuleViewModel>();
    }

    public void ConfigureRegions(IRegionManager regionManager) { }
    // ModuleManager records initialization. Navigation is a separate, repeatable action.
    public void OnInitialized(IServiceResolver serviceResolver) { }
}
