using ConvMVVM3.Core.Mvvm.Threading;
using Xunit;

namespace ConvMVVM3.Tests;

public class ThreadingTests
{
    [Fact]
    public void BusyScope_RestoresStateOnExceptionAndDisposesOnce()
    {
        var states = new List<bool>();
        BusyScope? scope = null;
        Assert.Throws<InvalidOperationException>((Action)(() =>
        {
            using (scope = BusyScope.Enter(states.Add))
            {
                Assert.Equal(new[] { true }, states);
                throw new InvalidOperationException();
            }
        }));
        scope!.Dispose();
        Assert.Equal(new[] { true, false }, states);
    }

    [Fact]
    public void BusyCounter_RemainsBusyUntilLastScopeExits()
    {
        var count = 0;
        var states = new List<bool>();
        using var first = BusyCounterScope.Enter(() => count, value => count = value, states.Add);
        using var second = BusyCounterScope.Enter(() => count, value => count = value, states.Add);
        Assert.Equal(2, count);
        first.Dispose();
        first.Dispose();
        Assert.Equal(1, count);
        Assert.Equal(new[] { true }, states);
        second.Dispose();
        Assert.Equal(0, count);
        Assert.Equal(new[] { true, false }, states);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OperationGuard_RejectsReentryAndAllowsNextOperation(bool generic)
    {
        var guard = new OperationGuard();
        var release = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        Task<int> Run() { calls++; return release.Task; }
        Task first = generic ? guard.RunAsync<int>(Run) : guard.RunAsync(() => Run());
        try
        {
            Assert.True(guard.IsRunning);
            if (generic) Assert.Equal(0, await guard.RunAsync<int>(Run));
            else await guard.RunAsync(() => Run());
            Assert.Equal(1, calls);
        }
        finally
        {
            release.TrySetResult(42);
            await first.WaitAsync(TimeSpan.FromSeconds(5));
        }
        Assert.False(guard.IsRunning);
        if (generic) Assert.Equal(42, await guard.RunAsync<int>(Run));
        else await guard.RunAsync(() => Run());
        Assert.Equal(2, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OperationGuard_ResetsAfterException(bool generic)
    {
        var guard = new OperationGuard();
        var error = new InvalidOperationException("failed");
        Task Run() => generic
            ? guard.RunAsync<int>(() => Task.FromException<int>(error))
            : guard.RunAsync(() => Task.FromException(error));
        Assert.Same(error, await Assert.ThrowsAsync<InvalidOperationException>(Run));
        Assert.False(guard.IsRunning);
        await guard.RunAsync(() => Task.CompletedTask);
    }
}
