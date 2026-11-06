using ConvMVVM3.Core;
using ConvMVVM3.Core.Mvvm.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.Host.DependencyInjection;
using Xunit;

namespace ConvMVVM3.Tests;

public class RegionManagerTests
{
    [Theory]
    [InlineData("generic")]
    [InlineData("type")]
    [InlineData("key")]
    public void Navigation_ReplacesContentAndPassesParametersToTargetAndCallback(string overload)
    {
        var services = new ServiceCollection();
        services.AddTransient<View>("view");
        using var container = new ServiceContainer(services);
        var manager = new RegionManager(container);
        manager.RegisterViewWithRegion<View>("Main");
        var previous = Assert.IsType<View>(manager.Regions["Main"].Content);
        var parameters = new NavigationParameters { ["id"] = 42 };
        NavigationContext? callback = null;
        void Capture(NavigationContext context) => callback = context;
        switch (overload)
        {
            case "generic": manager.RequestNavigate<View>("Main", parameters, Capture); break;
            case "type": manager.RequestNavigate("Main", typeof(View), parameters, Capture); break;
            default: manager.RequestNavigate("Main", "view", parameters, Capture); break;
        }
        var current = Assert.IsType<View>(manager.Regions["Main"].Content);
        Assert.NotSame(previous, current);
        Assert.Equal(1, previous.FromCount);
        Assert.Equal(1, current.ToCount);
        Assert.Same(parameters, current.Context!.Parameters);
        Assert.Same(parameters, previous.FromContext!.Parameters);
        Assert.Same(current.Context, callback);
        Assert.False(previous.IsPrimary);
        Assert.True(current.IsPrimary);
    }

    [Theory]
    [InlineData(RegionType.SingleView)]
    [InlineData(RegionType.MultiView)]
    public void ClearRegion_DestroysViewsAndResetsSelectionAndContext(RegionType type)
    {
        var services = new ServiceCollection();
        services.AddTransient<View>();
        using var container = new ServiceContainer(services);
        var manager = new RegionManager(container);
        manager.RegisterViewWithRegion<View>("Main", type);
        var region = manager.Regions["Main"];
        var view = Assert.IsType<View>(type == RegionType.SingleView ? region.Content : region.Views[0]);
        manager.Active("Main", (object)view);
        region.NavigationContext = new NavigationContext();
        manager.ClearRegion("Main");
        manager.ClearRegion("Main");
        Assert.Equal(1, view.DestroyCount);
        Assert.False(view.IsPrimary);
        Assert.Null(region.Content);
        Assert.Null(region.SelectedItem);
        Assert.Null(region.NavigationContext);
        Assert.Empty(region.Views);
    }

    [Fact]
    public void ActiveByIndex_DeactivatesPreviousSelection()
    {
        var services = new ServiceCollection();
        services.AddTransient<View>();
        using var container = new ServiceContainer(services);
        var manager = new RegionManager(container);
        manager.RegisterViewWithRegion<View>("Tabs", RegionType.MultiView);
        manager.RegisterViewWithRegion<View>("Tabs", RegionType.MultiView);
        var first = Assert.IsType<View>(manager.Regions["Tabs"].Views[0]);
        var second = Assert.IsType<View>(manager.Regions["Tabs"].Views[1]);
        manager.Active("Tabs", 0);
        Assert.True(first.IsPrimary);
        manager.Active("Tabs", 1);
        Assert.False(first.IsPrimary);
        Assert.True(second.IsPrimary);
        Assert.Same(second, manager.Regions["Tabs"].SelectedItem);
    }

    [Fact]
    public void Navigation_RejectsUnknownRegionAndIncompatibleRegionType()
    {
        using var container = new ServiceContainer(new ServiceCollection());
        var manager = new RegionManager(container);
        Assert.Throws<InvalidOperationException>(() => manager.RequestNavigate<View>("missing"));
        manager.RegisterViewWithRegion("Tabs", RegionType.MultiView);
        Assert.Throws<InvalidOperationException>(() => manager.RequestNavigate<View>("Tabs"));
    }

    [Fact]
    public void RegisteredResolver_NotifiesDistinctParticipantsInOrderWithSharedContext()
    {
        var calls = new List<string>();
        var old = new Participant("old", calls);
        var first = new Participant("first", calls);
        var second = new Participant("second", calls);
        var destination = new object();
        var origin = new object();
        var services = new ServiceCollection();
        services.AddSingleton<object>(_ => destination);
        services.AddSingleton<INavigationAwareResolver>(_ => new TestResolver(view =>
            ReferenceEquals(view, origin) ? new[] { old } : new[] { first, first, second }));
        services.AddRegionManager();
        using var container = new ServiceContainer(services);
        var manager = container.GetRequiredService<IRegionManager>();
        manager.RegisterViewWithRegion("Main", RegionType.SingleView);
        manager.Regions["Main"].Content = origin;
        var parameters = new NavigationParameters { ["id"] = 42 };
        manager.RequestNavigate<object>("Main", parameters, context =>
        {
            calls.Add("callback");
            Assert.Same(parameters, context.Parameters);
        });
        Assert.Equal(new[] { "old:from", "first:target", "first:to", "second:target", "second:to", "callback" }, calls);
        Assert.Same(first.Context, old.Context);
        Assert.Same(first.Context, second.Context);
    }

    [Fact]
    public void DestinationPreparationFailure_DoesNotLeaveOrReplaceCurrentView()
    {
        var calls = new List<string>();
        var previous = new Participant("old", calls);
        var services = new ServiceCollection();
        services.AddSingleton<object>(_ => new object());
        services.AddSingleton<INavigationAwareResolver>(_ => new TestResolver(view =>
            ReferenceEquals(view, previous) ? new[] { previous } : throw new InvalidOperationException("cannot wire")));
        using var container = new ServiceContainer(services);
        var manager = new RegionManager(container);
        manager.RegisterViewWithRegion("Main", RegionType.SingleView);
        manager.Regions["Main"].Content = previous;
        Assert.Throws<InvalidOperationException>(() => manager.RequestNavigate<object>("Main"));
        Assert.Same(previous, manager.Regions["Main"].Content);
        Assert.Empty(calls);
    }

    private sealed class TestResolver(Func<object, IEnumerable<INavigationAware>> resolve) : INavigationAwareResolver
    {
        public IEnumerable<INavigationAware> Resolve(object view) => resolve(view);
    }

    private sealed class Participant(string name, List<string> calls) : INavigationAware
    {
        public NavigationContext? Context { get; private set; }
        public bool IsNavigationTarget(NavigationContext context) { calls.Add(name + ":target"); return true; }
        public void OnNavigatedTo(NavigationContext context) { calls.Add(name + ":to"); Context = context; }
        public void OnNavigatedFrom(NavigationContext context) { calls.Add(name + ":from"); Context = context; }
    }

    public sealed class View : INavigationAware, IPrimaryAware, IDestructible
    {
        public bool IsPrimary { get; set; }
        public event EventHandler? IsPrimaryChanged { add { } remove { } }
        public int FromCount { get; private set; }
        public int ToCount { get; private set; }
        public int DestroyCount { get; private set; }
        public NavigationContext? Context { get; private set; }
        public NavigationContext? FromContext { get; private set; }
        public bool IsNavigationTarget(NavigationContext context) => true;
        public void OnNavigatedTo(NavigationContext context) { ToCount++; Context = context; }
        public void OnNavigatedFrom(NavigationContext context) { FromCount++; FromContext = context; }
        public void Destroy() => DestroyCount++;
    }
}
