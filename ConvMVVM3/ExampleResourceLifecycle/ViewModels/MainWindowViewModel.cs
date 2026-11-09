using System.Collections.ObjectModel;
using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Commands;
using ConvMVVM3.Core.Mvvm.Disposables;
using ConvMVVM3.Core.Mvvm.Threading;
using ExampleResourceLifecycle.Services;

namespace ExampleResourceLifecycle.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly SignalSource _source;
    private CompositeDisposable _group = new();
    private SerialDisposable _serial = new();
    private readonly OperationGuard _guard = new();
    private readonly CancellationTokenSource _shutdown = new();
    private int _resourceNumber;
    private bool _disposed;

    public MainWindowViewModel(SignalSource source)
    {
        _source = source;
        // Leave the button enabled so repeated invocations exercise OperationGuard itself.
        GuardedWorkCommand = new AsyncRelayCommand(RunGuardedWorkAsync,
            allowConcurrentExecutions: true, disableWhileRunning: false);
    }
    public AsyncRelayCommand GuardedWorkCommand { get; }
    public ObservableCollection<string> Events { get; } = new();
    [ObservableProperty] private int _activeSubscriptions;
    [ObservableProperty] private int _disposedResources;
    [ObservableProperty] private int _receivedSignals;
    [ObservableProperty] private int _workAttempts;
    [ObservableProperty] private int _acceptedWork;
    [ObservableProperty] private int _rejectedWork;
    [ObservableProperty] private int _completedWork;
    [ObservableProperty] private int _busyCount;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _simulateFailure;
    [ObservableProperty] private bool _groupDisposed;
    [ObservableProperty] private string _status = "Create subscriptions, send a signal, then release or replace their owners.";

    private IDisposable CreateSubscription(string owner)
    {
        var id = ++_resourceNumber;
        var subscription = _source.Subscribe(() =>
        {
            ReceivedSignals++;
            Record($"Signal received by {owner} resource {id}.");
        });
        Record($"Created {owner} resource {id}.");
        return Disposable.Create(() =>
        {
            subscription.Dispose();
            DisposedResources++;
            Record($"Released {owner} resource {id}.");
        });
    }
    [RelayCommand]
    private void AddGrouped()
    {
        _group.Add(CreateSubscription("grouped"));
        Refresh();
        Status = GroupDisposed ? "The disposed group released the new subscription immediately." : "Added a grouped subscription.";
    }
    [RelayCommand]
    private void ReplaceSerial()
    {
        _serial.Disposable = CreateSubscription("serial");
        Refresh();
        Status = "Replaced the serial subscription; the previous one was released.";
    }
    [RelayCommand] private void SendSignal() => _source.Send();
    [RelayCommand]
    private void ClearGrouped()
    {
        _group.Clear(); Refresh(); Status = "Cleared group resources. A live group can be reused.";
    }
    [RelayCommand]
    private void DisposeGrouped()
    {
        _group.Dispose(); Refresh(); Status = "Disposed the group. Repeating Dispose does not release resources twice.";
    }
    [RelayCommand]
    private void ResetResources()
    {
        _group.Dispose(); _serial.Dispose();
        _group = new CompositeDisposable(); _serial = new SerialDisposable();
        Refresh(); Status = "Started a new resource session. Counters retain the history.";
    }
    private void Refresh()
    {
        ActiveSubscriptions = _source.SubscriberCount;
        GroupDisposed = _group.IsDisposed;
    }

    private async Task RunGuardedWorkAsync()
    {
        WorkAttempts++;
        var accepted = false;
        var fail = SimulateFailure;
        try
        {
            await _guard.RunAsync(async () =>
            {
                accepted = true;
                AcceptedWork++;
                using var outer = EnterBusy();
                using var inner = EnterBusy();
                Record("Accepted work. Two nested busy scopes are active.");
                await Task.Delay(700, _shutdown.Token);
                if (fail) throw new InvalidOperationException("Simulated guarded operation failure.");
                CompletedWork++;
                Status = "Guarded work completed. You can run it again.";
            });
            if (!accepted) { RejectedWork++; Record("Ignored a reentrant work request."); }
        }
        catch (OperationCanceledException) when (_shutdown.IsCancellationRequested) { Status = "Work cancelled during shutdown."; }
        catch (Exception error) { Status = $"Failed: {error.Message} The guard and busy scopes were reset."; }
    }
    private BusyCounterScope EnterBusy() => BusyCounterScope.Enter(
        () => BusyCount, value => BusyCount = value, value => IsBusy = value);
    private void Record(string message)
    {
        Events.Insert(0, $"{DateTime.Now:HH:mm:ss} {message}");
        if (Events.Count > 50) Events.RemoveAt(Events.Count - 1);
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _shutdown.Cancel();
        _group.Dispose(); _serial.Dispose();
        Refresh();
        _shutdown.Dispose();
    }
}
