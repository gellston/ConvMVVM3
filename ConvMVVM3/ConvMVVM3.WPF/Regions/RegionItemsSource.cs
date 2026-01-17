using System;
using System.Collections.ObjectModel;
using System.Windows;
using ConvMVVM3.Core.Mvvm.Regions;

namespace ConvMVVM3.WPF.Regions
{
    /// <summary>
    /// ObservableCollection that mirrors region.Views via region.ViewsChanged.
    /// Uses WeakEventManager to avoid strong event-handler retention.
    /// Still implements IDisposable for deterministic detach/cleanup.
    /// </summary>
    internal sealed class RegionItemsSource : ObservableCollection<object>, IDisposable
    {
        private readonly IRegion _region;
        private readonly EventHandler<RegionViewsChangedEventArgs> _handler;
        private bool _disposed;

        public RegionItemsSource(IRegion region)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));

            _region = region;

            foreach (var v in region.Views)
                Add(v);

            _handler = OnViewsChanged;

            WeakEventManager<IRegion, RegionViewsChangedEventArgs>.AddHandler(
                _region, nameof(IRegion.ViewsChanged), _handler);
        }

        private void OnViewsChanged(object sender, RegionViewsChangedEventArgs e)
        {
            foreach (var r in e.Removed)
                Remove(r);

            foreach (var a in e.Added)
            {
                if (!Contains(a))
                    Add(a);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            WeakEventManager<IRegion, RegionViewsChangedEventArgs>.RemoveHandler(
                _region, nameof(IRegion.ViewsChanged), _handler);

            Clear();
        }
    }
}
