using ConvMVVM3.Core.Mvvm.Disposables;

namespace ExampleResourceLifecycle.Services;

// This demo source is owned and invoked on the UI thread.
public sealed class SignalSource
{
    private event Action? Pulse;
    public int SubscriberCount { get; private set; }
    public IDisposable Subscribe(Action callback)
    {
        Pulse += callback;
        SubscriberCount++;
        return Disposable.Create(() => { Pulse -= callback; SubscriberCount--; });
    }
    public void Send() => Pulse?.Invoke();
}
