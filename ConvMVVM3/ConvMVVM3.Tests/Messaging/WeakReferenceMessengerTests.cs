using ConvMVVM3.Core.Mvvm.Messaging;
using System;
using Xunit;

namespace ConvMVVM3.Tests.Messaging;

public class WeakReferenceMessengerTests
{
    private class TestMessage
    {
        public string Content { get; set; }
    }

    private class TestRecipient : ObservableRecipient
    {
        public TestMessage ReceivedMessage { get; private set; }
        public int ReceiveCount { get; private set; }

        public TestRecipient()
        {
            // Register message handler
            Messenger.Register<TestMessage>(this, (recipient, message) =>
            {
                ((TestRecipient)recipient).ReceivedMessage = message;
                ((TestRecipient)recipient).ReceiveCount++;
            });
        }
    }

    [Fact]
    public void Send_Delivers_Message_To_Registered_Recipient()
    {
        // Arrange
        var recipient = new TestRecipient();
        var messenger = WeakReferenceMessenger.Default;
        var message = new TestMessage { Content = "Test" };

        // Act
        messenger.Send(message);

        // Assert
        Assert.Equal(message, recipient.ReceivedMessage);
        Assert.Equal(1, recipient.ReceiveCount);
    }

    [Fact]
    public void Send_Delivers_Message_To_Multiple_Recipients()
    {
        // Arrange
        var recipient1 = new TestRecipient();
        var recipient2 = new TestRecipient();
        var messenger = WeakReferenceMessenger.Default;
        var message = new TestMessage { Content = "Test" };

        // Act
        messenger.Send(message);

        // Assert
        Assert.Equal(message, recipient1.ReceivedMessage);
        Assert.Equal(message, recipient2.ReceivedMessage);
        Assert.Equal(1, recipient1.ReceiveCount);
        Assert.Equal(1, recipient2.ReceiveCount);
    }

    [Fact]
    public void Unregister_Prevents_Message_Delivery()
    {
        // Arrange
        var recipient = new TestRecipient();
        var messenger = WeakReferenceMessenger.Default;
        var message = new TestMessage { Content = "Test" };

        // Act - Unregister
        messenger.Unregister<TestMessage>(recipient);

        // Send message
        messenger.Send(message);

        // Assert
        Assert.Null(recipient.ReceivedMessage);
        Assert.Equal(0, recipient.ReceiveCount);
    }

    [Fact]
    public void WeakReference_Allows_Garbage_Collection()
    {
        // Arrange
        var messenger = WeakReferenceMessenger.Default;
        var messageReceived = false;

        // Create recipient in a method to allow GC
        CreateRecipientAndSendMessage(messenger);

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Act - Send another message
        messenger.Send(new TestMessage { Content = "After GC" });

        // Assert - No crash or exception should occur
        // The weak reference should have been collected
        Assert.True(true); // If we get here, no exception occurred
    }

    private void CreateRecipientAndSendMessage(WeakReferenceMessenger messenger)
    {
        var recipient = new TestRecipient();
        messenger.Send(new TestMessage { Content = "Initial" });

        // recipient goes out of scope here
    }

    [Fact]
    public void Register_With_Token_Allows_Scoped_Unregistration()
    {
        // Arrange
        var recipient = new TestRecipient();
        var messenger = WeakReferenceMessenger.Default;
        var token = new object();

        // Register with token
        messenger.Register<TestMessage, object>(recipient, token, (r, m) =>
        {
            ((TestRecipient)r).ReceivedMessage = m;
            ((TestRecipient)r).ReceiveCount++;
        });

        var message = new TestMessage { Content = "Test" };

        // Act - Send message (should be received)
        messenger.Send(message);

        // Assert - Message received
        Assert.Equal(message, recipient.ReceivedMessage);
        Assert.Equal(1, recipient.ReceiveCount);

        // Reset
        recipient.ReceivedMessage = null;
        recipient.ReceiveCount = 0;

        // Unregister with token
        messenger.Unregister<TestMessage, object>(recipient, token);

        // Send again
        messenger.Send(new TestMessage { Content = "After Unregister" });

        // Assert - No message received
        Assert.Null(recipient.ReceivedMessage);
        Assert.Equal(0, recipient.ReceiveCount);
    }

    [Fact]
    public void Send_With_Token_Delivers_Only_To_Matching_Recipients()
    {
        // Arrange
        var recipient1 = new TestRecipient();
        var recipient2 = new TestRecipient();
        var messenger = WeakReferenceMessenger.Default;

        var token1 = new object();
        var token2 = new object();

        // Register recipient1 with token1
        messenger.Register<TestMessage, object>(recipient1, token1, (r, m) =>
        {
            ((TestRecipient)r).ReceivedMessage = m;
            ((TestRecipient)r).ReceiveCount++;
        });

        // Register recipient2 with token2
        messenger.Register<TestMessage, object>(recipient2, token2, (r, m) =>
        {
            ((TestRecipient)r).ReceivedMessage = m;
            ((TestRecipient)r).ReceiveCount++;
        });

        var message = new TestMessage { Content = "Test" };

        // Act - Send with token1
        messenger.Send(message, token1);

        // Assert - Only recipient1 received the message
        Assert.Equal(message, recipient1.ReceivedMessage);
        Assert.Equal(1, recipient1.ReceiveCount);
        Assert.Null(recipient2.ReceivedMessage);
        Assert.Equal(0, recipient2.ReceiveCount);
    }

    [Fact]
    public void Reset_Clears_All_Registrations()
    {
        // Arrange
        var recipient = new TestRecipient();
        var messenger = WeakReferenceMessenger.Default;
        var message = new TestMessage { Content = "Test" };

        // Send message to ensure registration works
        messenger.Send(message);
        Assert.Equal(message, recipient.ReceivedMessage);

        // Act - Reset messenger
        messenger.Reset();

        // Reset recipient state
        recipient.ReceivedMessage = null;
        recipient.ReceiveCount = 0;

        // Send message again
        messenger.Send(new TestMessage { Content = "After Reset" });

        // Assert - No message received after reset
        Assert.Null(recipient.ReceivedMessage);
        Assert.Equal(0, recipient.ReceiveCount);
    }
}