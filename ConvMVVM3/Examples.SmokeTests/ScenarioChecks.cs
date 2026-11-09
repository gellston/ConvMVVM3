using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Messaging.Abstractions;
using BasicsVM = ExampleMvvmBasics.ViewModels.MainWindowViewModel;
using AsyncVM = ExampleAsyncOperations.ViewModels.MainWindowViewModel;
using MessagingVM = ExampleMessaging.ViewModels.MainWindowViewModel;
using DispatcherVM = ExampleUIDispatcher.ViewModels.MainWindowViewModel;
using BehaviorsVM = ExampleBehaviors.ViewModels.MainWindowViewModel;
using CollectionsVM = ExampleCollections.ViewModels.MainWindowViewModel;
using ResourcesVM = ExampleResourceLifecycle.ViewModels.MainWindowViewModel;

internal static partial class Program
{
    private static async Task ValidateAsync(string sample, IServiceContainer services, Window shell)
    {
        switch (sample)
        {
            case "ExampleMvvmBasics":
            {
                var vm = services.GetRequiredService<BasicsVM>();
                Check(ReferenceEquals(vm, shell.DataContext), "Basic ViewModel was not wired.");
                var changed = new List<string?>();
                vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
                vm.FirstName = "Taylor";
                Check(vm.FullName == "Taylor Morgan" && vm.Greeting == "Hello, Taylor Morgan!", "Dependent values did not update.");
                Check(changed.Contains("FullName") && changed.Contains("Greeting"), "Dependent notifications were not raised.");
                vm.SaveCommand.Execute(null);
                Check(vm.SaveCount == 1, "Save command failed.");
                vm.FirstName = "";
                Check(!vm.SaveCommand.CanExecute(null), "An empty name should disable saving.");
                vm.SaveCommand.Execute(null);
                Check(vm.SaveCount == 1, "A disabled command executed.");
                vm.FirstName = "Taylor"; vm.AllowSave = false;
                Check(!vm.SaveCommand.CanExecute(null), "AllowSave was ignored.");
                vm.IncrementCommand.Execute(null);
                Check(vm.ClickCount == 1, "Generated command failed.");
                vm.ResetCommand.Execute(null);
                Check(vm.FirstName == "Alex" && vm.SaveCount == 0 && vm.ClickCount == 0, "Reset failed.");
                break;
            }
            case "ExampleAsyncOperations":
            {
                var vm = services.GetRequiredService<AsyncVM>();
                var service = services.GetRequiredService<ExampleAsyncOperations.Services.DemoJobService>();
                Check(ReferenceEquals(vm, shell.DataContext), "Async ViewModel was not wired.");
                service.StepDelay = TimeSpan.FromMilliseconds(1);
                await vm.RunCommand.ExecuteAsync(null);
                Check(vm.CompletedRuns == 1 && vm.Progress == 100 && !vm.IsBusy, "Successful async run failed.");
                vm.SimulateFailure = true;
                await vm.RunCommand.ExecuteAsync(null);
                Check(vm.FailedRuns == 1 && !vm.IsBusy && vm.Status.StartsWith("Failed:"), "Failure was not handled.");
                vm.SimulateFailure = false;
                service.StepDelay = TimeSpan.FromMilliseconds(30);
                var running = vm.RunCommand.ExecuteAsync(null);
                Check(!vm.RunCommand.CanExecute(null) && vm.CancelCommand.CanExecute(null), "Running command states are incorrect.");
                await WaitForAsync(vm, () => vm.Progress > 0);
                vm.CancelCommand.Execute(null);
                await running;
                Check(vm.CancelledRuns == 1 && vm.Progress < 100 && !vm.IsBusy, "Cancellation failed.");
                Check(!vm.CancelCommand.CanExecute(null), "Cancel remained enabled after completion.");
                service.StepDelay = TimeSpan.FromMilliseconds(1);
                await vm.RunCommand.ExecuteAsync(null);
                Check(vm.CompletedRuns == 2 && vm.Progress == 100, "Retry after cancellation failed.");
                break;
            }
            case "ExampleMessaging":
            {
                var vm = services.GetRequiredService<MessagingVM>();
                Check(ReferenceEquals(vm, shell.DataContext), "Messaging ViewModel was not wired.");
                vm.SendAlphaCommand.Execute(null);
                Check(vm.Left.ReceivedCount == 1 && vm.Right.ReceivedCount == 0, "Alpha channel leaked.");
                vm.SendBetaCommand.Execute(null);
                Check(vm.Right.ReceivedCount == 1, "Beta delivery failed.");
                vm.Left.IsActive = false;
                vm.SendAlphaCommand.Execute(null);
                Check(vm.Left.ReceivedCount == 1, "Inactive receiver received a message.");
                vm.Left.IsActive = true;
                vm.MoveRightToAlphaCommand.Execute(null);
                vm.SendAlphaCommand.Execute(null);
                Check(vm.Left.ReceivedCount == 2 && vm.Right.ReceivedCount == 2, "Channel reassignment failed.");
                vm.SendBetaCommand.Execute(null);
                Check(vm.Right.ReceivedCount == 2, "Old channel subscription remained.");
                var messenger = services.GetRequiredService<IMessenger>();
                var temporary = new ExampleMessaging.ViewModels.ReceiverViewModel(messenger, "Temporary", "Alpha");
                temporary.Dispose(); temporary.Dispose();
                vm.SendAlphaCommand.Execute(null);
                Check(temporary.ReceivedCount == 0, "Disposed recipient still received messages.");
                break;
            }
            case "ExampleUIDispatcher":
            {
                var vm = services.GetRequiredService<DispatcherVM>();
                Check(ReferenceEquals(vm, shell.DataContext), "Dispatcher ViewModel was not wired.");
                var wrongThread = false;
                vm.Updates.CollectionChanged += (_, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                        wrongThread |= Environment.CurrentManagedThreadId != vm.UIThreadId;
                };
                await vm.StartCommand.ExecuteAsync(null);
                Check(vm.Progress == 100 && vm.Updates.Count == 10, "Background work did not complete.");
                Check(vm.WorkerThreadId != vm.UIThreadId && vm.UpdateThreadId == vm.UIThreadId && !wrongThread,
                    "Work or collection updates ran on the wrong thread.");
                var pending = vm.StartCommand.ExecuteAsync(null);
                await WaitForAsync(vm, () => vm.Progress > 0);
                vm.CancelCommand.Execute(null);
                await pending;
                Check(!vm.IsBusy && vm.Progress < 100 && vm.Status.StartsWith("Cancelled"), "Background cancellation failed.");
                break;
            }
            case "ExampleBehaviors":
            {
                var vm = services.GetRequiredService<BehaviorsVM>();
                Check(ReferenceEquals(vm, shell.DataContext), "Behaviors ViewModel was not wired.");
                var eventButton = (Button)shell.FindName("EventButton");
                eventButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Check(vm.EventCount == 1 && vm.Events[0].Contains(vm.Payload), "Event-to-command parameter binding failed.");
                vm.AllowEvents = false;
                eventButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Check(vm.EventCount == 1, "InvokeCommandAction ignored CanExecute.");
                vm.AllowEvents = true; vm.Payload = "Updated payload";
                eventButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Check(vm.EventCount == 2 && vm.Events[0].Contains("Updated payload"), "Updated event payload was not passed.");
                ((Button)shell.FindName("MethodButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Check(vm.MethodCount == 1, "CallMethodAction failed.");
                var propertyButton = (Button)shell.FindName("PropertyButton");
                propertyButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Check((string)propertyButton.Content == "Changed by ChangePropertyAction", "ChangePropertyAction failed.");
                ConvMVVM3.WPF.Behaviors.Interaction.GetTriggers(eventButton).Clear();
                eventButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Check(vm.EventCount == 2, "Detached trigger still received events.");
                break;
            }
            case "ExampleCollections":
            {
                var vm = services.GetRequiredService<CollectionsVM>();
                Check(ReferenceEquals(vm, shell.DataContext), "Collections ViewModel was not wired.");
                vm.LoadIndividuallyCommand.Execute(null);
                Check(vm.IndividualItems.Count == 1000 && vm.IndividualNotifications == 1000, "Individual-add baseline is incorrect.");
                vm.LoadRangeCommand.Execute(null);
                Check(vm.Items.Count == 1000 && vm.RangeNotifications == 1 && vm.ResetNotifications == 1, "AddRange notifications are incorrect.");
                vm.ReplaceRangeCommand.Execute(null);
                Check(vm.Items.Count == 200 && vm.RangeNotifications == 1, "ReplaceRange failed.");
                vm.RemoveRangeCommand.Execute(null);
                Check(vm.Items.Count == 100 && vm.RangeNotifications == 1 && vm.Items[0] == "Replacement 0101", "RemoveRange failed.");
                vm.ClearRangeCommand.Execute(null);
                Check(vm.Items.Count == 0 && vm.RangeNotifications == 1, "Clear failed.");
                vm.ClearRangeCommand.Execute(null);
                Check(vm.RangeNotifications == 0, "Empty clear notified unexpectedly.");
                vm.LoadRangeCommand.Execute(null);
                break;
            }
            case "ExampleResourceLifecycle":
            {
                var vm = services.GetRequiredService<ResourcesVM>();
                Check(ReferenceEquals(vm, shell.DataContext), "Resource ViewModel was not wired.");
                vm.AddGroupedCommand.Execute(null); vm.AddGroupedCommand.Execute(null); vm.ReplaceSerialCommand.Execute(null);
                Check(vm.ActiveSubscriptions == 3, "Subscriptions were not created.");
                vm.SendSignalCommand.Execute(null);
                Check(vm.ReceivedSignals == 3, "Signal delivery failed.");
                vm.ReplaceSerialCommand.Execute(null);
                Check(vm.ActiveSubscriptions == 3 && vm.DisposedResources == 1, "Serial replacement leaked a subscription.");
                vm.ClearGroupedCommand.Execute(null);
                Check(vm.ActiveSubscriptions == 1 && vm.DisposedResources == 3 && !vm.GroupDisposed, "Composite clear failed.");
                vm.AddGroupedCommand.Execute(null); vm.DisposeGroupedCommand.Execute(null); vm.DisposeGroupedCommand.Execute(null);
                Check(vm.ActiveSubscriptions == 1 && vm.DisposedResources == 4 && vm.GroupDisposed, "Composite disposal is not idempotent.");
                vm.AddGroupedCommand.Execute(null);
                Check(vm.ActiveSubscriptions == 1 && vm.DisposedResources == 5, "A disposed group retained a new subscription.");
                vm.ResetResourcesCommand.Execute(null);
                Check(vm.ActiveSubscriptions == 0 && !vm.GroupDisposed, "Resource reset failed.");
                var first = vm.GuardedWorkCommand.ExecuteAsync(null);
                var second = vm.GuardedWorkCommand.ExecuteAsync(null);
                Check(vm.IsBusy && vm.BusyCount == 2, "Nested busy scopes were not tracked.");
                await Task.WhenAll(first, second);
                Check(vm.AcceptedWork == 1 && vm.RejectedWork == 1 && vm.CompletedWork == 1 && vm.BusyCount == 0 && !vm.IsBusy,
                    "OperationGuard did not reject reentry or reset state.");
                vm.SimulateFailure = true;
                await vm.GuardedWorkCommand.ExecuteAsync(null);
                Check(vm.Status.StartsWith("Failed:") && vm.BusyCount == 0 && !vm.IsBusy, "Failure left the guard or busy state active.");
                vm.SimulateFailure = false;
                await vm.GuardedWorkCommand.ExecuteAsync(null);
                Check(vm.CompletedWork == 2, "Guard did not recover after failure.");
                vm.AddGroupedCommand.Execute(null); vm.ReplaceSerialCommand.Execute(null);
                vm.Dispose(); vm.Dispose();
                Check(vm.ActiveSubscriptions == 0, "Window cleanup leaked subscriptions.");
                break;
            }
        }
    }
}
