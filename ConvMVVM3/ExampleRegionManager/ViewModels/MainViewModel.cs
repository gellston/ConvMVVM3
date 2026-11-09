using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ExampleRegionManager.Model;
using ExampleRegionManager.Services;

namespace ExampleRegionManager.ViewModels;

public partial class MainViewModel : ObservableObject, INavigationAware
{
    private readonly NavigationLog _log;
    public MainViewModel(NavigationLog log) => _log = log;
    public string InstanceId { get; } = Guid.NewGuid().ToString("N")[..8];
    [ObservableProperty] private Customer _customer = new(0, "No customer selected");
    [ObservableProperty] private int _navigationCount;

    public bool IsNavigationTarget(NavigationContext context) => true;

    public void OnNavigatedTo(NavigationContext context)
    {
        if (context.Parameters.TryGetValue("Customer", out var value) && value is Customer customer)
            Customer = customer;
        NavigationCount++;
        _log.Record($"Enter customer {Customer.Id} | VM {InstanceId} | Visit {NavigationCount}");
    }

    public void OnNavigatedFrom(NavigationContext context)
        => _log.Record($"Leave customer {Customer.Id} | VM {InstanceId}");
}
