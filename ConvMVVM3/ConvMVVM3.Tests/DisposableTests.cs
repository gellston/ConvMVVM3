using ConvMVVM3.Core.Mvvm.Disposables;
using Xunit;

namespace ConvMVVM3.Tests;

public class DisposableTests
{
    private sealed class Tracked(Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }

    [Fact]
    public void Composite_DisposesInReverseOrderOnceAndContinuesAfterFailure()
    {
        var calls = new List<int>();
        var group = new CompositeDisposable();
        group.Add(new Tracked(() => calls.Add(1)));
        group.Add(new Tracked(() => { calls.Add(2); throw new InvalidOperationException(); }));
        group.Add(new Tracked(() => calls.Add(3)));
        group.Dispose();
        group.Dispose();
        Assert.True(group.IsDisposed);
        Assert.Equal(new[] { 3, 2, 1 }, calls);
        group.Add(new Tracked(() => calls.Add(4)));
        Assert.Equal(new[] { 3, 2, 1, 4 }, calls);
    }

    [Fact]
    public void Composite_RemoveTransfersOwnershipAndClearAllowsReuse()
    {
        var calls = new List<int>();
        using var group = new CompositeDisposable();
        var removed = new Tracked(() => calls.Add(1));
        group.Add(removed);
        Assert.True(group.Remove(removed));
        Assert.False(group.Remove(removed));
        group.Add(new Tracked(() => calls.Add(2)));
        group.Clear();
        Assert.False(group.IsDisposed);
        group.Add(new Tracked(() => calls.Add(3)));
        group.Dispose();
        Assert.Equal(new[] { 2, 3 }, calls);
        removed.Dispose();
        Assert.Equal(new[] { 2, 3, 1 }, calls);
    }

    [Fact]
    public void Serial_ReplacingAndDisposingReleaseEachOwnedItemOnce()
    {
        var calls = new List<int>();
        var serial = new SerialDisposable();
        var first = new Tracked(() => calls.Add(1));
        var second = new Tracked(() => calls.Add(2));
        serial.Disposable = first;
        serial.Disposable = second;
        Assert.Equal(new[] { 1 }, calls);
        Assert.Same(second, serial.Disposable);
        serial.Dispose();
        serial.Dispose();
        Assert.Null(serial.Disposable);
        Assert.True(serial.IsDisposed);
        serial.Disposable = new Tracked(() => calls.Add(3));
        Assert.Equal(new[] { 1, 2, 3 }, calls);
    }
}
