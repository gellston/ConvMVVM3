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
    /// Tests for UIDispatcher DI registration.
    /// </summary>
    public class UIDispatcherDITests
    {
        [Fact]
        public void AddWPFUIDispatcher_Registers_Singleton()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddWPFUIDispatcher();
            
            // Assert
            var serviceProvider = new ServiceContainer(services);
            var dispatcher = serviceProvider.GetService<IUIDispatcher>();
            
            Assert.NotNull(dispatcher);
            Assert.IsType<WPFUIDispatcher>(dispatcher);
        }

        [Fact]
        public void AddWPFUIDispatcher_Throws_When_Services_Is_Null()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((IServiceRegistry)null).AddWPFUIDispatcher());
        }

        [Fact]
        public void UIDispatcher_Can_Be_Resolved_From_ServiceProvider()
        {
            // Arrange & Act & Assert
            StaThreadHelper.Run(() =>
            {
                var services = new ServiceCollection();
                services.AddWPFUIDispatcher();
                var serviceProvider = new ServiceContainer(services);

                // Act
                var dispatcher = serviceProvider.GetService<IUIDispatcher>();

                // Assert
                Assert.NotNull(dispatcher);
                Assert.IsType<WPFUIDispatcher>(dispatcher);
            });
        }
    }
}