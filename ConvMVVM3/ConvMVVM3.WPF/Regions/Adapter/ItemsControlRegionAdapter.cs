using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.WPF.Abstractions;
using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ConvMVVM3.WPF.Regions.Adapter
{
    public sealed class ItemsControlRegionAdapter : IRegionAdapter
    {
        public Type TargetType { get { return typeof(ItemsControl); } }

        // ItemsControl 별로 현재 attach 상태를 저장
        private static readonly ConditionalWeakTable<ItemsControl, Attachment> _map =
            new ConditionalWeakTable<ItemsControl, Attachment>();

        public void Attach(DependencyObject target, IRegion region)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (region == null) throw new ArgumentNullException("region");

            var itemsControl = target as ItemsControl;
            if (itemsControl == null) throw new InvalidOperationException("Target must be ItemsControl.");

            // 이미 붙어있으면 정리 후 재부착(Loaded 재진입/RegionName 변경 대비)
            Attachment old;
            if (_map.TryGetValue(itemsControl, out old))
            {
                old.Detach();
                _map.Remove(itemsControl);
            }

            var att = new Attachment(itemsControl, region, delegate { _map.Remove(itemsControl); });
            _map.Add(itemsControl, att);
            att.Attach();
        }

        private sealed class Attachment
        {
            private readonly ItemsControl _itemsControl;
            private readonly IRegion _region;
            private readonly Action _removeSelf;

            private RoutedEventHandler _unloaded;
            private bool _attached;

            public Attachment(ItemsControl itemsControl, IRegion region, Action removeSelf)
            {
                _itemsControl = itemsControl;
                _region = region;
                _removeSelf = removeSelf;
            }

            public void Attach()
            {
                if (_attached) return;
                _attached = true;

                // 기존 ItemsSource가 IDisposable이면 정리
                DisposeItemsSource(_itemsControl.ItemsSource);

                // RegionItemsSource로 연결
                _itemsControl.ItemsSource = new RegionItemsSource(_region);

                // Unloaded 때 자동 정리
                _unloaded = delegate (object s, RoutedEventArgs e)
                {
                    Detach();
                    _removeSelf();
                };
                _itemsControl.Unloaded += _unloaded;
            }

            public void Detach()
            {
                if (!_attached) return;
                _attached = false;

                if (_unloaded != null)
                {
                    _itemsControl.Unloaded -= _unloaded;
                    _unloaded = null;
                }

                DisposeItemsSource(_itemsControl.ItemsSource);
                _itemsControl.ItemsSource = null;
            }

            private static void DisposeItemsSource(object itemsSource)
            {
                var disp = itemsSource as IDisposable;
                if (disp != null) disp.Dispose();
            }
        }
    }
}
