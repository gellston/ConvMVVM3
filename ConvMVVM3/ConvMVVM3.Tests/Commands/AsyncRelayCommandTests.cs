using ConvMVVM3.Core.Mvvm.Commands;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ConvMVVM3.Tests.Commands;

public class AsyncRelayCommandTests
{
    [Fact]
    public async Task ExecuteAsync_Invokes_Action_When_CanExecute_Returns_True()
    {
        // Arrange
        var executed = false;
        var command = new AsyncRelayCommand(async () =>
        {
            await Task.Delay(1);
            executed = true;
        });

        // Act
        await command.ExecuteAsync(null);

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public async Task ExecuteAsync_With_Parameter_Passes_Parameter_To_Action()
    {
        // Arrange
        object receivedParameter = null;
        var command = new AsyncRelayCommand<object>(async param =>
        {
            await Task.Delay(1);
            receivedParameter = param;
        });

        var testParameter = new object();

        // Act
        await command.ExecuteAsync(testParameter);

        // Assert
        Assert.Equal(testParameter, receivedParameter);
    }

    [Fact]
    public void CanExecute_Returns_True_By_Default()
    {
        // Arrange
        var command = new AsyncRelayCommand(async () => await Task.Delay(1));

        // Act & Assert
        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public async Task ExecuteAsync_Prevents_Concurrent_Execution()
    {
        // Arrange
        var executionCount = 0;
        var command = new AsyncRelayCommand(async () =>
        {
            executionCount++;
            await Task.Delay(10);
        });

        // Act - Start multiple executions
        var task1 = command.ExecuteAsync(null);
        var task2 = command.ExecuteAsync(null);

        await Task.WhenAll(task1, task2);

        // Assert - Only one execution should occur
        Assert.Equal(1, executionCount);
    }

    [Fact]
    public async Task ExecuteAsync_Allows_Sequential_Execution_After_Completion()
    {
        // Arrange
        var executionCount = 0;
        var command = new AsyncRelayCommand(async () =>
        {
            executionCount++;
            await Task.Delay(1);
        });

        // Act - Execute sequentially
        await command.ExecuteAsync(null);
        await command.ExecuteAsync(null);

        // Assert - Both executions should occur
        Assert.Equal(2, executionCount);
    }

    [Fact]
    public async Task Cancel_Previous_Operation_When_New_Execute_Called()
    {
        // Arrange
        var firstExecutionCompleted = false;
        var secondExecutionStarted = false;
        var command = new AsyncRelayCommand(async () =>
        {
            if (!firstExecutionCompleted)
            {
                firstExecutionCompleted = true;
                await Task.Delay(100); // Long delay
            }
            else
            {
                secondExecutionStarted = true;
                await Task.Delay(1);
            }
        });

        // Act - Start first execution
        var firstTask = command.ExecuteAsync(null);
        await Task.Delay(10); // Let first execution start

        // Start second execution (should cancel first)
        var secondTask = command.ExecuteAsync(null);
        await secondTask;

        // Assert
        Assert.True(secondExecutionStarted);
        // First task should be cancelled
        Assert.True(firstTask.IsCanceled || !firstTask.IsCompleted);
    }

    [Fact]
    public void IsExecuting_True_During_Async_Operation()
    {
        // Arrange
        var command = new AsyncRelayCommand(async () => await Task.Delay(10));

        // Act & Assert - Before execution
        Assert.False(command.IsExecuting);

        var task = Task.Run(async () =>
        {
            await command.ExecuteAsync(null);
        });

        // Give it a moment to start
        Task.Delay(1).Wait();
        Assert.True(command.IsExecuting);

        // Wait for completion
        task.Wait();
        Assert.False(command.IsExecuting);
    }

    [Fact]
    public void CanExecuteChanged_Raised_When_IsExecuting_Changes()
    {
        // Arrange
        var command = new AsyncRelayCommand(async () => await Task.Delay(10));
        var eventRaised = false;

        command.CanExecuteChanged += (sender, e) => eventRaised = true;

        // Act
        var task = command.ExecuteAsync(null);
        Task.Delay(1).Wait(); // Let execution start

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void Constructor_Throws_When_Execute_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AsyncRelayCommand(null));
    }

    [Fact]
    public void Constructor_With_CanExecute_Throws_When_Execute_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AsyncRelayCommand(null, () => true));
    }
}