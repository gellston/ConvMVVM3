using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Host.DependencyInjection;
using ConvMVVM3.WPF;

internal static partial class Program
{
    private static int _checks;
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: Examples.SmokeTests <sample> <repository-root> [preview-directory]"); return 2; }
        var dispatcher = Dispatcher.CurrentDispatcher;
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));
        var exitCode = 1;
        dispatcher.BeginInvoke(new Action(async () =>
        {
            Application? app = null;
            try
            {
                // A plain Application avoids invoking sample startup twice during dispatcher pumping.
                app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                var appXml = XDocument.Load(Path.Combine(args[1], "ConvMVVM3", args[0], "App.xaml"));
                var resources = appXml.Root!.Elements().Single();
                var dictionaryXml = "<ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>"
                    + string.Join("", resources.Elements().Select(element => element.ToString())) + "</ResourceDictionary>";
                app.Resources = (ResourceDictionary)System.Windows.Markup.XamlReader.Parse(dictionaryXml);
                using var bindingLog = new StringWriter();
                using var listener = new TextWriterTraceListener(bindingLog);
                PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning;
                PresentationTraceSources.DataBindingSource.Listeners.Add(listener);
                var bootstrapper = CreateBootstrapper(args[0]);
                bootstrapper.Run(app);
                var shell = app.MainWindow;
                CheckWiring(shell);
                await ValidateAsync(args[0], ServiceLocator.Container, shell).WaitAsync(TimeSpan.FromSeconds(20));
                CheckWiring(shell);
                await dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
                if (args.Length >= 3) SavePreview(shell, Path.Combine(args[2], args[0] + ".png"));
                listener.Flush();
                Check(string.IsNullOrWhiteSpace(bindingLog.ToString()), "WPF binding diagnostics: " + bindingLog);
                if (shell.DataContext is IDisposable lifetime) lifetime.Dispose();
                Console.WriteLine($"PASS {args[0]}: {_checks} checks; no binding diagnostics.");
                exitCode = 0;
            }
            catch (Exception error) { Console.Error.WriteLine("FAIL " + args[0] + ": " + error); }
            finally { app?.Shutdown(); dispatcher.BeginInvokeShutdown(DispatcherPriority.Send); }
        }));
        Dispatcher.Run();
        return exitCode;
    }

    private static Bootstrapper CreateBootstrapper(string sample) => sample switch
    {
        "ExampleActivatorUtilities" => new ActivatorBoot(),
        "ExampleModuleManager" => new ModuleBoot(),
        "ExampleRegionManager" => new RegionBoot(),
                "ExampleMvvmBasics" => new ExampleMvvmBasicsBoot(),
                "ExampleAsyncOperations" => new ExampleAsyncOperationsBoot(),
                "ExampleMessaging" => new ExampleMessagingBoot(),
                "ExampleUIDispatcher" => new ExampleUIDispatcherBoot(),
                "ExampleBehaviors" => new ExampleBehaviorsBoot(),
                "ExampleCollections" => new ExampleCollectionsBoot(),
                "ExampleResourceLifecycle" => new ExampleResourceLifecycleBoot(),
        _ => throw new ArgumentException("Unknown sample: " + sample)
    };

    internal static void Check(bool condition, string message)
    {
        _checks++;
        if (!condition) throw new InvalidOperationException(message);
    }
    internal static void LoadTree(DependencyObject root)
    {
        if (root is FrameworkElement element) element.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>().ToArray()) LoadTree(child);
    }
    internal static void CheckWiring(Window shell)
    {
        LoadTree(shell);
        var content = (FrameworkElement)shell.Content;
        content.Measure(new Size(shell.Width - 32, shell.Height - 60));
        content.Arrange(new Rect(0, 0, shell.Width - 32, shell.Height - 60));
        content.UpdateLayout();
    }
    private static async Task WaitForAsync(INotifyPropertyChanged source, Func<bool> condition)
    {
        if (condition()) return;
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        PropertyChangedEventHandler handler = (_, _) => { if (condition()) completion.TrySetResult(); };
        source.PropertyChanged += handler;
        try { if (condition()) completion.TrySetResult(); await completion.Task.WaitAsync(TimeSpan.FromSeconds(5)); }
        finally { source.PropertyChanged -= handler; }
    }
    private static void SavePreview(Window shell, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var content = (FrameworkElement)shell.Content;
        var width = Math.Max(1, (int)content.ActualWidth);
        var height = Math.Max(1, (int)content.ActualHeight);
        var drawing = new DrawingVisual();
        using (var context = drawing.RenderOpen())
        {
            context.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
            context.DrawRectangle(new VisualBrush(content), null, new Rect(0, 0, width, height));
        }
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(drawing);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path); encoder.Save(stream);
    }
}
