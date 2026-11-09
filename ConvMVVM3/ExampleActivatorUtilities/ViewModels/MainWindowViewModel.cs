using System.Collections.ObjectModel;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Host.DependencyInjection;
using ExampleActivatorUtilities.Models;

namespace ExampleActivatorUtilities.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IServiceResolver _resolver;
    private readonly ActivatorUtilities.ObjectFactory _factory;

    public MainWindowViewModel(IServiceResolver resolver)
    {
        _resolver = resolver;
        _factory = ActivatorUtilities.CreateFactory(typeof(TestModel), new[] { typeof(string), typeof(int) });
    }

    [ObservableProperty] private string _name = "Alex Morgan";
    [ObservableProperty] private string _age = "25";
    [ObservableProperty] private string _status = "Enter a name and age, then compare the two creation methods.";
    public ObservableCollection<TestModel> TestModelCollection { get; } = new();

    [RelayCommand] private void GenerateModel() => CreateModel(useFactory: false);
    [RelayCommand] private void GenerateWithFactory() => CreateModel(useFactory: true);

    [RelayCommand]
    private void ClearModels()
    {
        TestModelCollection.Clear();
        Status = "The generated list is empty.";
    }

    private void CreateModel(bool useFactory)
    {
        if (string.IsNullOrWhiteSpace(Name) || !int.TryParse(Age, out var age) || age < 0 || age > 150)
        {
            Status = "Enter a name and a whole-number age between 0 and 150.";
            return;
        }

        try
        {
            var model = useFactory
                ? (TestModel)_factory(_resolver, new object[] { Name.Trim(), age })
                : ActivatorUtilities.CreateInstance<TestModel>(_resolver, Name.Trim(), age);
            model.CreationMethod = useFactory ? "CreateFactory" : "CreateInstance";
            TestModelCollection.Add(model);
            Status = $"{model.CreationMethod}: created {model.InstanceId}. Total: {TestModelCollection.Count}.";
        }
        catch (Exception error)
        {
            Status = $"Creation failed: {error.Message}";
        }
    }
}
