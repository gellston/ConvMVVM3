using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.WPF.Abstractions;

namespace ConvMVVM3.WPF.Regions
{
    /// <summary>
    /// Adapter for Selector-based controls (ListBox, TabControl, ComboBox, etc).
    /// Synchronizes SelectedItem <-> region.SelectedItem (ActiveView).
    /// </summary>
    public sealed class SelectorRegionAdapter : IRegionAdapter
    {
        public Type TargetType { get { return typeof(Selector); } }

        public IRegionAttachment Attach(DependencyObject target, IRegion region)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (region == null) throw new ArgumentNullException(nameof(region));

            var selector = (Selector)target;

            var oldDisp = selector.ItemsSource as IDisposable;
            if (oldDisp != null) oldDisp.Dispose();

            var source = new RegionItemsSource(region);
            selector.ItemsSource = source;

            bool syncing = false;

            SelectionChangedEventHandler uiChanged = delegate(object s, SelectionChangedEventArgs e)
            {
                if (syncing) return;
                syncing = true;
                try
                {
                    if (!object.Equals(region.SelectedItem, selector.SelectedItem))
                        region.SelectedItem = selector.SelectedItem;
                }
                finally
                {
                    syncing = false;
                }
            };
            selector.SelectionChanged += uiChanged;

            EventHandler<RegionActiveViewChangedEventArgs> regionChanged = delegate(object s, RegionActiveViewChangedEventArgs e)
            {
                if (syncing) return;
                syncing = true;
                try
                {
                    selector.SelectedItem = region.SelectedItem;
                }
                finally
                {
                    syncing = false;
                }

                if (e.Context != null)
                {
                    if (e.OldView != null)
                    {
                        NavigationAwareDispatcher.DispatchNavigatedFrom(e.OldView, e.Context);

                        var oldFe = e.OldView as FrameworkElement;
                        if (oldFe != null && oldFe.DataContext != null)
                            NavigationAwareDispatcher.DispatchNavigatedFrom(oldFe.DataContext, e.Context);
                    }

                    NavigationAwareDispatcher.DispatchToViewAndViewModel(e.NewView, e.Context).Dispose();
                }
            };

            WeakEventManager<IRegion, RegionActiveViewChangedEventArgs>.AddHandler(
                region, nameof(IRegion.ActiveViewChanged), regionChanged);

            selector.SelectedItem = region.SelectedItem;

            RoutedEventHandler unloaded = null;
            unloaded = delegate(object s, RoutedEventArgs e)
            {
                selector.Unloaded -= unloaded;
                selector.SelectionChanged -= uiChanged;

                WeakEventManager<IRegion, RegionActiveViewChangedEventArgs>.RemoveHandler(
                    region, nameof(IRegion.ActiveViewChanged), regionChanged);

                var disp = selector.ItemsSource as IDisposable;
                if (disp != null) disp.Dispose();
                selector.ItemsSource = null;
            };
            selector.Unloaded += unloaded;

            return new SimpleAttachment(delegate
            {
                selector.Unloaded -= unloaded;
                selector.SelectionChanged -= uiChanged;

                WeakEventManager<IRegion, RegionActiveViewChangedEventArgs>.RemoveHandler(
                    region, nameof(IRegion.ActiveViewChanged), regionChanged);

                var disp = selector.ItemsSource as IDisposable;
                if (disp != null) disp.Dispose();
                selector.ItemsSource = null;
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
