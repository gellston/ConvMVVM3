using ConvMVVM3.Core.Mvvm;
using System.ComponentModel;
using Xunit;

namespace ConvMVVM3.Tests;

public class ObservableObjectTests
{
    private class TestObservableObject : ConvMVVM3.Core.Mvvm.ObservableObject
    {
        private string _testProperty = string.Empty;
        private int _intProperty;

        public string TestProperty
        {
            get => _testProperty;
            set => SetProperty(ref _testProperty, value);
        }

        public int IntProperty
        {
            get => _intProperty;
            set => SetProperty(ref _intProperty, value);
        }
    }

    [Fact]
    public void PropertyChanged_Raised_When_Property_Value_Changes()
    {
        // Arrange
        var obj = new TestObservableObject();
        var propertyChangedRaised = false;
        var changedPropertyName = string.Empty;

        obj.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
            changedPropertyName = e.PropertyName;
        };

        // Act
        obj.TestProperty = "New Value";

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.Equal(nameof(TestObservableObject.TestProperty), changedPropertyName);
    }

    [Fact]
    public void PropertyChanged_Not_Raised_When_Property_Value_Same()
    {
        // Arrange
        var obj = new TestObservableObject
        {
            TestProperty = "Initial Value"
        };

        var propertyChangedRaised = false;
        obj.PropertyChanged += (sender, e) => propertyChangedRaised = true;

        // Act
        obj.TestProperty = "Initial Value"; // Same value

        // Assert
        Assert.False(propertyChangedRaised);
    }

    [Fact]
    public void Property_Setter_Works_With_Value_Types()
    {
        // Arrange
        var obj = new TestObservableObject();
        var propertyChangedRaised = false;

        obj.PropertyChanged += (sender, e) => propertyChangedRaised = true;

        // Act
        obj.IntProperty = 42;

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.Equal(42, obj.IntProperty);
    }

    [Fact]
    public void Implements_INotifyPropertyChanged()
    {
        // Arrange
        var obj = new TestObservableObject();

        // Assert
        Assert.IsAssignableFrom<INotifyPropertyChanged>(obj);
    }

    [Fact]
    public void Implements_INotifyPropertyChanging()
    {
        // Arrange
        var obj = new TestObservableObject();

        // Assert
        Assert.IsAssignableFrom<INotifyPropertyChanging>(obj);
    }

    [Fact]
    public void PropertyChanging_Raised_When_Property_Value_Changes()
    {
        // Arrange
        var obj = new TestObservableObject();
        var propertyChangingRaised = false;
        var changingPropertyName = string.Empty;

        obj.PropertyChanging += (sender, e) =>
        {
            propertyChangingRaised = true;
            changingPropertyName = e.PropertyName;
        };

        // Act
        obj.TestProperty = "New Value";

        // Assert
        Assert.True(propertyChangingRaised);
        Assert.Equal(nameof(TestObservableObject.TestProperty), changingPropertyName);
    }

    [Fact]
    public void PropertyChanging_Not_Raised_When_Property_Value_Same()
    {
        // Arrange
        var obj = new TestObservableObject
        {
            TestProperty = "Initial Value"
        };

        var propertyChangingRaised = false;
        obj.PropertyChanging += (sender, e) => propertyChangingRaised = true;

        // Act
        obj.TestProperty = "Initial Value"; // Same value

        // Assert
        Assert.False(propertyChangingRaised);
    }

    [Fact]
    public void PropertyChanged_Event_Can_Be_Subscribed_And_Unsubscribed()
    {
        // Arrange
        var obj = new TestObservableObject();
        var handlerCalled = false;

        PropertyChangedEventHandler handler = (sender, e) => handlerCalled = true;

        // Act - Subscribe
        obj.PropertyChanged += handler;
        obj.TestProperty = "Value";

        // Assert - Handler called
        Assert.True(handlerCalled);

        // Act - Unsubscribe and reset
        handlerCalled = false;
        obj.PropertyChanged -= handler;
        obj.TestProperty = "New Value";

        // Assert - Handler not called
        Assert.False(handlerCalled);
    }
}