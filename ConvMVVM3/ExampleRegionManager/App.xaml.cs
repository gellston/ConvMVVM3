using System.Windows;

namespace ExampleRegionManager;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        new AppBootStrapper().Run(this);
    }
}
