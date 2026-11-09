using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Commands;

namespace ExampleMvvmBasics.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        // 1.2.0's generator does not apply CanExecute options; wire the predicate explicitly.
        SaveCommand = new RelayCommand(Save, () => AllowSave &&
            !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor("FullName", "Greeting")]
    [NotifyCanExecuteChangedFor("Save")]
    private string _firstName = "Alex";

    [ObservableProperty]
    [NotifyPropertyChangedFor("FullName", "Greeting")]
    [NotifyCanExecuteChangedFor("Save")]
    private string _lastName = "Morgan";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor("Save")]
    private bool _allowSave = true;

    [ObservableProperty] private int _saveCount;
    [ObservableProperty] private int _clickCount;
    [ObservableProperty] private string _status = "Edit the name and watch dependent properties update.";
    public string FullName => $"{FirstName.Trim()} {LastName.Trim()}".Trim();
    public string Greeting => $"Hello, {FullName}!";
    public RelayCommand SaveCommand { get; }

    private void Save()
    {
        SaveCount++;
        Status = $"Saved {FullName}. Save count: {SaveCount}.";
    }

    [RelayCommand] private void Increment() => ClickCount++;
    [RelayCommand]
    private void Reset()
    {
        FirstName = "Alex";
        LastName = "Morgan";
        AllowSave = true;
        ClickCount = 0;
        SaveCount = 0;
        Status = "Reset to the initial values.";
    }
}
