using System.Windows;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.WPF;
using ExampleActivatorUtilities.Services;
using ExampleActivatorUtilities.ViewModels;
using ExampleActivatorUtilities.Views;

namespace ExampleActivatorUtilities;

public class AppBootStrapper : Bootstrapper
{
    protected override void RegisterServices(IServiceRegistry services)
    {
        services.AddSingleton<MainWindowView>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<IGreetingService, GreetingService>();
        // TestModel needs runtime arguments, so it is created by ActivatorUtilities.
    }

    protected override Window CreateShell(IServiceContainer provider) => provider.GetRequiredService<MainWindowView>();
    protected override void ConfigureRegion(IRegionManager regionManager) { }
    protected override void RegisterModules() { }
    protected override void OnInitialized(IServiceContainer provider) { }
}
