using System.Windows;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.WPF;
using ExampleMessaging.ViewModels;
using ExampleMessaging.Views;

namespace ExampleMessaging;

public class AppBootStrapper : Bootstrapper
{
    protected override void RegisterServices(IServiceRegistry services)
    {
        services.AddSingleton<MainWindowView>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<ConvMVVM3.Core.Mvvm.Messaging.Abstractions.IMessenger>(_ => new ConvMVVM3.Core.Mvvm.Messaging.WeakReferenceMessenger());
    }
    protected override Window CreateShell(IServiceContainer provider) => provider.GetRequiredService<MainWindowView>();
    protected override void RegisterModules() { }
    protected override void ConfigureRegion(IRegionManager regions) { }
    protected override void OnInitialized(IServiceContainer provider) { }
}
