using System;
using System.Threading;
using System.Threading.Tasks;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Abstractions;
using ConvMVVM3.Host.DependencyInjection;
using ConvMVVM3.WPF;
using ConvMVVM3.WPF.Tests;
using Xunit;

namespace ConvMVVM3.WPF.Tests
{
    /// <summary>
    /// Integration tests for UIDispatcher functionality.
    /// </summary>
    public class UIDispatcherIntegrationTests
    {
        [Fact]
        public void UIDispatcher_Integration_With_DI()
        {
            // Arrange
            StaThreadHelper.Run(() =>
            {
                var services = new ServiceCollection();
                services.AddWPFUIDispatcher();
                var serviceProvider = new ServiceContainer(services);
                var dispatcher = serviceProvider.GetService<IUIDispatcher>() as WPFUIDispatcher;
                var results = new bool[2];

                // Act
                results[0] = dispatcher.CheckAccess();  // UI thread
                
                var thread = new Thread(() =>
                {
                    results[1] = dispatcher.CheckAccess(); // Background thread
                });
                thread.SetApartmentState(ApartmentState.MTA);
                thread.Start();
                thread.Join();

                // Assert
                Assert.True(results[0]);  // UI thread
                Assert.False(results[1]); // Background thread
            });
        }

        [Fact]
        public async Task UIDispatcher_Integration_Async_Scenario()
        {
            // Arrange
            StaThreadHelper.Run(async () =>
            {
                var dispatcher = new WPFUIDispatcher(System.Windows.Threading.Dispatcher.CurrentDispatcher);
                var tasks = new Task[3];
                var results = new int[3];

                // Act
                for (int i = 0; i < 3; i++)
                {
                    int index = i;
                    tasks[index] = Task.Run(async () =>
                    {
                        results[index] = await dispatcher.InvokeAsync(() => index * 10);
                    });
                }

                await Task.WhenAll(tasks);

                // Assert
                Assert.Equal(0, results[0]);
                Assert.Equal(10, results[1]);
                Assert.Equal(20, results[2]);
            });
        }

        [Fact]
        public void UIDispatcher_Integration_Priority_Dispatching()
        {
            // Arrange
            StaThreadHelper.Run(async () =>
            {
                var services = new ServiceCollection();
                services.AddWPFUIDispatcher();
                var serviceProvider = new ServiceContainer(services);
                var dispatcher = serviceProvider.GetService<IUIDispatcher>() as WPFUIDispatcher;
                var executionOrder = new System.Collections.Generic.List<int>();

                // Act
                await dispatcher.InvokeAsync(() => executionOrder.Add(1), System.Windows.Threading.DispatcherPriority.Background);
                await dispatcher.InvokeAsync(() => executionOrder.Add(2), System.Windows.Threading.DispatcherPriority.Normal);
                await dispatcher.InvokeAsync(() => executionOrder.Add(3), System.Windows.Threading.DispatcherPriority.Send);

                // Assert
                Assert.Equal(3, executionOrder.Count);
                Assert.Contains(3, executionOrder); // High priority should execute first
            });
        }
    }
}