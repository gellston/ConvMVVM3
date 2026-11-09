

<p align="center">
   <img src="https://github.com/gellston/ConvMVVM3/blob/main/logo.png?raw=true" alt="ConvMVVM3 Logo" width="400"/>
</p>



<div align="center">

<a href="https://www.nuget.org/packages/ConvMVVM3.Core">
    <img src="https://img.shields.io/nuget/v/ConvMVVM3.Core.svg?style=for-the-badge&logo=nuget&label=NGET%20ConvMVVM3.Core" />

</a>

<a href="https://www.nuget.org/packages/ConvMVVM3.Host">
    <img src="https://img.shields.io/nuget/v/ConvMVVM3.Host.svg?style=for-the-badge&logo=nuget&label=NGET%20ConvMVVM3.Host" />

</a>

<a href="https://www.nuget.org/packages/ConvMVVM3.SourceGenerator">
    <img src="https://img.shields.io/nuget/v/ConvMVVM3.SourceGenerator.svg?style=for-the-badge&logo=nuget&label=NGET%20ConvMVVM3.SourceGenerator" />

</a>

<br/>
<a href="https://www.nuget.org/packages/ConvMVVM3.WPF">
    <img src="https://img.shields.io/nuget/v/ConvMVVM3.WPF.svg?style=for-the-badge&logo=nuget&label=NGET%20ConvMVVM3.WPF" />

</a>
</div>




**ConvMVVM3 (Convergence MVVM3)** is an MVVM framework for WPF on Windows, inspired by Prism and Community Toolkit. WPF is currently the only supported UI framework. The Core, Host, and SourceGenerator packages target .NET Standard 2.0, but this does not imply support for other UI frameworks.

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

### 🚀 **UIDispatcher** (WPF)
UI thread dispatcher with a WPF implementation:

```csharp
// Register the WPF dispatcher
services.AddWPFUIDispatcher();      // WPF

// Dispatch to the WPF UI thread
await dispatcher.InvokeAsync(() => Title = "Updated");
```

### 🔌 **Smart Dependency Injection** (UNIQUE - Built-in)
Lightweight service container with addon system - **No external DI required**:

```csharp
// Built-in container - Zero external dependencies
services.AddSingleton<IMyService, MyService>();
```

### 📡 **WeakReference Messenger** (Weak References)
Thread-safe messaging with automatic cleanup:

```csharp
// Messaging using weak references
WeakReferenceMessenger.Default.Send<Message>(this, newData);
```

### 🎯 **Region System** (Prism-inspired, WPF)
Built-in region management for WPF controls:

```xml
<!-- WPF region host -->
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
├── UIDispatcher                  # UI dispatcher abstraction
├── WeakReferenceMessenger        # Memory-safe messaging system
├── Commands/
│   ├── RelayCommand              # Synchronous command implementation
│   └── AsyncRelayCommand         # Async command with cancellation
└── DependencyInjection/          # DI abstractions (implementation in Host)
```

### **WPF Implementation** (Windows)
```
ConvMVVM3.WPF/                    # WPF-specific implementation
├── Regions/                      # Region system (Prism-inspired)
├── WPFUIDispatcher               # WPF-specific dispatcher
├── Behaviors/                    # Behaviors / triggers / actions
└── Locator/                      # ViewModel auto-wiring
```

### **Source Generation**
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
ConvMVVM3.SourceGenerator/        # Property and command source generator
ConvMVVM3.WPF/                    # WPF-specific interactivity + regions
ConvMVVM3.Host/                   # DI host implementation
ConvMVVM3.WPF.Tests/              # WPF unit tests
ConvMVVM3.Tests/                  # Core unit tests
```

---

## WPF examples

All ten examples target `net10.0-windows` and consume the published
`ConvMVVM3.WPF` 1.2.0 NuGet package. They use XAML
`ViewModelLocator.AutoWireViewModel="True"`; View constructors only call
`InitializeComponent()`. Run them on Windows with the .NET 10 SDK installed.

| Project | Focus | What to try |
|---|---|---|
| `ExampleActivatorUtilities` | DI services plus runtime arguments | Enter a name and age. Compare `CreateInstance` with a cached `CreateFactory`, inspect the greeting from the injected service, and try invalid input. |
| `ExampleModuleManager` | Automatic and on-demand initialization | Compare A/B initialization with C before and after its first use. Open A, C, A and C again; C must reopen and its visit count must increase. |
| `ExampleRegionManager` | SingleView navigation and MultiView hosts | Change customers and inspect the ViewModel navigation history. Add distinct transient items, select the second list item, clear the list, and add again. |
| `ExampleMvvmBasics` | Generated properties and commands | Edit a name, observe dependent values, disable saving with CanExecute, and reset the form. |
| `ExampleAsyncOperations` | Async commands and BusyScope | Complete a local job, cancel it, simulate a failure, and retry. |
| `ExampleMessaging` | WeakReferenceMessenger and ObservableRecipient | Send to Alpha/Beta, toggle recipient activation, and move a receiver between channels. |
| `ExampleUIDispatcher` | UI dispatch from background work | Compare worker/UI thread IDs and cancel a background operation safely. |
| `ExampleBehaviors` | XAML event triggers and actions | Invoke a command with a parameter, honor CanExecute, call a method, and change a control property. |
| `ExampleCollections` | Batch collection notifications | Compare 1,000 individual additions with a single range reset; replace, remove and clear items. |
| `ExampleResourceLifecycle` | Disposable ownership and OperationGuard | Replace subscriptions, clear/dispose groups, reject reentry and recover after failure. |

From the repository root, run one command at a time:

```powershell
dotnet run --project ConvMVVM3/ExampleActivatorUtilities -c Release
dotnet run --project ConvMVVM3/ExampleModuleManager -c Release
dotnet run --project ConvMVVM3/ExampleRegionManager -c Release
```

Build individual example projects explicitly when validating Release: the
solution configuration excludes examples from its Release build.

The Region example registers the detail View/ViewModel as singletons, so repeated
navigation preserves the instance ID. Its ViewModel implements `INavigationAware`
and receives the selected `Customer` in `NavigationParameters`. Instance reuse
comes from DI lifetime, not a view-reuse search by `IsNavigationTarget`. Both
multi-view regions use transient Views and ViewModels. Item numbers and IDs make
distinct instances visible. Adding multi-view items does not invoke SingleView
navigation callbacks.

All modules register services at startup. `OnDemand` delays C's initialization,
not its registration or assembly loading. Module categories are not observable,
so the example refreshes status text after initialization. The module example remains
in one project; loading separate plugin DLLs is not demonstrated.

### Run a new example

```powershell
dotnet run --project ConvMVVM3/ExampleMvvmBasics -c Release
dotnet run --project ConvMVVM3/ExampleAsyncOperations -c Release
dotnet run --project ConvMVVM3/ExampleMessaging -c Release
dotnet run --project ConvMVVM3/ExampleUIDispatcher -c Release
dotnet run --project ConvMVVM3/ExampleBehaviors -c Release
dotnet run --project ConvMVVM3/ExampleCollections -c Release
dotnet run --project ConvMVVM3/ExampleResourceLifecycle -c Release
```

The examples use only APIs available in 1.2.0. Predicate-based and parameterized
commands are constructed explicitly where generator options are not supported.
Async examples expose their own observable Busy/progress/error state and catch
exceptions inside their operations, including execution through ICommand.
Cancellation is cooperative; the local job simulation does not require a server.
The messaging sample owns its receivers and disposes subscriptions on window exit;
UI-thread dispatch is demonstrated separately rather than implied by Messenger.

### Reproducible example checks

The `Examples.SmokeTests` console runner uses a separate STA/dispatcher process for
each sample because WPF Application and ServiceLocator are process-wide. It boots
the real sample, initializes XAML, exercises commands and actual routed Click events,
checks binding diagnostics, and optionally renders a preview without showing a window.
It covers successful completion, cancellation, failure/retry, message channels,
UI-thread affinity, trigger detachment, range notifications and subscription cleanup.
These are automated runtime checks, not manual mouse/keyboard acceptance tests.
The validation project is under the solution's Validation folder and is not a NuGet package.

From the repository root:

```powershell
# Build and check all ten examples.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Examples.ps1

# Check one sample and save a rendered preview under the ignored build directory.
./scripts/Test-Examples.ps1 -Samples ExampleAsyncOperations -PreviewDirectory ConvMVVM3/Examples.SmokeTests/bin/previews
```

The first command applies the execution policy only to its child process; it does
not change machine/user policy. Use the second form when your current policy
already permits local scripts. Existing nullable warnings from the 1.2.0 source
generator may appear during a clean build; these are distinct from runtime binding
diagnostics, which cause the checks to fail.

## 🛠️ Installation

```bash
dotnet add package ConvMVVM3.WPF
```

---

## 🚀 Quick Start

### 1) Basic WPF ViewModel with Source Generation

```csharp
// ViewModel for a WPF application
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

### 3) WPF UIDispatcher Integration

```csharp
// Register in the WPF application service registry
services.AddWPFUIDispatcher();       // WPF

// Inject IUIDispatcher into the ViewModel
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

        // Update on the WPF UI thread
        await _dispatcher.InvokeAsync(() => Title = data);
    }
}
```

### 4) WPF Region Management

```xml
<!-- WPF region host -->
<Grid>
    <ContentControl convMVVM3:RegionPlugin.RegionName="MainContent" />
</Grid>
```

```csharp
// Navigate within a WPF region
_regionManager.RequestNavigate("MainContent", typeof(HomeView));
```

#### ViewModel navigation callbacks (WPF)

`Bootstrapper` registers `AddWPFRegionManager()` automatically. If configuring the
container without a bootstrapper, call `services.AddWPFRegionManager()` instead
of the Core-only `AddRegionManager()`.

Keep `ViewModelLocator.AutoWireViewModel="True"` on the View in XAML and implement
`INavigationAware` on its ViewModel. The View only needs `InitializeComponent()`;
constructor injection of the ViewModel and manual callback forwarding are unnecessary.
The WPF adapter completes opted-in wiring before navigation callbacks, without
waiting for `Loaded`. It preserves existing DataContexts, explicit nulls, and bindings.
An opted-in View whose DataContext cannot be connected causes navigation to fail
before the current content is replaced.

```csharp
public class CustomerViewModel : ObservableObject, INavigationAware
{
    public bool IsNavigationTarget(NavigationContext context) => true;

    public void OnNavigatedTo(NavigationContext context)
    {
        if (context.Parameters.TryGetValue("CustomerId", out var customerId))
        {
            // Initialize the screen for this customer.
        }
    }

    public void OnNavigatedFrom(NavigationContext context)
    {
        // Store screen state or cancel outstanding work.
    }
}

regionManager.RequestNavigate<CustomerView>("MainContent",
    new NavigationParameters { ["CustomerId"] = 123 });
```

The single-view `RequestNavigate` overloads (type, generic, and service key) notify
both the View and its DataContext, in that order, using the same navigation context.
The same instance is never notified twice by the adapter. Remove any existing
View-to-ViewModel forwarding code when enabling automatic dispatch to both.
Each participant's `IsNavigationTarget` still gates only its own `OnNavigatedTo`;
it does not cancel navigation or implement Prism-style view reuse. Multi-view item
addition, `Active`, and `ClearRegion` retain their existing notification behavior.

`ViewModelLocator.EnsureViewModelConnected(view)` can also be called explicitly
on the View's dispatcher thread. It returns whether a non-null DataContext exists;
repeated calls preserve an already connected transient ViewModel. Explicit local
nulls and bindings are never overwritten; opt out of automatic wiring if they are
intended to remain null during navigation.

The WPF 1.2.0 project uses NuGet references to Core, Host, and SourceGenerator
with dependency ranges `[1.2.0,1.3.0)`. These packages must be available from the
NuGet feed before building WPF; no local Core project reference is required.

### 5) Weak Reference Messaging

```csharp
// Sender
WeakReferenceMessenger.Default.Send<DataUpdatedMessage>(this, newData);

// Receiver
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

## 🎯 Supported Platform

WPF on Windows is currently the only supported UI framework.

| Component | Target frameworks |
|-----------|-------------------|
| Core, Host, SourceGenerator | .NET Standard 2.0 |
| WPF integration | .NET Framework 4.8; .NET 6, 7, 8, 9, 10 for Windows |
| Example WPF applications | .NET 10 for Windows |

The framework includes WPF implementations for regions, UI dispatching, ViewModel location, and behaviors.

---

## 📄 License

MIT License - see [LICENSE](LICENSE) file

---

**ConvMVVM3**: An MVVM framework for WPF combining modular application composition with property and command source generation.

Support for additional UI frameworks is not currently provided.
