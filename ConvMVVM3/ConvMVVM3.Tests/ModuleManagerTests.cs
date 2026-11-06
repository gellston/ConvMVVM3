using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Modules;
using ConvMVVM3.Core.Mvvm.Modules.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.Host.DependencyInjection;
using Xunit;

namespace ConvMVVM3.Tests;

public class ModuleManagerTests
{
    [Theory]
    [InlineData(InitializationMode.WhenAvailable, true, 1)]
    [InlineData(InitializationMode.OnDemand, true, 0)]
    [InlineData(InitializationMode.WhenAvailable, false, 0)]
    public void InitializeModules_InitializesOnlyRegisteredAutomaticModulesOnce(
        InitializationMode mode, bool registered, int expected)
    {
        using var container = new ServiceContainer(new ServiceCollection());
        var module = new TestModule();
        var category = Category(mode, registered);
        var manager = new ModuleManager(container, new() { module }, new() { category });
        manager.InitializeModules();
        manager.InitializeModules();
        Assert.Equal(expected, module.InitializeCount);
        Assert.Equal(expected == 1, category.IsInitialized);
        if (expected == 1) Assert.Same(container, module.Resolver);
    }

    [Fact]
    public void InitializeModule_InitializesOnDemandModuleByNameOnce()
    {
        using var container = new ServiceContainer(new ServiceCollection());
        var module = new TestModule();
        var category = Category(InitializationMode.OnDemand);
        var manager = new ModuleManager(container, new() { module }, new() { category });
        manager.InitializeModule("unknown");
        Assert.Equal(0, module.InitializeCount);
        manager.InitializeModule("Test");
        manager.InitializeModule("Test");
        Assert.Equal(1, module.InitializeCount);
        Assert.True(category.IsInitialized);
    }

    [Fact]
    public void FailedInitialization_DoesNotMarkInitializedAndCanRetry()
    {
        using var container = new ServiceContainer(new ServiceCollection());
        var module = new TestModule { Fail = true };
        var category = Category(InitializationMode.WhenAvailable);
        var manager = new ModuleManager(container, new() { module }, new() { category });
        Assert.Throws<InvalidOperationException>(() => manager.InitializeModules());
        Assert.False(category.IsInitialized);
        module.Fail = false;
        manager.InitializeModules();
        Assert.True(category.IsInitialized);
        Assert.Equal(2, module.InitializeCount);
    }

    [Fact]
    public void ConfigureRegions_PassesRegisteredManagerAndRunsOnlyOnce()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IRegionManager>(resolver => new RegionManager((IServiceContainer)resolver));
        using var container = new ServiceContainer(services);
        var module = new TestModule();
        var category = Category(InitializationMode.OnDemand);
        var manager = new ModuleManager(container, new() { module }, new() { category });
        manager.ConfigureRegions();
        manager.ConfigureRegions();
        Assert.Equal(1, module.ConfigureCount);
        Assert.Same(container.GetRequiredService<IRegionManager>(), module.Regions);
        Assert.True(category.IsRegionConfigured);
        Assert.False(category.IsInitialized);
    }

    private static ModuleCategory Category(InitializationMode mode, bool registered = true)
        => new("Test", "1.0.0", mode, Array.Empty<string>()) { IsRegistered = registered };

    [Module("Test")]
    private sealed class TestModule : IModule
    {
        public int InitializeCount { get; private set; }
        public int ConfigureCount { get; private set; }
        public bool Fail { get; set; }
        public IServiceResolver? Resolver { get; private set; }
        public IRegionManager? Regions { get; private set; }
        public void RegisterServices(IServiceRegistry registry) { }
        public void ConfigureRegions(IRegionManager regionManager) { ConfigureCount++; Regions = regionManager; }
        public void OnInitialized(IServiceResolver resolver)
        {
            InitializeCount++;
            if (Fail) throw new InvalidOperationException("module failed");
            Resolver = resolver;
        }
    }
}
