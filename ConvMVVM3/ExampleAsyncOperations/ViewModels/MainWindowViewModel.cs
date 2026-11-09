using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Commands;
using ConvMVVM3.Core.Mvvm.Threading;
using ExampleAsyncOperations.Services;

namespace ExampleAsyncOperations.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly DemoJobService _service;
    public MainWindowViewModel(DemoJobService service)
    {
        _service = service;
        // A manual command passes its CancellationToken to the service explicitly.
        RunCommand = new AsyncRelayCommand(RunAsync);
        CancelCommand = new RelayCommand(() => RunCommand.Cancel(), () => IsBusy);
    }

    public AsyncRelayCommand RunCommand { get; }
    public RelayCommand CancelCommand { get; }
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _simulateFailure;
    [ObservableProperty] private int _progress;
    [ObservableProperty] private int _completedRuns;
    [ObservableProperty] private int _cancelledRuns;
    [ObservableProperty] private int _failedRuns;
    [ObservableProperty] private string _status = "Run a local job, cancel it, or simulate a service failure.";

    private async Task RunAsync(CancellationToken token)
    {
        using var busy = BusyScope.Enter(value =>
        {
            IsBusy = value;
            CancelCommand.NotifyCanExecuteChanged();
        });
        Progress = 0;
        Status = "Running...";
        try
        {
            await _service.RunAsync(value => Progress = value, SimulateFailure, token);
            CompletedRuns++;
            Status = "Completed. You can run another job.";
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            CancelledRuns++;
            Status = "Cancelled. Partial progress is preserved; the next run starts at zero.";
        }
        catch (Exception error)
        {
            FailedRuns++;
            Status = $"Failed: {error.Message} You can retry.";
        }
    }

    public void Dispose() => RunCommand.Cancel();
}
