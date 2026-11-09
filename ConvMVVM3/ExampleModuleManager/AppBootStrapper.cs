using System.Windows;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.WPF;
using ExampleModuleManager.Modules;
using ExampleModuleManager.ViewModels;
using ExampleModuleManager.Views;

namespace ExampleModuleManager;

public class AppBootStrapper : Bootstrapper
{
    protected override void RegisterModules()
    {
        RegisterModule<AModule>();
        RegisterModule<BModule>();
        RegisterModule<CModule>();
        RegisterModule<MainModule>();
    }

    protected override Window CreateShell(IServiceContainer provider) => provider.GetRequiredService<MainWindowView>();
    protected override void RegisterServices(IServiceRegistry services) { }
    protected override void ConfigureRegion(IRegionManager regionManager) { }

    protected override void OnInitialized(IServiceContainer provider)
    {
        var vm = provider.GetRequiredService<MainContentViewModel>();
        vm.RefreshModuleStatus();
        vm.AModuleCommand.Execute(null);
    }
}
