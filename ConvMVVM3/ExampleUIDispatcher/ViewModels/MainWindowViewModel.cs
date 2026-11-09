using System.Collections.ObjectModel;
using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Commands;
using ConvMVVM3.Core.Mvvm.UIDispatcher.Abstractions;

namespace ExampleUIDispatcher.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly IUIDispatcher _dispatcher;
    public MainWindowViewModel(IUIDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
        UIThreadId = Environment.CurrentManagedThreadId;
        StartCommand = new AsyncRelayCommand(RunAsync);
        CancelCommand = new RelayCommand(() => StartCommand.Cancel(), () => IsBusy);
    }
    public int UIThreadId { get; }
    public AsyncRelayCommand StartCommand { get; }
    public RelayCommand CancelCommand { get; }
    public ObservableCollection<string> Updates { get; } = new();
    [ObservableProperty] private int _workerThreadId;
    [ObservableProperty] private int _updateThreadId;
    [ObservableProperty] private int _progress;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _status = "Background computation is marshalled to the UI through IUIDispatcher.";

    private async Task RunAsync(CancellationToken token)
    {
        IsBusy = true;
        CancelCommand.NotifyCanExecuteChanged();
        Progress = 0;
        Updates.Clear();
        Status = "Processing on a worker thread...";
        try
        {
            await Task.Run(async () =>
            {
                for (var step = 1; step <= 10; step++)
                {
                    await Task.Delay(120, token).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    var worker = Environment.CurrentManagedThreadId;
                    var current = step;
                    await _dispatcher.InvokeAsync(() =>
                    {
                        WorkerThreadId = worker;
                        UpdateThreadId = Environment.CurrentManagedThreadId;
                        Progress = current * 10;
                        Updates.Add($"Step {current}: result {current * current}; worker {worker} -> UI {UpdateThreadId}");
                    });
                }
            }, token);
            Status = "Completed. Every collection update ran on the UI thread.";
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { Status = "Cancelled safely."; }
        catch (Exception error) { Status = $"Failed: {error.Message}"; }
        finally
        {
            IsBusy = false;
            CancelCommand.NotifyCanExecuteChanged();
        }
    }
    public void Dispose() => StartCommand.Cancel();
}
