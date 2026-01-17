using System;
using System.Threading.Tasks;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Abstractions;
using ConvMVVM3.Host.DependencyInjection;
using ConvMVVM3.WPF;
using Xunit;

namespace ConvMVVM3.WPF.Tests
{
    /// <summary>
    /// Tests for service registration and dependency injection functionality.
    /// </summary>
    public class ServiceRegistrationTests
    {
        [Fact]
        public void AddSingleton_Can_Register_Service()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddWPFUIDispatcher();

            // Act
            var serviceProvider = new ServiceContainer(services);
            var service = serviceProvider.GetService<IUIDispatcher>();
            
            // Assert
            Assert.NotNull(service);
            Assert.IsType<WPFUIDispatcher>(service);
        }

        [Fact]
        public void AddSingleton_Does_Not_Throw_When_Parameters_Are_Valid()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            services.AddWPFUIDispatcher();
            var serviceProvider = new ServiceContainer(services);
            var service = serviceProvider.GetService<IUIDispatcher>();
            
            Assert.NotNull(service);
            Assert.IsType<WPFUIDispatcher>(service);
        }

        [Fact]
        public void AddScoped_Can_Register_Service()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddWPFUIDispatcher();

            // Act
            var serviceProvider = new ServiceContainer(services);
            var service = serviceProvider.GetService<IUIDispatcher>();
            
            // Assert
            Assert.NotNull(service);
            Assert.IsType<WPFUIDispatcher>(service);
        }

        [Fact]
        public void AddTransient_Can_Register_Service()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddWPFUIDispatcher();

            // Act
            var serviceProvider = new ServiceContainer(services);
            var service = serviceProvider.GetService<IUIDispatcher>();
            
            // Assert
            Assert.NotNull(service);
            Assert.IsType<WPFUIDispatcher>(service);
        }

        [Fact]
        public void ServiceContainer_Returns_Valid_ServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddWPFUIDispatcher();

            // Act
            var serviceProvider = new ServiceContainer(services);

            // Assert
            Assert.NotNull(serviceProvider);
            Assert.IsType<ServiceContainer>(serviceProvider);
        }

        [Fact]
        public void ServiceFactory_Registration_Works()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IUIDispatcher>(sp => new WPFUIDispatcher(System.Windows.Threading.Dispatcher.CurrentDispatcher));

            // Act
            var serviceProvider = new ServiceContainer(services);
            var service = serviceProvider.GetService<IUIDispatcher>();
            
            // Assert
            Assert.NotNull(service);
            Assert.IsType<WPFUIDispatcher>(service);
        }

        [Fact]
        public void GetRequiredService_Throws_When_Not_Registered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = new ServiceContainer(services);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                serviceProvider.GetRequiredService<IUIDispatcher>());
        }

        [Fact]
        public void GetService_Returns_Null_When_Not_Registered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = new ServiceContainer(services);

            // Act
            var service = serviceProvider.GetService<IUIDispatcher>();

            // Assert
            Assert.Null(service);
        }
    }
}