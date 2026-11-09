using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Messaging.Abstractions;
using ExampleMessaging.Models;

namespace ExampleMessaging.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly IMessenger _messenger;
    public MainWindowViewModel(IMessenger messenger)
    {
        _messenger = messenger;
        Left = new ReceiverViewModel(messenger, "Receiver A", "Alpha");
        Right = new ReceiverViewModel(messenger, "Receiver B", "Beta");
    }
    public ReceiverViewModel Left { get; }
    public ReceiverViewModel Right { get; }
    [ObservableProperty] private string _message = "Hello from the sender";
    [ObservableProperty] private string _status = "Each receiver starts on a different token channel.";

    [RelayCommand] private void SendAlpha() => Send("Alpha");
    [RelayCommand] private void SendBeta() => Send("Beta");
    [RelayCommand]
    private void MoveRightToAlpha()
    {
        var active = Right.IsActive;
        Right.IsActive = false; // Tokens can only change while inactive.
        Right.Token = "Alpha";
        Right.IsActive = active;
        Status = "Receiver B now uses Alpha; its previous active state was preserved.";
    }
    private void Send(string channel)
    {
        if (string.IsNullOrWhiteSpace(Message)) { Status = "Enter a non-empty message."; return; }
        _messenger.Send(new Notice(Message.Trim(), DateTime.Now), channel);
        Status = $"Sent to {channel}. Inactive receivers do not receive it.";
    }
    public void Dispose() { Left.Dispose(); Right.Dispose(); }
}
