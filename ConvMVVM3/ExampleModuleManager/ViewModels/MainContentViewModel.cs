using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Modules.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;

namespace ExampleModuleManager.ViewModels;

public partial class MainContentViewModel : ObservableObject
{
    private readonly IRegionManager _regions;
    private readonly IModuleManager _modules;

    public MainContentViewModel(IRegionManager regions, IModuleManager modules)
    {
        _regions = regions;
        _modules = modules;
    }

    [ObservableProperty] private string _moduleStatus = "Waiting for initialization...";
    [ObservableProperty] private string _status = "Open a module to see its ViewModel navigation count.";

    // ModuleCategory is not observable; refresh this display after explicit initialization.
    public void RefreshModuleStatus()
    {
        ModuleStatus = string.Join(" | ", _modules.Categories.Select(category =>
            $"{category.Name}: {(category.IsInitialized ? "initialized" : "not initialized")} ({category.Mode})"));
    }

    [RelayCommand] private void AModule() => OpenModule("AModule");
    [RelayCommand] private void BModule() => OpenModule("BModule");

    [RelayCommand]
    private void CModuleLoad()
    {
        try
        {
            _modules.InitializeModule("CModule"); // Idempotent: only initializes once.
            OpenModule("CModule"); // Always navigate, including subsequent button presses.
        }
        catch (Exception error)
        {
            Status = $"Initialization failed: {error.Message}";
        }
        finally
        {
            RefreshModuleStatus();
        }
    }

    private void OpenModule(string name)
    {
        try
        {
            _regions.RequestNavigate("SubContent", $"/SubContent/{name}View");
            Status = $"Opened {name}. Reopening preserves its singleton ViewModel.";
        }
        catch (Exception error)
        {
            Status = $"Navigation failed: {error.Message}";
        }
    }
}
