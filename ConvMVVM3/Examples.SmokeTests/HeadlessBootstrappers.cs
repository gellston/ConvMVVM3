using System.Windows;

internal sealed class ExampleMvvmBasicsBoot : ExampleMvvmBasics.AppBootStrapper
{
    protected override void InitializeShell(Application app, Window shell) => app.MainWindow = shell;
}

internal sealed class ExampleAsyncOperationsBoot : ExampleAsyncOperations.AppBootStrapper
{
    protected override void InitializeShell(Application app, Window shell) => app.MainWindow = shell;
}

internal sealed class ExampleMessagingBoot : ExampleMessaging.AppBootStrapper
{
    protected override void InitializeShell(Application app, Window shell) => app.MainWindow = shell;
}

internal sealed class ExampleUIDispatcherBoot : ExampleUIDispatcher.AppBootStrapper
{
    protected override void InitializeShell(Application app, Window shell) => app.MainWindow = shell;
}

internal sealed class ExampleBehaviorsBoot : ExampleBehaviors.AppBootStrapper
{
    protected override void InitializeShell(Application app, Window shell) => app.MainWindow = shell;
}

internal sealed class ExampleCollectionsBoot : ExampleCollections.AppBootStrapper
{
    protected override void InitializeShell(Application app, Window shell) => app.MainWindow = shell;
}

internal sealed class ExampleResourceLifecycleBoot : ExampleResourceLifecycle.AppBootStrapper
{
    protected override void InitializeShell(Application app, Window shell) => app.MainWindow = shell;
}
