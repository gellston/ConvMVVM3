# ConvMVVM3

<p align="center">
   <img src="logo.png" alt="ConvMVVM3 Logo" width="400"/>
</p>

**ConvMVVM3 (Convergence MVVM3)** is a free MVVM library for WPF inspired by Community Toolkit and Prism frameworks.

## ✅ Features

- **Dependency Injection** - Service container with addon system
- **MVVM Core** - ObservableObject, Commands, Messaging  
- **UIDispatcher** - Microsoft-compatible UI thread dispatcher
- **Source Generator** - Automatic property/command generation
- **WPF Behaviors** - Triggers, actions, event binding
- **Testing Framework** - Comprehensive unit test coverage (107 tests)

## 📁 Project Structure

```
ConvMVVM3.Core/          # Core MVVM library + UIDispatcher
ConvMVVM3.SourceGenerator/  # Roslyn source generator
ConvMVVM3.WPF/           # WPF-specific behaviors
ConvMVVM3.Host/           # DI host implementation
ConvMVVM3.Tests/          # Unit tests (107 passing)
```

## 🛠️ Installation

```bash
dotnet add package ConvMVVM3
```

## 📖 Quick Start

### Basic ViewModel

```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "Hello World!";
    
    [RelayCommand]
    private void ShowMessage() => MessageBox.Show(Title);
}
```

### UIDispatcher (NEW!)

```csharp
// Setup
services.AddWPFUIDispatcher();

// Usage
await dispatcher.InvokeAsync(() => 
{
    Title = "Updated from background thread";
});
```

### WPF Behavior

```xml
<Button Content="Click Me">
    <i:Interaction.Triggers>
        <i:EventTrigger EventName="Click">
            <i:InvokeCommandAction Command="{Binding ShowMessageCommand}" />
        </i:EventTrigger>
    </i:Interaction.Triggers>
</Button>
```

## 🎯 Build Status

✅ **107/107 tests passing** (100%)
✅ All projects build successfully on .NET 10.0
✅ UIDispatcher fully implemented and tested

## 📄 License

MIT License - see [LICENSE](LICENSE) file

---

**ConvMVVM3**: Modern MVVM framework for WPF applications