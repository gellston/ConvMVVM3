using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;

namespace ExampleModuleManager.ViewModels;

public abstract partial class ModuleViewModel : ObservableObject, INavigationAware
{
    protected ModuleViewModel(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public string Title { get; }
    public string Description { get; }
    public string InstanceId { get; } = Guid.NewGuid().ToString("N")[..8];
    [ObservableProperty] private int _navigationCount;
    [ObservableProperty] private string _lastNavigation = "Not visited yet";
    public bool IsNavigationTarget(NavigationContext context) => true;
    public void OnNavigatedTo(NavigationContext context)
    {
        NavigationCount++;
        LastNavigation = $"Entered at {DateTime.Now:HH:mm:ss}";
    }
    public void OnNavigatedFrom(NavigationContext context) => LastNavigation = $"Left at {DateTime.Now:HH:mm:ss}";
}
