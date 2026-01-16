using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Messaging;
using ConvMVVM3.Core.Mvvm.Messaging.Abstractions;
using System.ComponentModel;
using Xunit;

namespace ConvMVVM3.Tests;

public class ObservableRecipientTests
{
    private class TestMessage
    {
        public string Content { get; set; } = string.Empty;
    }

    private class TestMessageRecipient : ObservableRecipient, IRecipient<TestMessage>
    {
        private TestMessage _receivedMessage;
        
        public TestMessage ReceivedMessage => _receivedMessage;
        
        // Public property to track message reception
        public bool HasReceivedMessage => _receivedMessage != null;

        public void Receive(TestMessage message)
        {
            _receivedMessage = message;
        }
        
        // Helper method to clear message for testing
        public void ClearReceivedMessage()
        {
            _receivedMessage = null;
        }
    }

    private class TestRecipientWithTracking : ObservableRecipient
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

    [Fact]
    public void Is_ObservableObject()
    {
        // Arrange
        var recipient = new TestMessageRecipient();

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
    public void Can_Receive_Message_Through_Default_Messenger()
    {
        // Arrange
        var recipient = new TestMessageRecipient();
        var message = new TestMessage { Content = "Test Message" };

        // Act
        recipient.IsActive = true; // Need to activate to receive messages
        WeakReferenceMessenger.Default.Send(message);

        // Assert
        Assert.True(recipient.HasReceivedMessage);
        Assert.Equal("Test Message", recipient.ReceivedMessage.Content);
    }

    [Fact]
    public void Can_Receive_Message_When_IsActive_True()
    {
        // Arrange
        var recipient = new TestMessageRecipient();
        var message = new TestMessage { Content = "Active Test" };

        // Act
        recipient.IsActive = true;
        WeakReferenceMessenger.Default.Send(message);

        // Assert
        Assert.True(recipient.HasReceivedMessage);
        Assert.Equal("Active Test", recipient.ReceivedMessage.Content);
    }

    [Fact]
    public void Does_Not_Receive_Message_When_IsActive_False()
    {
        // Arrange
        var recipient = new TestMessageRecipient();
        var message = new TestMessage { Content = "Inactive Test" };

        // Act
        recipient.IsActive = true;
        recipient.IsActive = false; // Deactivate
        WeakReferenceMessenger.Default.Send(message);

        // Assert
        Assert.False(recipient.HasReceivedMessage);
        Assert.Null(recipient.ReceivedMessage);
    }

    [Fact]
    public void IsActive_Changes_Property_Notify()
    {
        // Arrange
        var recipient = new TestRecipientWithTracking();

        // Act
        recipient.IsActive = true;
        recipient.IsActive = false;

        // Assert
        Assert.True(recipient.PropertyChangedCount >= 2); // Should fire for each change
    }

    [Fact]
    public void Can_Use_Token_For_Channel_Separation()
    {
        // Arrange
        var recipient1 = new TestMessageRecipient();
        var recipient2 = new TestMessageRecipient();
        var token = "TestChannel";
        var message = new TestMessage { Content = "Channel Message" };

        // Act - Register recipients with different tokens
        recipient1.Token = token;
        recipient1.IsActive = true;
        
        recipient2.IsActive = true; // No token (default channel)
        
        // Send message with specific token
        WeakReferenceMessenger.Default.Send(message, token);

        // Assert
        Assert.True(recipient1.HasReceivedMessage); // Should receive (has token)
        Assert.False(recipient2.HasReceivedMessage); // Should not receive (different channel)
    }

    [Fact]
    public void Can_Change_Token_When_Inactive()
    {
        // Arrange
        var recipient = new TestMessageRecipient();
        var oldToken = "OldChannel";
        var newToken = "NewChannel";

        // Act
        recipient.Token = oldToken;
        Assert.Equal(oldToken, recipient.Token);
        
        recipient.Token = newToken;
        Assert.Equal(newToken, recipient.Token);
    }

    [Fact]
    public void Token_Throws_When_IsActive_True()
    {
        // Arrange
        var recipient = new TestMessageRecipient();
        recipient.IsActive = true;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => recipient.Token = "NewToken");
    }

    [Fact]
    public void Can_Activate_And_Deactivate_Multiple_Times()
    {
        // Arrange
        var recipient = new TestMessageRecipient();
        var message1 = new TestMessage { Content = "Message 1" };
        var message2 = new TestMessage { Content = "Message 2" };
        var message3 = new TestMessage { Content = "Message 3" };

        // Act & Assert
        recipient.IsActive = true;
        WeakReferenceMessenger.Default.Send(message1);
        Assert.True(recipient.HasReceivedMessage);
        Assert.Equal("Message 1", recipient.ReceivedMessage.Content);

        // Clear and deactivate
        recipient.ClearReceivedMessage();
        recipient.IsActive = false;
        WeakReferenceMessenger.Default.Send(message2);
        Assert.False(recipient.HasReceivedMessage); // Should not receive

        // Reactivate
        recipient.IsActive = true;
        WeakReferenceMessenger.Default.Send(message3);
        Assert.True(recipient.HasReceivedMessage);
        Assert.Equal("Message 3", recipient.ReceivedMessage.Content);
    }

    [Fact]
    public void Can_Dispose_Properly()
    {
        // Arrange
        var recipient = new TestMessageRecipient();
        recipient.IsActive = true;

        // Act
        recipient.Dispose();

        // Assert - Should not throw and can be disposed multiple times
        recipient.Dispose(); // If no exception thrown, test passes
    }
}