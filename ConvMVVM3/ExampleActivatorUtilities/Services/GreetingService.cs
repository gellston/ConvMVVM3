namespace ExampleActivatorUtilities.Services;

public interface IGreetingService
{
    string CreateGreeting(string name);
}

public sealed class GreetingService : IGreetingService
{
    public string CreateGreeting(string name) => $"Hello, {name}!";
}
