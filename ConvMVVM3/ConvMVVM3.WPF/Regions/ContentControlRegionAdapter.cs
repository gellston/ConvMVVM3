using System;
using System.Windows;
using System.Windows.Controls;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.WPF.Abstractions;

namespace ConvMVVM3.WPF.Regions
{
    public sealed class ContentControlRegionAdapter : IRegionAdapter
    {
        public Type TargetType { get { return typeof(ContentControl); } }

        public IRegionAttachment Attach(DependencyObject target, IRegion region)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (region == null) throw new ArgumentNullException(nameof(region));

            var contentControl = (ContentControl)target;

            Action applyActive = delegate
            {
                contentControl.Content = region.ActiveView;
            };

            applyActive();

            EventHandler<RegionActiveViewChangedEventArgs> handler = delegate(object s, RegionActiveViewChangedEventArgs e)
            {
                applyActive();

                if (e.Context != null)
                {
                    if (e.OldView != null)
                    {
                        NavigationAwareDispatcher.DispatchNavigatedFrom(e.OldView, e.Context);

                        var oldFe = e.OldView as FrameworkElement;
                        if (oldFe != null && oldFe.DataContext != null)
                            NavigationAwareDispatcher.DispatchNavigatedFrom(oldFe.DataContext, e.Context);
                    }

                    // new (handles late DataContext via weak subscription)
                    NavigationAwareDispatcher.DispatchToViewAndViewModel(e.NewView, e.Context).Dispose();
                }
            };

            WeakEventManager<IRegion, RegionActiveViewChangedEventArgs>.AddHandler(
                region, nameof(IRegion.ActiveViewChanged), handler);

            // If there are views but none active, activate first (no context)
            if (region.Views.Count > 0 && region.ActiveView == null)
                region.Activate(region.Views[0], null);

            return new SimpleAttachment(delegate
            {
                WeakEventManager<IRegion, RegionActiveViewChangedEventArgs>.RemoveHandler(
                    region, nameof(IRegion.ActiveViewChanged), handler);

                contentControl.ClearValue(ContentControl.ContentProperty);
            });
        }

        private sealed class SimpleAttachment : IRegionAttachment
        {
            private Action _dispose;

            public SimpleAttachment(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                var d = _dispose;
                _dispose = null;
                if (d != null) d();
            }
        }
    }
}
