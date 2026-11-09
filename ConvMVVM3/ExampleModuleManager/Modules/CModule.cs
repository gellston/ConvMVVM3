using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Modules.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ExampleModuleManager.ViewModels;
using ExampleModuleManager.Views;

namespace ExampleModuleManager.Modules;

[Module("CModule", "1.0.0", InitializationMode.OnDemand)]
public class CModule : IModule
{
    public void RegisterServices(IServiceRegistry services)
    {
        services.AddSingleton<CModuleView>("/SubContent/CModuleView");
        services.AddSingleton<CModuleViewModel>();
    }

    public void ConfigureRegions(IRegionManager regionManager) { }
    // ModuleManager records initialization. Navigation is a separate, repeatable action.
    public void OnInitialized(IServiceResolver serviceResolver) { }
}
