using ConvMVVM3.Core.Mvvm;

namespace ExampleRegionManager.ViewModels;

// SubView1 does not end in "View", so the convention appends "ViewModel".
public sealed class SubView1ViewModel : ObservableObject
{
    private static int _nextNumber;
    public int InstanceNumber { get; } = Interlocked.Increment(ref _nextNumber);
    public string InstanceId { get; } = Guid.NewGuid().ToString("N")[..8];
}
