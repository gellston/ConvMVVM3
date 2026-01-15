# ConvMVVM3 (Convergence MVVM3)

![ConvMVVM3 Logo](logo.png)

**ConvMVVM3** is a free, modern MVVM (Model-View-ViewModel) library for WPF applications, inspired by Community Toolkit library and Prism frameworks. This library provides a comprehensive set of tools and patterns to help developers build maintainable, scalable, and testable WPF applications using the MVVM pattern.

## ✨ Features

### 🔧 Dependency Injection
- **Service Container**: Modern DI container with scoped services support
- **Addon System**: Modular architecture with plugin-like functionality
- **Service Lifetime Management**: Singleton, Scoped, and Transient service lifetimes
- **Addon Attributes**: Easy addon registration and discovery

### 🎯 MVVM Core
- **Observable Object**: Base class implementing INotifyPropertyChanged
- **Commands**: Relay Command and Async Relay Command support
- **Source Generation**: Automatic property and command generation with attributes
- **Observable Properties**: Declarative property change notification
- **Cross-property Notification**: Automatic notification of related properties

### 📨 Messaging
- **Weak Reference Messenger**: Memory-safe messaging system
- **Message Types**: Support for various message patterns
- **Recipient Registration**: Easy message subscription management
- **Observable Recipient**: Base class for message-aware ViewModels

### 🎭 WPF Behaviors
- **Trigger Actions**: Event-driven actions and behaviors
- **Control Storyboard**: Animation and storyboard control
- **State Management**: Visual state transition support
- **Command Invocation**: Bind command actions to events

### 🔨 Source Generator
- **Automatic Code Generation**: Generate boilerplate MVVM code
- **Attribute-Driven**: Use attributes to generate properties and commands
- **Compile-Time Validation**: Early error detection and optimization
- **Performance Optimized**: Generated code runs at native speed

## 📁 Project Structure

```
ConvMVVM3/
├── ConvMVVM3.Core/              # Core MVVM library
│   ├── DependencyInjection/         # DI container implementation
│   ├── Mvvm/                    # MVVM core components
│   │   ├── Commands/             # Command implementations
│   │   ├── Attributes/           # Source generator attributes
│   │   ├── Messaging/            # Messaging system
│   │   └── ObservableObject.cs  # Base observable class
├── ConvMVVM3.SourceGenerator/     # Roslyn source generator
├── ConvMVVM3.WPF/              # WPF-specific behaviors
│   └── Behaviors/              # WPF behaviors and triggers
├── ConvMVVM3.Host/              # DI host implementation
└── ConvMVVM3.Tests/             # Unit tests
```

## ✅ Completed Features

### Core Library (ConvMVVM3.Core)
- [x] **Dependency Injection Framework**
  - [x] Service container with lifetime management
  - [x] Addon system for modular architecture
  - [x] Service registration and resolution
  - [x] Scoped services support

- [x] **MVVM Implementation**
  - [x] ObservableObject base class
  - [x] RelayCommand and AsyncRelayCommand
  - [x] Messaging system with WeakReferenceMessenger
  - [x] ObservableRecipient for message-aware ViewModels

- [x] **Source Generator Attributes**
  - [x] ObservableProperty attribute
  - [x] RelayCommand attribute
  - [x] AsyncRelayCommand attribute
  - [x] Property change notification attributes
  - [x] Cross-property notification support

### Source Generator (ConvMVVM3.SourceGenerator)
- [x] **Code Generation**
  - [x] Roslyn-based source generator
  - [x] Property generation from attributes
  - [x] Command generation from attributes
  - [x] Compile-time validation and optimization

### WPF Integration (ConvMVVM3.WPF)
- [x] **Behaviors and Triggers**
  - [x] EventTrigger for event-driven actions
  - [x] TriggerAction base classes
  - [x] Behavior base classes
  - [x] Control storyboard actions
  - [x] Property change actions
  - [x] Command invocation actions
  - [x] State management actions
  - [x] Method calling actions

### Testing Framework (ConvMVVM3.Tests)
- [x] **Comprehensive Test Coverage**
  - [x] Unit tests for core components
  - [x] Integration tests for DI container
  - [x] Message system testing
  - [x] Command testing framework

## 🚀 Upcoming Features

### 🔮 Future Roadmap
- [ ] **Enhanced Source Generation**
  - [ ] Collection property generation
  - [] Validation attribute support
  - [] Navigation command generation
  - [ ] Async property generation

- [ ] **Advanced DI Features**
  - [ ] Conditional service registration
  - [ ] Service decorator pattern
  - [ ] Interceptor support
  - [ ] Configuration-based registration

- [ ] **WPF Enhancements**
  - [ ] Blend behaviors integration
  - [ ] Custom markup extensions
  - [ ] Template selectors
  - [ ] Value converters

- [ ] **Performance Optimizations**
  - [ ] Compiled binding support
  - [ ] Memory usage optimization
  - [ ] UI virtualization helpers
  - [ ] Async UI helpers

- [ ] **Developer Experience**
  - [ ] Design-time support
  - [ ] Debugging helpers
  - [ ] Performance profilers
  - [ ] Documentation samples

## 🛠️ Installation

### NuGet Package
```bash
dotnet add package ConvMVVM3
```

### Manual Installation
1. Clone the repository
2. Add reference to required projects:
   - `ConvMVVM3.Core` for MVVM functionality
   - `ConvMVVM3.WPF` for WPF-specific features
   - `ConvMVVM3.SourceGenerator` for code generation

## 📖 Quick Start

### Basic MVVM Usage

```csharp
// ViewModel with source generation
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "Hello World!";
    
    [RelayCommand]
    private void ShowMessage() => MessageBox.Show(Title);
    
    [AsyncRelayCommand]
    private async Task LoadDataAsync()
    {
        // Async operation
        await Task.Delay(1000);
        Title = "Data Loaded!";
    }
}
```

### Dependency Injection

```csharp
// Service registration
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddService<ILogger, ConsoleLogger>();
        services.AddService<IDataService, DataService>(ServiceLifetime.Singleton);
        
        // Add addon
        services.AddAddon<NotificationAddon>();
    }
}

// Service resolution
public class MainViewModel : ObservableObject
{
    private readonly ILogger _logger;
    
    public MainViewModel(IServiceResolver resolver)
    {
        _logger = resolver.ResolveService<ILogger>();
    }
}
```

### Messaging

```csharp
// Send message
messenger.Send(new DataChangedMessage(data));

// Receive message
public partial class DataViewModel : ObservableRecipient, IRecipient<DataChangedMessage>
{
    protected override void OnActivated()
    {
        Messenger.Register<DataChangedMessage>(this);
    }
    
    public void Receive(DataChangedMessage message)
    {
        // Handle message
        UpdateData(message.Data);
    }
}
```

### WPF Behaviors

```xml
<!-- Event to command binding -->
<Button Content="Click Me">
    <i:Interaction.Triggers>
        <i:EventTrigger EventName="Click">
            <i:InvokeCommandAction Command="{Binding ShowMessageCommand}" />
        </i:EventTrigger>
    </i:Interaction.Triggers>
</Button>

<!-- Property change behavior -->
<TextBox Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}">
    <i:Interaction.Behaviors>
        <i:CallMethodAction 
            MethodName="OnSearchTextChanged"
            TargetObject="{Binding}" />
    </i:Interaction.Behaviors>
</TextBox>
```

## 🧪 Build Status

- ✅ **ConvMVVM3.Core** - All features implemented and tested
- ✅ **ConvMVVM3.SourceGenerator** - Full code generation support
- ✅ **ConvMVVM3.WPF** - Complete WPF behavior system
- ✅ **ConvMVVM3.Tests** - Comprehensive test coverage
- ✅ **Build System** - All projects compile successfully

## 🎯 Target Frameworks

- **.NET Standard 2.0** - Core library (maximum compatibility)
- **.NET 10.0** - Source generator (latest features)
- **.NET 10.0 Windows** - WPF integration (Windows-specific)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

### Development Setup
1. Clone the repository
2. Ensure you have .NET 10.0 SDK installed
3. Run `dotnet build` to verify all projects compile
4. Run tests with `dotnet test`

## 📞 Support

If you have any questions or need help, please:
- Open an issue on GitHub
- Check the documentation
- Review the sample projects

## 🙏 Acknowledgments

- Inspired by **Community Toolkit** MVVM patterns
- Architecture influenced by **Prism** framework
- Source generator techniques from **Roslyn** best practices
- WPF behaviors inspired by **Expression Blend** SDK

---

**ConvMVVM3** - Building modern WPF applications with confidence! 🚀