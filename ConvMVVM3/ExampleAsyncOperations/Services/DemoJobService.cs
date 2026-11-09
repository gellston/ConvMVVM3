namespace ExampleAsyncOperations.Services;

public sealed class DemoJobService
{
    // A local simulation keeps this example independent of network availability.
    public TimeSpan StepDelay { get; set; } = TimeSpan.FromMilliseconds(250);
    public async Task RunAsync(Action<int> report, bool fail, CancellationToken token)
    {
        for (var step = 1; step <= 10; step++)
        {
            await Task.Delay(StepDelay, token);
            if (fail && step == 5) throw new InvalidOperationException("Simulated service failure at 50%.");
            report(step * 10);
        }
    }
}
