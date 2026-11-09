using System.Collections.ObjectModel;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Messaging;
using ConvMVVM3.Core.Mvvm.Messaging.Abstractions;
using ExampleMessaging.Models;

namespace ExampleMessaging.ViewModels;

public partial class ReceiverViewModel : ObservableRecipient, IRecipient<Notice>
{
    public ReceiverViewModel(IMessenger messenger, string name, string channel) : base(messenger)
    {
        Name = name;
        Token = channel;
        IsActive = true;
    }
    public string Name { get; }
    public string Channel => Token?.ToString() ?? "Default";
    public ObservableCollection<string> Messages { get; } = new();
    [ObservableProperty] private int _receivedCount;
    protected override void OnTokenChanged() => OnPropertyChanged(nameof(Channel));

    public void Receive(Notice message)
    {
        ReceivedCount++;
        Messages.Insert(0, $"{message.SentAt:HH:mm:ss}: {message.Text}");
        if (Messages.Count > 30) Messages.RemoveAt(Messages.Count - 1);
    }
}
