using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.WPF.Abstractions;
using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ConvMVVM3.WPF.Regions.Adapter
{
    /// <summary>
    /// Adapter for Selector-based controls (ListBox, TabControl, ComboBox, etc).
    /// Synchronizes SelectedItem <-> region.SelectedItem (ActiveView).
    /// </summary>
    public sealed class SelectorRegionAdapter : IRegionAdapter
    {
        public Type TargetType { get { return typeof(Selector); } }

        private static readonly ConditionalWeakTable<Selector, Attachment> _map =
            new ConditionalWeakTable<Selector, Attachment>();

        public void Attach(DependencyObject target, IRegion region)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (region == null) throw new ArgumentNullException("region");

            var selector = target as Selector;
            if (selector == null) throw new InvalidOperationException("Target must be Selector.");

            // 이미 attach 된 상태면 정리 후 재부착
            Attachment old;
            if (_map.TryGetValue(selector, out old))
            {
                old.Detach();
                _map.Remove(selector);
            }

            var att = new Attachment(selector, region, delegate { _map.Remove(selector); });
            _map.Add(selector, att);
            att.Attach();
        }

        private sealed class Attachment
        {
            private readonly Selector _selector;
            private readonly IRegion _region;
            private readonly Action _removeSelf;

            private bool _attached;
            private bool _syncing;

            private SelectionChangedEventHandler _uiChanged;
            private EventHandler<RegionActiveViewChangedEventArgs> _regionChanged;
            private RoutedEventHandler _unloaded;

            public Attachment(Selector selector, IRegion region, Action removeSelf)
            {
                _selector = selector;
                _region = region;
                _removeSelf = removeSelf;
            }

            public void Attach()
            {
                if (_attached) return;
                _attached = true;

                // 기존 ItemsSource가 IDisposable이면 정리
                DisposeItemsSource(_selector.ItemsSource);

                // RegionItemsSource로 연결
                _selector.ItemsSource = new RegionItemsSource(_region);

                // UI -> Region SelectedItem 동기화
                _uiChanged = delegate (object s, SelectionChangedEventArgs e)
                {
                    if (_syncing) return;
                    _syncing = true;
                    try
                    {
                        if (!object.Equals(_region.SelectedItem, _selector.SelectedItem))
                            _region.SelectedItem = _selector.SelectedItem;
                    }
                    finally
                    {
                        _syncing = false;
                    }
                };
                _selector.SelectionChanged += _uiChanged;

                // Region -> UI SelectedItem 동기화 + 네비게이션 디스패치(기존 동작 유지)
                _regionChanged = delegate (object s, RegionActiveViewChangedEventArgs e)
                {
                    if (_syncing) return;
                    _syncing = true;
                    try
                    {
                        _selector.SelectedItem = _region.SelectedItem;
                    }
                    finally
                    {
                        _syncing = false;
                    }

                    // 기존 코드 유지
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
                    _region, nameof(IRegion.ActiveViewChanged), _regionChanged);

                // 초기 상태 반영
                _selector.SelectedItem = _region.SelectedItem;

                // Unloaded 때 자동 정리
                _unloaded = delegate (object s, RoutedEventArgs e)
                {
                    Detach();
                    _removeSelf();
                };
                _selector.Unloaded += _unloaded;
            }

            public void Detach()
            {
                if (!_attached) return;
                _attached = false;

                if (_unloaded != null)
                {
                    _selector.Unloaded -= _unloaded;
                    _unloaded = null;
                }

                if (_uiChanged != null)
                {
                    _selector.SelectionChanged -= _uiChanged;
                    _uiChanged = null;
                }

                if (_regionChanged != null)
                {
                    WeakEventManager<IRegion, RegionActiveViewChangedEventArgs>.RemoveHandler(
                        _region, nameof(IRegion.ActiveViewChanged), _regionChanged);
                    _regionChanged = null;
                }

                DisposeItemsSource(_selector.ItemsSource);
                _selector.ItemsSource = null;
            }

            private static void DisposeItemsSource(object itemsSource)
            {
                var disp = itemsSource as IDisposable;
                if (disp != null) disp.Dispose();
            }
        }
    }
}
