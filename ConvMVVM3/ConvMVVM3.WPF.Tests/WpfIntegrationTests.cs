using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.Host.DependencyInjection;
using ConvMVVM3.WPF.Locator;
using ConvMVVM3.WPF.Regions;
using ConvMVVM3.WPF.UIDispatcher;
using Xunit;

namespace ConvMVVM3.WPF.Tests
{
    // All tests using the process-wide ServiceLocator share one fixture and class.
    public class LocatorFixture : IDisposable
    {
        public ServiceContainer Container { get; }
        public int NavigationViewModelCreations { get; private set; }
        public LocatorFixture()
        {
            var services = new ServiceCollection();
            services.AddSingleton<TestViewModel>();
            services.AddWPFRegionManager();
            services.AddTransient<NavigationTestViewModel>(_ =>
            {
                NavigationViewModelCreations++;
                return new NavigationTestViewModel();
            });
            services.AddTransient<Views.NavigationTestView>("NavigationTestView");
            Container = new ServiceContainer(services);
            ServiceLocator.Initialize(Container);
        }
        public void Dispose() => Container.Dispose();
    }

    public partial class WpfIntegrationTests : IClassFixture<LocatorFixture>
    {
        private readonly LocatorFixture fixture;
        public WpfIntegrationTests(LocatorFixture fixture) { this.fixture = fixture; }

        [Fact]
        public Task Locator_ManualTypeResolvesRegisteredViewModel()
        {
            return OnSta(async () =>
            {
                var view = new ContentControl();
                ViewModelLocator.SetMode(view, ViewModelLocatorMode.Manual);
                ViewModelLocator.SetViewModelType(view, typeof(TestViewModel));
                ViewModelLocator.SetAutoWireViewModel(view, true);
                Assert.Same(fixture.Container.GetRequiredService<TestViewModel>(), view.DataContext);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public Task Locator_FactoryReceivesContainerAndSetsDataContext()
        {
            return OnSta(async () =>
            {
                var expected = new TestViewModel();
                ViewModelLocationProvider.RegisterFactory(typeof(FactoryView), resolver =>
                {
                    Assert.Same(fixture.Container, resolver);
                    return expected;
                });
                var view = new FactoryView();
                ViewModelLocator.SetAutoWireViewModel(view, true);
                Assert.Same(expected, view.DataContext);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public void Locator_AttributeTakesPrecedenceOverExplicitMapping()
        {
            ViewModelLocationProvider.Register(typeof(AttributedView), typeof(object));
            Assert.Equal(typeof(TestViewModel), ViewModelLocationProvider.ResolveViewModelType(typeof(AttributedView)));
        }

        [Fact]
        public Task Region_ContentControlBindsOnLoadAndClearsOnUnload()
        {
            return OnSta(async () =>
            {
                var name = "Main_" + Guid.NewGuid().ToString("N");
                var host = new ContentControl();
                RegionPlugin.SetRegionName(host, name);
                var manager = fixture.Container.GetRequiredService<IRegionManager>();
                var region = manager.Regions[name];
                var content = new object();
                region.Content = content;
                host.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                Assert.True(region.IsAttaced);
                Assert.True(BindingOperations.IsDataBound(host, ContentControl.ContentProperty));
                Assert.Same(content, host.Content);
                var replacement = new object();
                region.Content = replacement;
                Assert.Same(replacement, host.Content);
                host.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
                Assert.False(region.IsAttaced);
                Assert.False(BindingOperations.IsDataBound(host, ContentControl.ContentProperty));
                Assert.Null(region.Content);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public Task Dispatcher_MarshalsBackgroundCallsToOwnerThreadAndPropagatesErrors()
        {
            return OnSta(async () =>
            {
                var dispatcher = new WPFUIDispatcher(Dispatcher.CurrentDispatcher);
                var owner = Thread.CurrentThread.ManagedThreadId;
                Assert.True(dispatcher.CheckAccess());
                await Task.Run(async () =>
                {
                    Assert.False(dispatcher.CheckAccess());
                    Assert.Throws<InvalidOperationException>(() => dispatcher.VerifyAccess());
                    var actual = await dispatcher.InvokeAsync<int>(() => Thread.CurrentThread.ManagedThreadId);
                    Assert.Equal(owner, actual);
                    var error = new InvalidOperationException("callback failed");
                    Assert.Same(error, await Assert.ThrowsAsync<InvalidOperationException>(() =>
                        dispatcher.InvokeAsync((Action)(() => { throw error; }))));
                });
            });
        }

        private static async Task OnSta(Func<Task> action)
        {
            var completion = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                var dispatcher = Dispatcher.CurrentDispatcher;
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));
                dispatcher.BeginInvoke(new Action(async () =>
                {
                    try { await action(); completion.TrySetResult(null); }
                    catch (Exception error) { completion.TrySetException(error); }
                    finally { dispatcher.BeginInvokeShutdown(DispatcherPriority.Send); }
                }));
                Dispatcher.Run();
            }) { IsBackground = true };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.Same(completion.Task, await Task.WhenAny(completion.Task, Task.Delay(TimeSpan.FromSeconds(10))));
            await completion.Task;
        }

        public sealed class FactoryView : ContentControl { }
        [ViewModelFor(typeof(TestViewModel))]
        public sealed class AttributedView : ContentControl { }
    }

    public sealed class TestViewModel { }
}
