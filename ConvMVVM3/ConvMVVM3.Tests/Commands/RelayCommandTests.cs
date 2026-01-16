using ConvMVVM3.Core.Mvvm.Commands;
using System;
using Xunit;

namespace ConvMVVM3.Tests.Commands;

public class RelayCommandTests
{
    [Fact]
    public void Execute_Invokes_Action_When_CanExecute_Returns_True()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand(() => executed = true);

        // Act
        command.Execute(null);

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void Execute_With_Parameter_Passes_Parameter_To_Action()
    {
        // Arrange
        object receivedParameter = null;
        var command = new RelayCommand<object>(param => receivedParameter = param);

        var testParameter = new object();

        // Act
        command.Execute(testParameter);

        // Assert
        Assert.Equal(testParameter, receivedParameter);
    }

    [Fact]
    public void CanExecute_Returns_True_By_Default()
    {
        // Arrange
        var command = new RelayCommand(() => { });

        // Act & Assert
        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public void CanExecute_With_Predicate_Uses_Predicate()
    {
        // Arrange
        var canExecute = false;
        var command = new RelayCommand(() => { }, () => canExecute);

        // Act & Assert
        Assert.False(command.CanExecute(null));

        canExecute = true;
        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public void CanExecuteChanged_Raised_When_RaiseCanExecuteChanged_Called()
    {
        // Arrange
        var command = new RelayCommand(() => { });
        var eventRaised = false;

        command.CanExecuteChanged += (sender, e) => eventRaised = true;

        // Act
        command.RaiseCanExecuteChanged();

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void Constructor_Throws_When_Execute_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RelayCommand(null));
    }

    [Fact]
    public void Constructor_With_CanExecute_Throws_When_Execute_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RelayCommand(null, () => true));
    }

    [Fact]
    public void Execute_Does_Not_Execute_When_CanExecute_Returns_False()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand(() => executed = true, () => false);

        // Act
        command.Execute(null);

        // Assert
        Assert.False(executed);
    }

    [Fact]
    public void Generic_RelayCommand_Works_With_Type_Parameter()
    {
        // Arrange
        string receivedValue = null;
        var command = new RelayCommand<string>(value => receivedValue = value);

        // Act
        command.Execute("test");

        // Assert
        Assert.Equal("test", receivedValue);
    }
}