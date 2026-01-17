using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.WPF.Abstractions;

namespace ConvMVVM3.WPF.Regions.Adapter
{


    public sealed class PanelRegionAdapter : IRegionAdapter
    {
        public Type TargetType { get { return typeof(Panel); } }

        // Panel 별 attach 상태 보관
        private static readonly ConditionalWeakTable<Panel, Attachment> _map =
            new ConditionalWeakTable<Panel, Attachment>();

        public void Attach(DependencyObject target, IRegion region)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (region == null) throw new ArgumentNullException("region");

            var panel = target as Panel;
            if (panel == null) throw new InvalidOperationException("Target must be Panel.");

            // 이미 붙어 있으면 정리 후 재부착
            Attachment old;
            if (_map.TryGetValue(panel, out old))
            {
                old.Detach();
                _map.Remove(panel);
            }

            var att = new Attachment(panel, region, delegate { _map.Remove(panel); });
            _map.Add(panel, att);
            att.Attach();
        }

        private sealed class Attachment
        {
            private readonly Panel _panel;
            private readonly IRegion _region;
            private readonly Action _removeSelf;

            private EventHandler<RegionViewsChangedEventArgs> _handler;
            private RoutedEventHandler _unloaded;
            private bool _attached;

            public Attachment(Panel panel, IRegion region, Action removeSelf)
            {
                _panel = panel;
                _region = region;
                _removeSelf = removeSelf;
            }

            public void Attach()
            {
                if (_attached) return;
                _attached = true;

                // 1) 현재 Views를 panel에 반영
                foreach (var v in _region.Views)
                    AddToPanel(v);

                // 2) ViewsChanged 구독
                _handler = delegate (object s, RegionViewsChangedEventArgs e)
                {
                    // 제거
                    foreach (var r in e.Removed)
                        RemoveFromPanel(r);

                    // 추가
                    foreach (var a in e.Added)
                        AddToPanel(a);
                };

                WeakEventManager<IRegion, RegionViewsChangedEventArgs>.AddHandler(
                    _region, nameof(IRegion.ViewsChanged), _handler);

                // 3) Unloaded 때 자동 정리
                _unloaded = delegate (object s, RoutedEventArgs e)
                {
                    Detach();
                    _removeSelf();
                };
                _panel.Unloaded += _unloaded;
            }

            public void Detach()
            {
                if (!_attached) return;
                _attached = false;

                if (_unloaded != null)
                {
                    _panel.Unloaded -= _unloaded;
                    _unloaded = null;
                }

                if (_handler != null)
                {
                    WeakEventManager<IRegion, RegionViewsChangedEventArgs>.RemoveHandler(
                        _region, nameof(IRegion.ViewsChanged), _handler);
                    _handler = null;
                }

                // 원본과 동일 정책: panel children 전체 제거
                _panel.Children.Clear();
            }

            private void AddToPanel(object view)
            {
                var el = view as UIElement;
                if (el != null && !_panel.Children.Contains(el))
                    _panel.Children.Add(el);
            }

            private void RemoveFromPanel(object view)
            {
                var el = view as UIElement;
                if (el != null)
                    _panel.Children.Remove(el);
            }
        }
    }
}
