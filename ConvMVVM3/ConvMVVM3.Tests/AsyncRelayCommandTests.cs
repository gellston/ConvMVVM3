using ConvMVVM3.Core.Mvvm.Commands;
using Xunit;

namespace ConvMVVM3.Tests;

public class AsyncRelayCommandTests
{
    private static TaskCompletionSource Gate() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task ExecuteAsync_PassesParameterToAction()
    {
        object? received = null;
        var command = new AsyncRelayCommand<object>((value, _) =>
        {
            received = value;
            return Task.CompletedTask;
        });
        var parameter = new object();
        await command.ExecuteAsync(parameter);
        Assert.Same(parameter, received);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotStartAnotherOperationWhileRunning()
    {
        var release = Gate();
        var count = 0;
        // Ignore cancellation so the first operation stays in progress.
        var command = new AsyncRelayCommand(() => { count++; return release.Task; });
        var first = command.ExecuteAsync(null);
        try
        {
            await command.ExecuteAsync(null).WaitAsync(Timeout);
            Assert.Equal(1, count);
            Assert.True(command.IsExecuting);
        }
        finally
        {
            release.TrySetResult();
            await first.WaitAsync(Timeout);
        }
        await command.ExecuteAsync(null);
        Assert.Equal(2, count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cancel_CancelsTokenAndRestoresState(bool generic)
    {
        var started = Gate();
        async Task Run(CancellationToken token)
        {
            started.TrySetResult();
            await Task.Delay(System.Threading.Timeout.Infinite, token);
        }
        var command = new AsyncRelayCommand(Run);
        var typed = new AsyncRelayCommand<int>((_, token) => Run(token));
        var operation = generic ? typed.ExecuteAsync(42) : command.ExecuteAsync(null);
        await started.Task.WaitAsync(Timeout);
        if (generic) typed.Cancel(); else command.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation.WaitAsync(Timeout));
        Assert.False(generic ? typed.IsRunning : command.IsRunning);
        Assert.True(generic ? typed.CanExecute(42) : command.CanExecute(null));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execution_RaisesStartAndFinishNotifications(bool generic)
    {
        var release = Gate();
        var command = new AsyncRelayCommand(() => release.Task);
        var typed = new AsyncRelayCommand<int>((_, _) => release.Task);
        var states = new List<bool>();
        if (generic) typed.CanExecuteChanged += (_, _) => states.Add(typed.CanExecute(1));
        else command.CanExecuteChanged += (_, _) => states.Add(command.CanExecute(null));
        Assert.True(generic ? typed.CanExecute(1) : command.CanExecute(null));
        var operation = generic ? typed.ExecuteAsync(1) : command.ExecuteAsync(null);
        try
        {
            Assert.True(generic ? typed.IsRunning : command.IsExecuting);
            Assert.Equal(new[] { false }, states);
        }
        finally
        {
            release.TrySetResult();
            await operation.WaitAsync(Timeout);
        }
        Assert.False(generic ? typed.IsRunning : command.IsExecuting);
        Assert.Equal(new[] { false, true }, states);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Failure_PropagatesAndAllowsNextExecution(bool generic)
    {
        var count = 0;
        var error = new InvalidOperationException("operation failed");
        Task Run() => ++count == 1 ? Task.FromException(error) : Task.CompletedTask;
        var command = new AsyncRelayCommand(Run);
        var typed = new AsyncRelayCommand<int>((_, _) => Run());
        Task Execute() => generic ? typed.ExecuteAsync(1) : command.ExecuteAsync(null);
        Assert.Same(error, await Assert.ThrowsAsync<InvalidOperationException>(Execute));
        Assert.False(generic ? typed.IsRunning : command.IsRunning);
        await Execute();
        Assert.Equal(2, count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DisableWhileRunningFalse_KeepsCanExecuteEnabled(bool generic)
    {
        var release = Gate();
        var command = new AsyncRelayCommand(() => release.Task, disableWhileRunning: false);
        var typed = new AsyncRelayCommand<int>((_, _) => release.Task, disableWhileRunning: false);
        var operation = generic ? typed.ExecuteAsync(1) : command.ExecuteAsync(null);
        try { Assert.True(generic ? typed.CanExecute(1) : command.CanExecute(null)); }
        finally { release.TrySetResult(); await operation.WaitAsync(Timeout); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConcurrentExecutions_KeepRunningAndCancellationUntilLastOperationEnds(bool generic)
    {
        var firstRelease = Gate();
        var secondStarted = Gate();
        var secondRelease = Gate();
        var calls = 0;
        async Task Run(CancellationToken token)
        {
            if (++calls == 1) await firstRelease.Task;
            else
            {
                secondStarted.TrySetResult();
                await secondRelease.Task.WaitAsync(token);
            }
        }
        var command = new AsyncRelayCommand(Run, allowConcurrentExecutions: true);
        var typed = new AsyncRelayCommand<int>((_, token) => Run(token), allowConcurrentExecutions: true);
        var first = generic ? typed.ExecuteAsync(1) : command.ExecuteAsync(null);
        var second = generic ? typed.ExecuteAsync(2) : command.ExecuteAsync(null);
        try
        {
            await secondStarted.Task.WaitAsync(Timeout);
            Assert.Equal(2, calls);
            firstRelease.TrySetResult();
            await first.WaitAsync(Timeout);
            Assert.True(generic ? typed.IsRunning : command.IsRunning);
            Assert.False(generic ? typed.CanExecute(1) : command.CanExecute(null));
            if (generic) typed.Cancel(); else command.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => second.WaitAsync(Timeout));
            Assert.False(generic ? typed.IsRunning : command.IsRunning);
        }
        finally
        {
            firstRelease.TrySetResult();
            if (generic) typed.Cancel(); else command.Cancel();
            secondRelease.TrySetResult();
            await first.WaitAsync(Timeout);
            try { await second.WaitAsync(Timeout); }
            catch (OperationCanceledException) { }
        }
    }

    [Fact]
    public void CanExecute_UsesPredicateAndRejectsWrongParameterType()
    {
        var enabled = false;
        var command = new AsyncRelayCommand(() => Task.CompletedTask, () => enabled);
        Assert.False(command.CanExecute(null));
        enabled = true;
        Assert.True(command.CanExecute(null));
        var typed = new AsyncRelayCommand<int>((_, _) => Task.CompletedTask, value => value > 0);
        Assert.False(typed.CanExecute("wrong type"));
        Assert.False(typed.CanExecute(0));
        Assert.True(typed.CanExecute(1));
    }

    [Fact]
    public void Constructors_RejectNullActions()
    {
        Assert.Throws<ArgumentNullException>(() => new AsyncRelayCommand((Func<Task>)null!));
        Assert.Throws<ArgumentNullException>(() => new AsyncRelayCommand((Func<CancellationToken, Task>)null!));
        Assert.Throws<ArgumentNullException>(() => new AsyncRelayCommand<int>(null!));
    }
}
