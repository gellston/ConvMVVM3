using System;
using System.Windows;
using System.Windows.Controls;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.WPF.Abstractions;

namespace ConvMVVM3.WPF.Regions
{
    public sealed class PanelRegionAdapter : IRegionAdapter
    {
        public Type TargetType { get { return typeof(Panel); } }

        public IRegionAttachment Attach(DependencyObject target, IRegion region)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (region == null) throw new ArgumentNullException(nameof(region));

            var panel = (Panel)target;

            Action<object> addToPanel = delegate(object view)
            {
                var el = view as UIElement;
                if (el != null && !panel.Children.Contains(el))
                    panel.Children.Add(el);
            };

            Action<object> removeFromPanel = delegate(object view)
            {
                var el = view as UIElement;
                if (el != null)
                    panel.Children.Remove(el);
            };

            foreach (var v in region.Views)
                addToPanel(v);

            EventHandler<RegionViewsChangedEventArgs> handler = delegate(object s, RegionViewsChangedEventArgs e)
            {
                foreach (var r in e.Removed)
                    removeFromPanel(r);

                foreach (var a in e.Added)
                    addToPanel(a);
            };

            WeakEventManager<IRegion, RegionViewsChangedEventArgs>.AddHandler(
                region, nameof(IRegion.ViewsChanged), handler);

            RoutedEventHandler unloaded = null;
            unloaded = delegate(object s, RoutedEventArgs e)
            {
                panel.Unloaded -= unloaded;

                WeakEventManager<IRegion, RegionViewsChangedEventArgs>.RemoveHandler(
                    region, nameof(IRegion.ViewsChanged), handler);

                panel.Children.Clear();
            };
            panel.Unloaded += unloaded;

            return new SimpleAttachment(delegate
            {
                panel.Unloaded -= unloaded;

                WeakEventManager<IRegion, RegionViewsChangedEventArgs>.RemoveHandler(
                    region, nameof(IRegion.ViewsChanged), handler);

                panel.Children.Clear();
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
