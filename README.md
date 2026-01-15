# ConvMVVM3

<p align="center">
  <img src="logo.png" alt="ConvMVVM3 Logo" width="200"/>
</p>

**ConvMVVM3 (Convergence MVVM3)** is a free MVVM library for WPF inspired by Community Toolkit and Prism frameworks.

## ✅ Completed Features

- **Dependency Injection** - Service container with addon system
- **MVVM Core** - ObservableObject, Commands, Messaging
- **Source Generator** - Automatic property/command generation  
- **WPF Behaviors** - Triggers, actions, event binding
- **Testing Framework** - Comprehensive unit test coverage

## 🚀 Upcoming Features

- Enhanced source generation with validation
- Advanced DI features (decorators, interceptors)
- WPF markup extensions
- Performance optimizations
- Design-time support

## 📁 Project Structure

```
ConvMVVM3.Core/          # Core MVVM library
ConvMVVM3.SourceGenerator/  # Roslyn source generator
ConvMVVM3.WPF/           # WPF-specific behaviors
ConvMVVM3.Host/           # DI host implementation
ConvMVVM3.Tests/          # Unit tests
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
    
    [AsyncRelayCommand]
    private async Task LoadDataAsync()
    {
        await Task.Delay(1000);
        Title = "Data Loaded!";
    }
}
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

✅ All projects build successfully on .NET 10.0

## 📄 License

MIT License - see [LICENSE](LICENSE) file