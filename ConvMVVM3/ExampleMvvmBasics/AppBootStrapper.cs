using System.Windows;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.WPF;
using ExampleMvvmBasics.ViewModels;
using ExampleMvvmBasics.Views;

namespace ExampleMvvmBasics;

public class AppBootStrapper : Bootstrapper
{
    protected override void RegisterServices(IServiceRegistry services)
    {
        services.AddSingleton<MainWindowView>();
        services.AddSingleton<MainWindowViewModel>();
        // This example needs no additional application services.
    }
    protected override Window CreateShell(IServiceContainer provider) => provider.GetRequiredService<MainWindowView>();
    protected override void RegisterModules() { }
    protected override void ConfigureRegion(IRegionManager regions) { }
    protected override void OnInitialized(IServiceContainer provider) { }
}
