using ConvMVVM3.Core.Mvvm;

namespace ExampleRegionManager.ViewModels;

// SubView2 does not end in "View", so the convention appends "ViewModel".
public sealed class SubView2ViewModel : ObservableObject
{
    private static int _nextNumber;
    public int InstanceNumber { get; } = Interlocked.Increment(ref _nextNumber);
    public string InstanceId { get; } = Guid.NewGuid().ToString("N")[..8];
}
