using ConvMVVM3.Host.DependencyInjection;
using ConvMVVM3.Host.DependencyInjection;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using System;
using Xunit;

namespace ConvMVVM3.Tests.DependencyInjection;

public class ServiceContainerTests
{
    [Fact]
    public void Resolve_Returns_Registered_Service()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        var container = new ServiceContainer(services);

        // Act
        var service = container.GetRequiredService<ITestService>();

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void GetRequiredService_Throws_When_Service_Not_Registered()
    {
        // Arrange
        var services = new ServiceCollection();
        var container = new ServiceContainer(services);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            container.GetRequiredService<ITestService>());
    }

    [Fact]
    public void GetService_Returns_Null_When_Service_Not_Registered()
    {
        // Arrange
        var services = new ServiceCollection();
        var container = new ServiceContainer(services);

        // Act
        var service = container.GetService<ITestService>();

        // Assert
        Assert.Null(service);
    }

    [Fact]
    public void Scoped_Services_Are_Isolated_Per_Scope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IScopedService, ScopedService>();
        var container = new ServiceContainer(services);

        // Act
        using var scope1 = container.CreateScope();
        using var scope2 = container.CreateScope();

        var service1 = scope1.ServiceProvider.GetRequiredService<IScopedService>();
        var service2 = scope2.ServiceProvider.GetRequiredService<IScopedService>();

        // Assert
        Assert.NotNull(service1);
        Assert.NotNull(service2);
        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void Transient_Services_Are_New_Instances_Each_Time()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<ITransientService, TransientService>();
        var container = new ServiceContainer(services);

        // Act
        var service1 = container.GetRequiredService<ITransientService>();
        var service2 = container.GetRequiredService<ITransientService>();

        // Assert
        Assert.NotNull(service1);
        Assert.NotNull(service2);
        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void Singleton_Services_Are_Same_Instance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ISingletonService, SingletonService>();
        var container = new ServiceContainer(services);

        // Act
        var service1 = container.GetRequiredService<ISingletonService>();
        var service2 = container.GetRequiredService<ISingletonService>();

        // Assert
        Assert.NotNull(service1);
        Assert.NotNull(service2);
        Assert.Same(service1, service2);
    }

    [Fact]
    public void Service_With_Dependencies_Is_Resolved_Correctly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDependency, Dependency>();
        services.AddTransient<IServiceWithDependency, ServiceWithDependency>();
        var container = new ServiceContainer(services);

        // Act
        var service = container.GetRequiredService<IServiceWithDependency>();

        // Assert
        Assert.NotNull(service);
        Assert.IsType<ServiceWithDependency>(service);
        Assert.NotNull(service.Dependency);
        Assert.IsType<Dependency>(service.Dependency);
    }

    [Fact]
    public void Dispose_Cleans_Up_Scoped_Services()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IDisposableService, DisposableService>();
        var container = new ServiceContainer(services);

        DisposableService instance;
        using (var scope = container.CreateScope())
        {
            instance = (DisposableService)scope.ServiceProvider.GetRequiredService<IDisposableService>();
        }

        // Act - scope is disposed
        // Assert
        Assert.True(instance.IsDisposed);
    }

    [Fact]
    public void Constructor_Throws_When_Services_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ServiceContainer(null));
    }

    // Test interfaces and classes
    public interface ITestService { }
    public class TestService : ITestService { }

    public interface IScopedService { }
    public class ScopedService : IScopedService { }

    public interface ITransientService { }
    public class TransientService : ITransientService { }

    public interface ISingletonService { }
    public class SingletonService : ISingletonService { }

    public interface IDependency { }
    public class Dependency : IDependency { }

    public interface IServiceWithDependency
    {
        IDependency Dependency { get; }
    }
    public class ServiceWithDependency : IServiceWithDependency
    {
        public IDependency Dependency { get; }

        public ServiceWithDependency(IDependency dependency)
        {
            Dependency = dependency;
        }
    }

    public interface IDisposableService { }
    public class DisposableService : IDisposableService, IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}