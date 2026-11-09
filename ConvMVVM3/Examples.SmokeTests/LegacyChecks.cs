using System.Windows;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Modules.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;

internal sealed class ActivatorBoot : ExampleActivatorUtilities.AppBootStrapper
{
    protected override void InitializeShell(Application app, Window shell) => app.MainWindow = shell;
    protected override void OnInitialized(IServiceContainer provider)
    {
        base.OnInitialized(provider);
        var vm = provider.GetRequiredService<ExampleActivatorUtilities.ViewModels.MainWindowViewModel>();
        var view = provider.GetRequiredService<ExampleActivatorUtilities.Views.MainWindowView>();
        Program.Check(ReferenceEquals(view.DataContext, vm), "Activator ViewModel wiring failed");
        vm.Age = "invalid";
        vm.GenerateModelCommand.Execute(null);
        Program.Check(vm.TestModelCollection.Count == 0 && vm.Status.Contains("whole-number"), "Input validation failed");
        vm.Name = "Alice";
        vm.Age = "31";
        vm.GenerateModelCommand.Execute(null);
        vm.GenerateWithFactoryCommand.Execute(null);
        Program.Check(vm.TestModelCollection.Count == 2, vm.Status);
        Program.Check(vm.TestModelCollection.All(item => item.Greeting == "Hello, Alice!" && item.Age == 31), "DI service or runtime arguments failed");
        Program.Check(vm.TestModelCollection[0].InstanceId != vm.TestModelCollection[1].InstanceId, "Models were reused");
        var notified = false;
        vm.TestModelCollection[0].PropertyChanged += (_, e) => notified |= e.PropertyName == "Greeting";
        vm.TestModelCollection[0].Name = "Bob";
        Program.Check(notified && vm.TestModelCollection[0].Greeting == "Hello, Bob!", "Dependent notification failed");
        Program.CheckWiring(view);
        vm.ClearModelsCommand.Execute(null);
        Program.Check(vm.TestModelCollection.Count == 0, "Clear failed");
        vm.GenerateModelCommand.Execute(null);
        vm.GenerateWithFactoryCommand.Execute(null);
    }
}

internal sealed class ModuleBoot : ExampleModuleManager.AppBootStrapper
{
    protected override void InitializeShell(Application app, Window shell) => app.MainWindow = shell;
    protected override void OnInitialized(IServiceContainer provider)
    {
        base.OnInitialized(provider);
        var modules = provider.GetRequiredService<IModuleManager>();
        var regions = provider.GetRequiredService<IRegionManager>();
        var vm = provider.GetRequiredService<ExampleModuleManager.ViewModels.MainContentViewModel>();
        var a = provider.GetRequiredService<ExampleModuleManager.ViewModels.AModuleViewModel>();
        var c = provider.GetRequiredService<ExampleModuleManager.ViewModels.CModuleViewModel>();
        Program.Check(a.NavigationCount == 1, "Initial A navigation failed");
        Program.Check(!modules.Categories.Single(item => item.Name == "CModule").IsInitialized, "C initialized too early");
        vm.CModuleLoadCommand.Execute(null);
        Program.Check(modules.Categories.Single(item => item.Name == "CModule").IsInitialized && c.NavigationCount == 1, vm.Status);
        vm.AModuleCommand.Execute(null);
        vm.CModuleLoadCommand.Execute(null);
        Program.Check(c.NavigationCount == 2 && a.NavigationCount == 2, "C cannot reopen");
        Program.Check(ReferenceEquals(((FrameworkElement)regions.Regions["SubContent"].Content).DataContext, c), "Module DataContext mismatch");
        Program.Check(vm.ModuleStatus.Contains("CModule: initialized"), "Module status did not refresh");
        Program.CheckWiring(provider.GetRequiredService<ExampleModuleManager.Views.MainWindowView>());
    }
}

internal sealed class RegionBoot : ExampleRegionManager.AppBootStrapper
{
    protected override void InitializeShell(Application app, Window shell) => app.MainWindow = shell;
    protected override void OnInitialized(IServiceContainer provider)
    {
        base.OnInitialized(provider);
        var regions = provider.GetRequiredService<IRegionManager>();
        var vm = provider.GetRequiredService<ExampleRegionManager.ViewModels.MainWindowViewModel>();
        var detail = provider.GetRequiredService<ExampleRegionManager.ViewModels.MainViewModel>();
        var first = regions.Regions["ContentView"].Content;
        Program.Check(detail.Customer.Id == 101 && detail.NavigationCount == 1, "Initial navigation failed");
        vm.SwitchContentCommand.Execute(null);
        Program.Check(detail.Customer.Id == 102 && detail.NavigationCount == 2, "Parameter callbacks failed");
        Program.Check(ReferenceEquals(first, regions.Regions["ContentView"].Content), "Singleton was recreated");
        Program.Check(vm.NavigationHistory.Count == 3, "Navigation history is incomplete");
        foreach (var name in new[] { "SubView1", "SubView2" })
        {
            var views = regions.Regions[name].Views.Cast<FrameworkElement>().ToArray();
            Program.Check(views.Length == 3 && views.Select(view => view.DataContext).Distinct().Count() == 3, "Transient wiring failed");
        }
        var shell = provider.GetRequiredService<ExampleRegionManager.Views.MainWindowView>();
        Program.CheckWiring(shell);
        vm.SelectListCommand.Execute(null);
        Program.Check(ReferenceEquals(regions.Regions["SubView2"].SelectedItem, regions.Regions["SubView2"].Views[1]), "Selection index is incorrect");
        vm.ClearItemsCommand.Execute(null);
        vm.ClearListCommand.Execute(null);
        Program.Check(regions.Regions["SubView1"].Views.Count == 0 && regions.Regions["SubView2"].Views.Count == 0, "Clear failed");
        vm.SelectListCommand.Execute(null);
        Program.Check(vm.Status.StartsWith("Operation failed:"), "Selection error is hidden");
        vm.AddListCommand.Execute(null);
        Program.Check(regions.Regions["SubView2"].Views.Count == 3, "Cannot add after clearing");
    }
}

