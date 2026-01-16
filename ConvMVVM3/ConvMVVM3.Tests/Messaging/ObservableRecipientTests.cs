using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Messaging;
using ConvMVVM3.Core.Mvvm.Messaging.Abstractions;
using System.ComponentModel;
using Xunit;

namespace ConvMVVM3.Tests.Messaging;

public class ObservableRecipientTests
{
    private class TestRecipient : ObservableRecipient
    {
        private string _message = string.Empty;

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public int PropertyChangedCount { get; private set; }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            PropertyChangedCount++;
        }
    }

    private class TestMessage
    {
        public string Content { get; set; } = string.Empty;
    }

    private class TestMessageRecipient : ObservableRecipient, IRecipient<TestMessage>
    {
        public TestMessage ReceivedMessage { get; private set; }

        public void Receive(TestMessage message)
        {
            ReceivedMessage = message;
        }
    }

    [Fact]
    public void Is_ObservableObject()
    {
        // Arrange
        var recipient = new TestRecipient();

        // Assert
        Assert.IsAssignableFrom<ObservableObject>(recipient);
        Assert.IsAssignableFrom<INotifyPropertyChanged>(recipient);
    }

    [Fact]
    public void Implements_IRecipient()
    {
        // Arrange
        var recipient = new TestMessageRecipient();

        // Assert
        Assert.IsAssignableFrom<IRecipient>(recipient);
        Assert.IsAssignableFrom<IRecipient<TestMessage>>(recipient);
    }

    [Fact]
    public void Messenger_Is_Accessible()
    {
        // Arrange
        var recipient = new TestRecipient();

        // Assert
        Assert.NotNull(recipient.Messenger);
        Assert.IsType<WeakReferenceMessenger>(recipient.Messenger);
    }

    [Fact]
    public void IsRegistered_Initially_False()
    {
        // Arrange
        var recipient = new TestRecipient();

        // Assert
        Assert.False(recipient.IsRegistered);
    }

    [Fact]
    public void IsRegistered_True_After_Registering_With_IRecipient()
    {
        // Arrange
        var recipient = new TestMessageRecipient();

        // Act
        recipient.Messenger.Register<TestMessage>(recipient);

        // Assert
        Assert.True(recipient.IsRegistered);
    }

    [Fact]
    public void IsRegistered_False_After_Unregistering_All()
    {
        // Arrange
        var recipient = new TestMessageRecipient();
        recipient.Messenger.Register<TestMessage>(recipient);

        // Act
        recipient.Messenger.UnregisterAll(recipient);

        // Assert
        Assert.False(recipient.IsRegistered);
    }

    [Fact]
    public void Messenger_Is_WeakReferenceMessenger_Default()
    {
        // Arrange
        var recipient = new TestRecipient();

        // Assert
        Assert.Same(WeakReferenceMessenger.Default, recipient.Messenger);
    }

    [Fact]
    public void Can_Set_Custom_Messenger()
    {
        // Arrange
        var recipient = new TestRecipient();
        var customMessenger = new WeakReferenceMessenger();

        // Act
        recipient.Messenger = customMessenger;

        // Assert
        Assert.Same(customMessenger, recipient.Messenger);
    }

    [Fact]
    public void PropertyChanged_Works_In_ObservableRecipient()
    {
        // Arrange
        var recipient = new TestRecipient();
        var propertyChangedRaised = false;

        recipient.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(TestRecipient.Message))
                propertyChangedRaised = true;
        };

        // Act
        recipient.Message = "Test";

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.Equal("Test", recipient.Message);
        Assert.Equal(1, recipient.PropertyChangedCount);
    }

    [Fact]
    public void Can_Receive_Messages_When_IRecipient()
    {
        // Arrange
        var recipient = new TestMessageRecipient();
        var message = new TestMessage { Content = "Hello" };

        recipient.Messenger.Register<TestMessage>(recipient);

        // Act
        recipient.Messenger.Send(message);

        // Assert
        Assert.NotNull(recipient.ReceivedMessage);
        Assert.Equal(message, recipient.ReceivedMessage);
        Assert.Equal("Hello", recipient.ReceivedMessage.Content);
    }

    [Fact]
    public void OnActivated_Can_Be_Overridden()
    {
        // Arrange
        var activated = false;
        var recipient = new TestRecipient();

        // Create a derived class to test OnActivated
        var derivedRecipient = new TestRecipientDerived(() => activated = true);

        // Act
        derivedRecipient.OnActivated();

        // Assert
        Assert.True(activated);
    }

    [Fact]
    public void OnDeactivated_Can_Be_Overridden()
    {
        // Arrange
        var deactivated = false;
        var recipient = new TestRecipient();

        // Create a derived class to test OnDeactivated
        var derivedRecipient = new TestRecipientDerived(null, () => deactivated = true);

        // Act
        derivedRecipient.OnDeactivated();

        // Assert
        Assert.True(deactivated);
    }

    private class TestRecipientDerived : ObservableRecipient
    {
        private readonly Action _onActivated;
        private readonly Action _onDeactivated;

        public TestRecipientDerived(Action onActivated = null, Action onDeactivated = null)
        {
            _onActivated = onActivated;
            _onDeactivated = onDeactivated;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            _onActivated?.Invoke();
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            _onDeactivated?.Invoke();
        }
    }
}