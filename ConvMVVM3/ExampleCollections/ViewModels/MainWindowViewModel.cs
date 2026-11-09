using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Collections;

namespace ExampleCollections.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        IndividualItems.CollectionChanged += (_, _) => IndividualNotifications++;
        Items.CollectionChanged += (_, change) =>
        {
            RangeNotifications++;
            if (change.Action == NotifyCollectionChangedAction.Reset) ResetNotifications++;
            LastAction = change.Action.ToString();
        };
    }
    public ObservableCollection<string> IndividualItems { get; } = new();
    public ObservableRangeCollection<string> Items { get; } = new();
    [ObservableProperty] private int _individualNotifications;
    [ObservableProperty] private int _rangeNotifications;
    [ObservableProperty] private int _resetNotifications;
    [ObservableProperty] private string _lastAction = "None";
    [ObservableProperty] private string _status = "Compare 1,000 individual additions with one AddRange operation.";

    [RelayCommand]
    private void LoadIndividually()
    {
        IndividualItems.Clear();
        IndividualNotifications = 0; // Exclude setup from the operation being measured.
        foreach (var item in Enumerable.Range(1, 1000)) IndividualItems.Add($"Item {item:0000}");
        Status = $"Individual add: {IndividualItems.Count} items, {IndividualNotifications} notifications.";
    }
    [RelayCommand]
    private void LoadRange()
    {
        Items.ClearAndNotify();
        ResetMetrics();
        Items.AddRange(Enumerable.Range(1, 1000).Select(item => $"Item {item:0000}"));
        Status = $"AddRange: {Items.Count} items, {RangeNotifications} notification.";
    }
    [RelayCommand]
    private void ReplaceRange()
    {
        ResetMetrics();
        Items.ReplaceRange(Enumerable.Range(1, 200).Select(item => $"Replacement {item:0000}"));
        Status = "Replaced the entire range with 200 items.";
    }
    [RelayCommand]
    private void RemoveRange()
    {
        ResetMetrics();
        Items.RemoveRange(Items.Take(100).ToArray());
        Status = $"Removed up to 100 items. Remaining: {Items.Count}.";
    }
    [RelayCommand]
    private void ClearRange()
    {
        ResetMetrics();
        Items.ClearAndNotify();
        Status = "Cleared the range. Clearing an empty collection sends no notification.";
    }
    private void ResetMetrics() { RangeNotifications = 0; ResetNotifications = 0; LastAction = "None"; }
}
