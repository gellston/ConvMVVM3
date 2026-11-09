using System.Collections.ObjectModel;
using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Commands;

namespace ExampleBehaviors.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        EventCommand = new RelayCommand<string>(message =>
        {
            EventCount++;
            Record($"InvokeCommandAction received: {message}");
        }, _ => AllowEvents);
    }
    [ObservableProperty] private string _payload = "Payload from XAML";
    [ObservableProperty]
    [NotifyCanExecuteChangedFor("Event")]
    private bool _allowEvents = true;
    [ObservableProperty] private int _eventCount;
    [ObservableProperty] private int _methodCount;
    public RelayCommand<string> EventCommand { get; }
    public ObservableCollection<string> Events { get; } = new();

    // Called by CallMethodAction, not by a code-behind event handler.
    public void RecordMethodCall()
    {
        MethodCount++;
        Record("CallMethodAction invoked the ViewModel method.");
    }
    private void Record(string message)
    {
        Events.Insert(0, $"{DateTime.Now:HH:mm:ss} {message}");
        if (Events.Count > 30) Events.RemoveAt(Events.Count - 1);
    }
}
