using System;
using System.Windows;
using System.Windows.Controls;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.WPF.Abstractions;

namespace ConvMVVM3.WPF.Regions
{
    public sealed class ItemsControlRegionAdapter : IRegionAdapter
    {
        public Type TargetType { get { return typeof(ItemsControl); } }

        public IRegionAttachment Attach(DependencyObject target, IRegion region)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (region == null) throw new ArgumentNullException(nameof(region));

            var itemsControl = (ItemsControl)target;

            var oldDisp = itemsControl.ItemsSource as IDisposable;
            if (oldDisp != null) oldDisp.Dispose();

            var source = new RegionItemsSource(region);
            itemsControl.ItemsSource = source;

            RoutedEventHandler unloaded = null;
            unloaded = delegate(object s, RoutedEventArgs e)
            {
                itemsControl.Unloaded -= unloaded;

                var disp = itemsControl.ItemsSource as IDisposable;
                if (disp != null) disp.Dispose();
                itemsControl.ItemsSource = null;
            };
            itemsControl.Unloaded += unloaded;

            return new SimpleAttachment(delegate
            {
                itemsControl.Unloaded -= unloaded;

                var disp = itemsControl.ItemsSource as IDisposable;
                if (disp != null) disp.Dispose();
                itemsControl.ItemsSource = null;
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
