using System.Windows;

namespace ExampleMessaging;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        new AppBootStrapper().Run(this);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (MainWindow?.DataContext is IDisposable lifetime) lifetime.Dispose();
        base.OnExit(e);
    }
}
