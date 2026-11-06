using ConvMVVM3.Host.DependencyInjection;
using Xunit;

namespace ConvMVVM3.Tests;

public class ServiceContainerEdgeCaseTests
{
    [Fact]
    public void ScopedService_IsReusedWithinScopeButCannotResolveFromRoot()
    {
        var services = new ServiceCollection();
        services.AddScoped<Tracked>();
        using var container = new ServiceContainer(services);
        Assert.Throws<InvalidOperationException>(() => container.GetRequiredService<Tracked>());
        using var scope = container.CreateScope();
        var first = scope.ServiceProvider.GetRequiredService<Tracked>();
        Assert.Same(first, scope.ServiceProvider.GetRequiredService<Tracked>());
        scope.Dispose();
        Assert.Equal(1, first.DisposeCount);
        Assert.Throws<ObjectDisposedException>(() => scope.ServiceProvider.GetRequiredService<Tracked>());
    }

    [Fact]
    public void RootDisposal_ReleasesSingletonAndTransientExactlyOnce()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Tracked>();
        services.AddTransient<OtherTracked>();
        var container = new ServiceContainer(services);
        var singleton = container.GetRequiredService<Tracked>();
        var transient = container.GetRequiredService<OtherTracked>();
        using (var scope = container.CreateScope())
            Assert.Same(singleton, scope.ServiceProvider.GetRequiredService<Tracked>());
        Assert.Equal(0, singleton.DisposeCount);
        container.Dispose();
        container.Dispose();
        Assert.Equal(1, singleton.DisposeCount);
        Assert.Equal(1, transient.DisposeCount);
        Assert.Throws<ObjectDisposedException>(() => container.GetRequiredService<Tracked>());
        Assert.Throws<ObjectDisposedException>(() => container.CreateScope());
    }

    [Fact]
    public void MultipleRegistrations_ResolveLastAndEnumerateAllInOrder()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IService, First>();
        services.AddSingleton<IService, Second>();
        using var container = new ServiceContainer(services);
        var all = container.GetServices<IService>().ToArray();
        Assert.Collection(all, item => Assert.IsType<First>(item), item => Assert.IsType<Second>(item));
        Assert.Same(all[1], container.GetRequiredService<IService>());
        Assert.Empty(container.GetServices<IDisposable>());
    }

    [Fact]
    public void CircularDependency_ThrowsWithDependencyPath()
    {
        var services = new ServiceCollection();
        services.AddTransient<CycleA>();
        services.AddTransient<CycleB>();
        using var container = new ServiceContainer(services);
        var error = Assert.Throws<InvalidOperationException>(() => container.GetRequiredService<CycleA>());
        Assert.Contains(nameof(CycleA), error.ToString());
        Assert.Contains(nameof(CycleB), error.ToString());
        Assert.Contains("Circular", error.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ActivatorUtilities_MixesRuntimeArgumentsWithInjectedServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IService, First>();
        using var container = new ServiceContainer(services);
        var direct = ActivatorUtilities.CreateInstance<Details>(container, 42);
        var factory = ActivatorUtilities.CreateFactory(typeof(Details), new[] { typeof(int) });
        var created = Assert.IsType<Details>(factory(container, new object[] { 17 }));
        Assert.Equal(42, direct.Id);
        Assert.Equal(17, created.Id);
        Assert.Same(container.GetRequiredService<IService>(), direct.Service);
        Assert.Same(direct.Service, created.Service);
        Assert.Throws<ArgumentException>(() => factory(container, Array.Empty<object>()));
        Assert.Throws<ArgumentException>(() => factory(container, new object[] { "wrong" }));
    }

    [Fact]
    public void ActivatorUtilities_PrefersRegisteredInstanceAndCreatesUnregisteredType()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Tracked>();
        using var container = new ServiceContainer(services);
        Assert.Same(container.GetRequiredService<Tracked>(),
            ActivatorUtilities.GetServiceOrCreateInstance<Tracked>(container));
        using var created = ActivatorUtilities.GetServiceOrCreateInstance<OtherTracked>(container);
        Assert.NotNull(created);
    }

    public interface IService { }
    public sealed class First : IService { }
    public sealed class Second : IService { }
    public class Tracked : IDisposable
    {
        public int DisposeCount { get; private set; }
        public void Dispose() => DisposeCount++;
    }
    public sealed class OtherTracked : Tracked { }
    public sealed class CycleA(CycleB dependency) { public CycleB Dependency { get; } = dependency; }
    public sealed class CycleB(CycleA dependency) { public CycleA Dependency { get; } = dependency; }
    public sealed class Details(IService service, int id)
    {
        public IService Service { get; } = service;
        public int Id { get; } = id;
    }
}
