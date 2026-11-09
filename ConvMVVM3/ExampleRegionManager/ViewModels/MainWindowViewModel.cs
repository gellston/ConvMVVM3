using System.Collections.ObjectModel;
using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ExampleRegionManager.Model;
using ExampleRegionManager.Services;
using ExampleRegionManager.Views;

namespace ExampleRegionManager.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IRegionManager _regions;
    private readonly Customer[] _customers = { new(101, "Alex Morgan"), new(102, "Taylor Reed"), new(103, "Jordan Lee") };
    private int _customerIndex = -1;
    public MainWindowViewModel(IRegionManager regions, NavigationLog log)
    {
        _regions = regions;
        NavigationHistory = log.Entries;
    }

    public ObservableCollection<string> NavigationHistory { get; }
    [ObservableProperty] private string _status = "Compare customer navigation with adding multiple views.";

    [RelayCommand]
    private void SwitchContent() => Run(() =>
    {
        _customerIndex = (_customerIndex + 1) % _customers.Length;
        _regions.RequestNavigate<MainView>("ContentView", new NavigationParameters
        {
            ["Customer"] = _customers[_customerIndex]
        });
    }, "Customer changed. The singleton ViewModel keeps the same instance ID.");

    [RelayCommand]
    private void AddItems() => Run(() => _regions.RequestNavigate("SubView1", "ExampleApp/SubView/1", 3),
        "Added three distinct transient views to the ItemsControl.");

    [RelayCommand]
    private void AddList() => Run(() => _regions.RequestNavigate("SubView2", "ExampleApp/SubView/2", 3),
        "Added three distinct transient views to the ListBox.");

    [RelayCommand]
    private void SelectList() => Run(() =>
    {
        if (_regions.Regions["SubView2"].Views.Count < 2)
            throw new InvalidOperationException("Add at least two list items first.");
        _regions.Active("SubView2", 1); // Zero-based index: select the second item.
    }, "Selected the second list item.");

    [RelayCommand] private void ClearItems() => Run(() => _regions.ClearRegion("SubView1"), "Cleared the ItemsControl.");
    [RelayCommand] private void ClearList() => Run(() => _regions.ClearRegion("SubView2"), "Cleared the ListBox.");

    private void Run(Action action, string success)
    {
        try { action(); Status = success; }
        catch (Exception error) { Status = $"Operation failed: {error.Message}"; }
    }
}
