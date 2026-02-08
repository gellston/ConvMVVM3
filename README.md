<p align="center">
   <img src="https://github.com/gellston/ConvMVVM3/blob/main/logo.png?raw=true" alt="ConvMVVM3 Logo" width="400"/>
</p>

**ConvMVVM3 (Convergence MVVM3)** is a modern, cross-platform MVVM framework that combines the best features from Prism and Community Toolkit while adding unique innovations.

---

## 📌 XAML Namespace Mapping

ConvMVVM3 exposes a **single** XAML XML namespace for a clean, consistent XAML experience:

```xml
<Window
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:convMVVM3="https://github.com/gellston/ConvMVVM3"
    convMVVM3:ViewModelLocator.AutoWireViewModel="True">
</Window>
```

- `xmlns:convMVVM3="https://github.com/gellston/ConvMVVM3"` → ConvMVVM3 XAML features (Regions, ViewModelLocator, Behaviors/Interactivity, etc.)
- `convMVVM3:ViewModelLocator.AutoWireViewModel="True"` → ViewModel auto-wiring (Prism-like)

---

## ✨ Key Differentiators from Prism & Community Toolkit

### 🚀 **UIDispatcher** (UNIQUE - Cross-Platform)
Microsoft-compatible UI thread dispatcher for multiple platforms:

```csharp
// Setup - Available for WPF, MAUI, Avalonia, WinUI
services.AddWPFUIDispatcher();      // WPF

// Usage - Cross-platform compatible
await dispatcher.InvokeAsync(() => Title = "Updated");
```

### 🔌 **Smart Dependency Injection** (UNIQUE - Built-in)
Lightweight service container with addon system - **No external DI required**:

```csharp
// Built-in container - Zero external dependencies
services.AddSingleton<IMyService, MyService>();
```

### 📡 **WeakReference Messenger** (Memory-Safe, Cross-Platform)
Thread-safe messaging with automatic cleanup:

```csharp
// Automatic weak references - Platform-agnostic
WeakReferenceMessenger.Default.Send<Message>(this, newData);
```

### 🎯 **Region System** (Prism-inspired, Cross-Platform)
Built-in region management for multiple frameworks:

```xml
<!-- Works on WPF, MAUI, Avalonia, WinUI -->
<ContentControl convMVVM3:RegionPlugin.RegionName="MainContent" />
```

### ⚡ **Source Generator** (Community Toolkit-inspired)
Automatic property and command generation with enhanced features:

```csharp
[ObservableProperty] private string title;       // Generates Title property
[RelayCommand] private void Save();              // Generates SaveCommand
[AsyncRelayCommand] private async Task Load();   // Generates LoadCommand
```

---

## 🏗️ Architecture Overview

### **Core MVVM Components** (Platform-Agnostic)
```
ConvMVVM3.Core/
├── ObservableObject              # Base class with INotifyPropertyChanged
├── ObservableRecipient           # Message recipient base class
├── UIDispatcher                  # Cross-platform dispatcher interface
├── WeakReferenceMessenger        # Memory-safe messaging system
├── Commands/
│   ├── RelayCommand              # Synchronous command implementation
│   └── AsyncRelayCommand         # Async command with cancellation
└── DependencyInjection/          # Built-in DI container
```

### **WPF Implementation** (One of Many Platforms)
```
ConvMVVM3.WPF/                    # WPF-specific implementation
├── Regions/                      # Region system (Prism-inspired)
├── WPFUIDispatcher               # WPF-specific dispatcher
├── Interactivity/                # Behaviors / triggers / actions
└── WeakEventManager              # Memory-efficient event handling
```

### **Source Generation** (Cross-Platform)
```
ConvMVVM3.SourceGenerator/
├── ObservableProperty generation # Auto property implementation
├── RelayCommand generation       # Auto command creation
├── Dependency tracking           # Smart property notification
└── Compile-time validation       # Early error detection
```

---

## 📁 Project Structure

```
ConvMVVM3.Core/                   # Platform-agnostic MVVM library
ConvMVVM3.SourceGenerator/        # Cross-platform source generator
ConvMVVM3.WPF/                    # WPF-specific interactivity + regions
ConvMVVM3.Host/                   # DI host implementation
ConvMVVM3.WPF.Tests/              # WPF unit tests
ConvMVVM3.Tests/                  # Core unit tests
```

---

## 🛠️ Installation

```bash
dotnet add package ConvMVVM3
```

---

## 🚀 Quick Start

### 1) Basic ViewModel with Cross-Platform Generation

```csharp
// Works on ALL supported platforms
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "Hello ConvMVVM3!";

    [RelayCommand]
    private void ShowMessage() => MessageBox.Show(Title);

    [AsyncRelayCommand]
    private async Task LoadDataAsync()
    {
        Title = "Loading...";
        await Task.Delay(1000);
        Title = "Data Loaded!";
    }
}
```

### 2) ViewModelLocator (AutoWireViewModel)

```xml
<Window
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:convMVVM3="https://github.com/gellston/ConvMVVM3"
    convMVVM3:ViewModelLocator.AutoWireViewModel="True">
    <!-- View content -->
</Window>
```

### 3) Cross-Platform UIDispatcher Integration

```csharp
// Setup varies by platform
services.AddWPFUIDispatcher();       // WPF

// Usage is identical across platforms
public partial class MyViewModel : ObservableObject
{
    private readonly IUIDispatcher _dispatcher;

    public MyViewModel(IUIDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [AsyncRelayCommand]
    private async Task UpdateFromBackgroundAsync()
    {
        var data = await Task.Run(() => GetHeavyData());

        // Thread-safe UI update (cross-platform)
        await _dispatcher.InvokeAsync(() => Title = data);
    }
}
```

### 4) Cross-Platform Region Management

```xml
<!-- Works on WPF, MAUI, Avalonia, WinUI -->
<Grid>
    <ContentControl convMVVM3:RegionPlugin.RegionName="MainContent" />
</Grid>
```

```csharp
// Platform-agnostic navigation
_regionManager.RequestNavigate("MainContent", typeof(HomeViewModel));
```

### 5) Memory-Safe Cross-Platform Messaging

```csharp
// Sender (all platforms)
WeakReferenceMessenger.Default.Send<DataUpdatedMessage>(this, newData);

// Receiver (auto-cleanup, all platforms)
public partial class MyViewModel : ObservableObject, IRecipient<DataUpdatedMessage>
{
    public void Receive(DataUpdatedMessage message)
    {
        Title = message.Data;
    }
}
```

### 6) WPF Interactivity / Behaviors

```xml
<Button Content="Click Me">
    <convMVVM3:Interaction.Triggers>
        <convMVVM3:EventTrigger EventName="Click">
            <convMVVM3:InvokeCommandAction Command="{Binding SaveCommand}" />
        </convMVVM3:EventTrigger>
    </convMVVM3:Interaction.Triggers>
</Button>
```

---

## 🧰 Helpful Utilities

This section includes a few practical helpers used frequently in Prism-style MVVM apps:

- `ConvMVVM3.Core.Mvvm.Threading`
- `ConvMVVM3.Core.Mvvm.Collections`
- `ConvMVVM3.Host.DependencyInjection`

### A) BusyScope — ConvMVVM3.Core.Mvvm.Threading

Use a scope to ensure `IsBusy` is always restored (even if an exception occurs).  
Recommended for simple “single operation” flows.

```csharp
using ConvMVVM3.Core.Mvvm.Threading;

// In your ViewModel
private bool _isBusy;
public bool IsBusy
{
    get => _isBusy;
    set { _isBusy = value; /* RaisePropertyChanged */ }
}

public async Task RefreshAsync()
{
    using (BusyScope.Enter(v => IsBusy = v))
    {
        await LoadAsync();
    }
}
```

### B) BusyCounterScope (Recommended) — ConvMVVM3.Core.Mvvm.Threading

Recommended when operations can overlap (nested calls, multiple async commands).

```csharp
using ConvMVVM3.Core.Mvvm.Threading;

private int _busyCount;
private bool _isBusy;
public bool IsBusy
{
    get => _isBusy;
    set { _isBusy = value; /* RaisePropertyChanged */ }
}

public async Task RefreshAsync()
{
    using (BusyCounterScope.Enter(
        getCount: () => _busyCount,
        setCount: v => _busyCount = v,
        setBusy:  v => IsBusy = v))
    {
        await LoadAsync();
    }
}
```

### C) OperationGuard — ConvMVVM3.Core.Mvvm.Threading

Prevents re-entrancy (double-click save, multiple async executions).  
Combine it with BusyScope/BusyCounterScope for a great UX.

```csharp
using ConvMVVM3.Core.Mvvm.Threading;

private readonly OperationGuard _saveGuard = new OperationGuard();
private int _busyCount;

public Task SaveAsync()
{
    return _saveGuard.RunAsync(async () =>
    {
        using (BusyCounterScope.Enter(
            () => _busyCount,
            v => _busyCount = v,
            v => IsBusy = v))
        {
            await SaveAsyncCore();
        }
    });
}
```

### D) ObservableRangeCollection — ConvMVVM3.Core.Mvvm.Collections

Use `AddRange/ReplaceRange` to avoid thousands of per-item collection change notifications.

```csharp
using ConvMVVM3.Core.Mvvm.Collections;

public ObservableRangeCollection<ItemViewModel> Items { get; } = new();

public async Task LoadItemsAsync()
{
    var items = await LoadFromServerAsync();

    // One reset notification instead of N adds
    Items.ReplaceRange(items);
}
```

### E) ActivatorUtilities — ConvMVVM3.Host.DependencyInjection

`ActivatorUtilities` helps create objects by mixing:
- **DI-resolved services** from `IServiceResolver`
- **runtime arguments** you provide (e.g., IDs, parameters, view references)

```csharp
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Host.DependencyInjection;

// Example types
public sealed class UserDetailsViewModel
{
    public UserDetailsViewModel(IUserService service, int userId)
    {
        // service comes from DI, userId comes from runtime args
    }
}

public static class Example
{
    public static void CreateWithRuntimeArgs(IServiceResolver resolver)
    {
        // 1) CreateInstance: DI + runtime args
        var vm = (UserDetailsViewModel)ActivatorUtilities.CreateInstance(
            resolver,
            typeof(UserDetailsViewModel),
            123);

        // 2) GetServiceOrCreateInstance: prefer registered instance, otherwise create
        var shell = ActivatorUtilities.GetServiceOrCreateInstance<MainShellViewModel>(resolver);

        // 3) CreateFactory: preselect ctor + argument mapping for speed
        var factory = ActivatorUtilities.CreateFactory(
            typeof(UserDetailsViewModel),
            new[] { typeof(int) });

        var vmFast = (UserDetailsViewModel)factory(resolver, new object[] { 456 });
    }
}
```

> Note: The class name is **ActivatorUtilities** (not ActivityUtility).

---

## 🧩 Bootstrapper

ConvMVVM3 supports a Prism-style **Bootstrapper** flow for registering modules, building the shell, and initializing regions/services.

```csharp
public class AppBootStrapper : Bootstrapper
{
    protected override void ConfigureRegion(IRegionManager regionManager)
    {
    }

    protected override Window CreateShell(IServiceContainer provider)
    {
        return (Window)provider.GetService("MainWindowView");
    }

    protected override void OnInitialized(IServiceContainer provider)
    {
    }

    protected override void RegisterModules()
    {
        this.RegisterModule<AModule>();
        this.RegisterModule<BModule>();
        this.RegisterModule<CModule>();
        this.RegisterModule<MainModule>();
    }

    protected override void RegisterServices(IServiceRegistry container)
    {
    }
}
```

---

## 🎯 Platform Support Matrix

| Feature | WPF | MAUI | Avalonia | WinUI | UNO |
|---------|------|------|----------|-------|-----|
| **Core MVVM** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **UIDispatcher** | ✅ | 🔄 | 🔄 | 🔄 | 🔄 |
| **Source Generator** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Region System** | ✅ | 🔄 | 🔄 | 🔄 | 🔄 |
| **Built-in DI** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Weak Messenger** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **WPF Interactivity** | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Platform Interactivity** | ❌ | 🔄 | 🔄 | 🔄 | 🔄 |

*✅ Available | 🔄 Planned | ❌ Not applicable*

---

## 📄 License

MIT License - see [LICENSE](LICENSE) file

---

**ConvMVVM3**: Modern, cross-platform MVVM framework that bridges the gap between Prism's power and Community Toolkit's simplicity while adding unique cross-platform innovations.

**Future Roadmap**: MAUI, Avalonia, WinUI, and UNO Platform support coming soon!
