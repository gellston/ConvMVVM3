using System.Windows;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.WPF;
using ExampleAsyncOperations.ViewModels;
using ExampleAsyncOperations.Views;

namespace ExampleAsyncOperations;

public class AppBootStrapper : Bootstrapper
{
    protected override void RegisterServices(IServiceRegistry services)
    {
        services.AddSingleton<MainWindowView>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<ExampleAsyncOperations.Services.DemoJobService>();
    }
    protected override Window CreateShell(IServiceContainer provider) => provider.GetRequiredService<MainWindowView>();
    protected override void RegisterModules() { }
    protected override void ConfigureRegion(IRegionManager regions) { }
    protected override void OnInitialized(IServiceContainer provider) { }
}
