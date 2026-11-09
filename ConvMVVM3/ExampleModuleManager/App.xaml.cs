using System.Windows;

namespace ExampleModuleManager;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        new AppBootStrapper().Run(this);
    }
}
