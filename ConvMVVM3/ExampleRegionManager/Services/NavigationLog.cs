using System.Collections.ObjectModel;

namespace ExampleRegionManager.Services;

public sealed class NavigationLog
{
    public ObservableCollection<string> Entries { get; } = new();
    public void Record(string message)
    {
        Entries.Insert(0, $"{DateTime.Now:HH:mm:ss}  {message}");
        if (Entries.Count > 30) Entries.RemoveAt(Entries.Count - 1);
    }
}
