using System;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.UIDispatcher.Abstractions;
using ConvMVVM3.Host.DependencyInjection;
using ConvMVVM3.WPF.UIDispatcher;
using Xunit;

namespace ConvMVVM3.WPF.Tests
{
    public class ServiceRegistrationTests
    {
        [Fact]
        public void AddWPFUIDispatcher_RegistersSingletonAcrossScopes()
        {
            var services = new ServiceCollection();
            Assert.Same(services, services.AddWPFUIDispatcher());
            using (var container = new ServiceContainer(services))
            using (var scope = container.CreateScope())
            {
                var dispatcher = container.GetRequiredService<IUIDispatcher>();
                Assert.IsType<WPFUIDispatcher>(dispatcher);
                Assert.Same(dispatcher, container.GetRequiredService<IUIDispatcher>());
                Assert.Same(dispatcher, scope.ServiceProvider.GetRequiredService<IUIDispatcher>());
            }
        }

        [Fact]
        public void AddWPFUIDispatcher_RejectsNullRegistry()
        {
            Assert.Throws<ArgumentNullException>(() => UIDispatcherExtensions.AddWPFUIDispatcher(null));
        }

        [Fact]
        public void MissingDispatcher_OptionalReturnsNullAndRequiredThrows()
        {
            using (var container = new ServiceContainer(new ServiceCollection()))
            {
                Assert.Null(container.GetService<IUIDispatcher>());
                Assert.Throws<InvalidOperationException>(() => container.GetRequiredService<IUIDispatcher>());
            }
        }
    }
}
