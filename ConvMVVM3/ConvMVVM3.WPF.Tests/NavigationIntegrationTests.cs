using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.WPF.Locator;
using ConvMVVM3.WPF.Regions;
using Xunit;

namespace ConvMVVM3.WPF.Tests
{
    public partial class WpfIntegrationTests
    {
        [Theory]
        [InlineData("generic")]
        [InlineData("type")]
        [InlineData("key")]
        public Task Navigation_XamlWiredViewModelReceivesCallbacksBeforeLoaded(string overload)
        {
            return OnSta(() =>
            {
                var before = fixture.NavigationViewModelCreations;
                var manager = fixture.Container.GetRequiredService<IRegionManager>();
                var name = "Navigation_" + Guid.NewGuid().ToString("N");
                manager.RegisterViewWithRegion(name, RegionType.SingleView);
                var parameters = new NavigationParameters { ["id"] = 123 };
                NavigationContext callback = null;
                switch (overload)
                {
                    case "generic": manager.RequestNavigate<Views.NavigationTestView>(name, parameters, c => callback = c); break;
                    case "type": manager.RequestNavigate(name, typeof(Views.NavigationTestView), parameters, c => callback = c); break;
                    default: manager.RequestNavigate(name, "NavigationTestView", parameters, c => callback = c); break;
                }
                var view = Assert.IsType<Views.NavigationTestView>(manager.Regions[name].Content);
                var vm = Assert.IsType<NavigationTestViewModel>(view.DataContext);
                Assert.False(view.IsLoaded);
                Assert.Equal(before + 1, fixture.NavigationViewModelCreations);
                Assert.Same(parameters, Assert.Single(vm.Entered).Parameters);
                Assert.Same(callback, vm.Entered[0]);
                Assert.Equal(1, vm.TargetChecks);
                manager.RequestNavigate<Views.NavigationTestView>(name, parameters);
                Assert.Same(parameters, Assert.Single(vm.Left).Parameters);
                Assert.Equal(before + 2, fixture.NavigationViewModelCreations);
                manager.ClearRegion(name);
                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Locator_EnsureAndLoadedDoNotRecreateTransientViewModel()
        {
            return OnSta(() =>
            {
                var before = fixture.NavigationViewModelCreations;
                var view = new Views.NavigationTestView();
                var vm = view.DataContext;
                var resolver = new WPFNavigationAwareResolver();
                Assert.True(ViewModelLocator.EnsureViewModelConnected(view));
                Assert.True(ViewModelLocator.EnsureViewModelConnected(view));
                Assert.Same(vm, Assert.Single(resolver.Resolve(view)));
                view.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                Assert.Same(vm, Assert.Single(resolver.Resolve(view)));
                Assert.Equal(before + 1, fixture.NavigationViewModelCreations);
                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Locator_RetriesLateMappingSynchronouslyWithoutLoaded()
        {
            return OnSta(() =>
            {
                var before = fixture.NavigationViewModelCreations;
                var view = new LateMappedView();
                ViewModelLocator.SetAutoWireViewModel(view, true);
                Assert.Null(view.DataContext);
                ViewModelLocationProvider.Register(typeof(LateMappedView), typeof(NavigationTestViewModel));
                var aware = Assert.Single(new WPFNavigationAwareResolver().Resolve(view));
                Assert.Same(view.DataContext, aware);
                Assert.False(view.IsLoaded);
                Assert.Equal(before + 1, fixture.NavigationViewModelCreations);
                view.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                Assert.Same(aware, view.DataContext);
                Assert.Equal(before + 1, fixture.NavigationViewModelCreations);
                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Resolver_ReturnsViewThenViewModelAndDeduplicatesSelfDataContext()
        {
            return OnSta(() =>
            {
                var view = new AwareView();
                var vm = new NavigationTestViewModel();
                view.DataContext = vm;
                var resolver = new WPFNavigationAwareResolver();
                Assert.Equal(new INavigationAware[] { view, vm }, resolver.Resolve(view));
                view.DataContext = view;
                Assert.Same(view, Assert.Single(resolver.Resolve(view)));
                Assert.Empty(resolver.Resolve(new object()));
                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Navigation_NotifiesViewAndViewModelOnceAndHonorsEachTargetGate()
        {
            return OnSta(() =>
            {
                var vm = new NavigationTestViewModel { AcceptNavigation = false };
                var view = new AwareView { DataContext = vm };
                var services = new ConvMVVM3.Host.DependencyInjection.ServiceCollection();
                services.AddWPFRegionManager();
                services.AddSingleton<AwareView>(_ => view);
                using (var container = new ConvMVVM3.Host.DependencyInjection.ServiceContainer(services))
                {
                    var manager = container.GetRequiredService<IRegionManager>();
                    manager.RegisterViewWithRegion("Main", RegionType.SingleView);
                    manager.RequestNavigate<AwareView>("Main");
                    Assert.Equal(1, view.Entered);
                    Assert.Equal(1, vm.TargetChecks);
                    Assert.Empty(vm.Entered);
                    vm.AcceptNavigation = true;
                    manager.RequestNavigate<AwareView>("Main");
                    Assert.Equal(2, view.Entered);
                    Assert.Equal(1, view.Left);
                    Assert.Single(vm.Left);
                    Assert.NotNull(Assert.Single(vm.Entered).Parameters);
                }
                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Locator_PreservesExistingInheritedAndBoundDataContexts()
        {
            return OnSta(() =>
            {
                var before = fixture.NavigationViewModelCreations;
                var vm = new NavigationTestViewModel();
                var explicitView = new ContentControl { DataContext = vm };
                ViewModelLocator.SetViewModelType(explicitView, typeof(NavigationTestViewModel));
                ViewModelLocator.SetAutoWireViewModel(explicitView, true);
                Assert.True(ViewModelLocator.EnsureViewModelConnected(explicitView));
                Assert.Same(vm, explicitView.DataContext);

                var parent = new StackPanel { DataContext = vm };
                var inheritedView = new ContentControl();
                parent.Children.Add(inheritedView);
                ViewModelLocator.SetAutoWireViewModel(inheritedView, true);
                Assert.True(ViewModelLocator.EnsureViewModelConnected(inheritedView));
                Assert.Same(vm, inheritedView.DataContext);

                var boundView = new ContentControl();
                BindingOperations.SetBinding(boundView, FrameworkElement.DataContextProperty,
                    new Binding("Value") { Source = new ContextSource() });
                ViewModelLocator.SetViewModelType(boundView, typeof(NavigationTestViewModel));
                ViewModelLocator.SetAutoWireViewModel(boundView, true);
                Assert.False(ViewModelLocator.EnsureViewModelConnected(boundView));
                Assert.True(BindingOperations.IsDataBound(boundView, FrameworkElement.DataContextProperty));
                var nullView = new ContentControl { DataContext = null };
                ViewModelLocator.SetAutoWireViewModel(nullView, true);
                Assert.False(ViewModelLocator.EnsureViewModelConnected(nullView));
                Assert.Equal(before, fixture.NavigationViewModelCreations);
                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Resolver_ReportsUnresolvedOptInAndRespectsOptOut()
        {
            return OnSta(() =>
            {
                var before = fixture.NavigationViewModelCreations;
                var view = new ContentControl();
                ViewModelLocator.SetViewModelType(view, typeof(NavigationTestViewModel));
                Assert.False(ViewModelLocator.EnsureViewModelConnected(view));
                Assert.Empty(new WPFNavigationAwareResolver().Resolve(view));
                Assert.Equal(before, fixture.NavigationViewModelCreations);

                var missing = new MissingMappedView();
                ViewModelLocator.SetAutoWireViewModel(missing, true);
                Assert.Throws<InvalidOperationException>(() => new WPFNavigationAwareResolver().Resolve(missing));
                ViewModelLocator.SetAutoWireViewModel(missing, false);
                ViewModelLocator.SetViewModelType(missing, typeof(NavigationTestViewModel));
                missing.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                Assert.Null(missing.DataContext);
                return Task.CompletedTask;
            });
        }

        public sealed class LateMappedView : ContentControl { }
        public sealed class MissingMappedView : ContentControl { }
        public sealed class ContextSource { public object Value { get; set; } }
        public sealed class AwareView : ContentControl, INavigationAware
        {
            public int Entered { get; private set; }
            public int Left { get; private set; }
            public bool IsNavigationTarget(NavigationContext context) => true;
            public void OnNavigatedTo(NavigationContext context) => Entered++;
            public void OnNavigatedFrom(NavigationContext context) => Left++;
        }
    }

    public sealed class NavigationTestViewModel : INavigationAware
    {
        public List<NavigationContext> Entered { get; } = new List<NavigationContext>();
        public List<NavigationContext> Left { get; } = new List<NavigationContext>();
        public bool AcceptNavigation { get; set; } = true;
        public int TargetChecks { get; private set; }
        public bool IsNavigationTarget(NavigationContext context) { TargetChecks++; return AcceptNavigation; }
        public void OnNavigatedTo(NavigationContext context) => Entered.Add(context);
        public void OnNavigatedFrom(NavigationContext context) => Left.Add(context);
    }
}
