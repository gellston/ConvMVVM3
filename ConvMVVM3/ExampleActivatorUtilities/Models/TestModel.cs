using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ExampleActivatorUtilities.Services;

namespace ExampleActivatorUtilities.Models;

public partial class TestModel : ObservableObject
{
    private readonly IGreetingService _greetingService;

    // The service comes from DI; name and age are supplied at runtime.
    public TestModel(IGreetingService greetingService, string name, int age)
    {
        _greetingService = greetingService;
        _name = name;
        _age = age;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor("Greeting")]
    private string _name;

    [ObservableProperty] private int _age;

    public string InstanceId { get; } = Guid.NewGuid().ToString("N")[..8];
    public string CreationMethod { get; set; } = "";
    public string Greeting => _greetingService.CreateGreeting(Name);
}
