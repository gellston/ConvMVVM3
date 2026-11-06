using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using Xunit;

namespace ConvMVVM3.Tests;

public class GeneratedNotificationTests
{
    [Fact]
    public void PropertyChange_NotifiesDependenciesAndCommandExactlyOnce()
    {
        var vm = new NotificationViewModel();
        var changed = new List<string?>();
        var changing = new List<string?>();
        var commandChanges = 0;
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        vm.PropertyChanging += (_, e) => changing.Add(e.PropertyName);
        vm.SaveCommand.CanExecuteChanged += (_, _) => commandChanges++;
        vm.Name = "Alice";
        Assert.Equal("Hello Alice", vm.DisplayName);
        Assert.Equal(new[] { "Name", "DisplayName" }, changed);
        Assert.Equal(new[] { "Name", "DisplayName" }, changing);
        Assert.Equal(1, commandChanges);
        vm.Name = "Alice";
        Assert.Equal(2, changed.Count);
        Assert.Equal(2, changing.Count);
        Assert.Equal(1, commandChanges);
        vm.SaveCommand.Execute(null);
        Assert.Equal("Alice", vm.SavedName);
    }
}

public partial class NotificationViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor("DisplayName")]
    [NotifyPropertyChangingFor("DisplayName")]
    [NotifyCanExecuteChangedFor("Save")]
    private string _name = "";
    public string DisplayName => "Hello " + Name;
    public string? SavedName { get; private set; }
    [RelayCommand] private void Save() => SavedName = Name;
}
