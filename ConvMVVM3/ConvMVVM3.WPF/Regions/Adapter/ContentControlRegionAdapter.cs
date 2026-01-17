using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.WPF.Abstractions;

namespace ConvMVVM3.WPF.Regions.Adapter
{
    public sealed class ContentControlRegionAdapter : IRegionAdapter
    {
        public Type TargetType { get { return typeof(ContentControl); } }

        // target(ContentControl)별로 attach 상태를 내부에서 보관
        private static readonly ConditionalWeakTable<ContentControl, Attachment> _map =
            new ConditionalWeakTable<ContentControl, Attachment>();

        public void Attach(DependencyObject target, IRegion region)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (region == null) throw new ArgumentNullException("region");

            var cc = target as ContentControl;
            if (cc == null) throw new InvalidOperationException("Target must be ContentControl.");

            // 이미 붙어있으면 정리 후 다시 붙임(Loaded 여러 번 대비)
            Attachment old;
            if (_map.TryGetValue(cc, out old))
            {
                old.Detach();
                _map.Remove(cc);
            }

            var att = new Attachment(cc, region, delegate { _map.Remove(cc); });
            _map.Add(cc, att);
            att.Attach();
        }

        private sealed class Attachment
        {
            private readonly ContentControl _cc;
            private readonly IRegion _region;
            private readonly Action _removeSelf;

            private EventHandler<RegionActiveViewChangedEventArgs> _activeChanged;
            private RoutedEventHandler _unloaded;

            private bool _attached;

            public Attachment(ContentControl cc, IRegion region, Action removeSelf)
            {
                _cc = cc;
                _region = region;
                _removeSelf = removeSelf;
            }

            public void Attach()
            {
                if (_attached) return;
                _attached = true;

                // 1) 초기 반영
                ApplyActiveView();

                // 2) ActiveViewChanged 구독 (WeakEvent로 누수 방지)
                _activeChanged = delegate (object s, RegionActiveViewChangedEventArgs e)
                {
                    // 가장 단순: ActiveView만 UI에 반영
                    ApplyActiveView();
                };

                WeakEventManager<IRegion, RegionActiveViewChangedEventArgs>.AddHandler(
                    _region, nameof(IRegion.ActiveViewChanged), _activeChanged);

                // 3) 필요 시 첫 뷰 활성화(정책: 유지/삭제 선택)
                if (_region.Views.Count > 0 && _region.ActiveView == null)
                    _region.Activate(_region.Views[0], null);

                // 4) 컨트롤이 트리에서 빠질 때 자동 정리
                _unloaded = delegate (object sender, RoutedEventArgs e)
                {
                    Detach();
                    _removeSelf();
                };
                _cc.Unloaded += _unloaded;
            }

            private void ApplyActiveView()
            {
                _cc.Content = _region.ActiveView;
            }

            public void Detach()
            {
                if (!_attached) return;
                _attached = false;

                if (_activeChanged != null)
                {
                    WeakEventManager<IRegion, RegionActiveViewChangedEventArgs>.RemoveHandler(
                        _region, nameof(IRegion.ActiveViewChanged), _activeChanged);
                    _activeChanged = null;
                }

                if (_unloaded != null)
                {
                    _cc.Unloaded -= _unloaded;
                    _unloaded = null;
                }

                _cc.ClearValue(ContentControl.ContentProperty);
            }
        }
    }
}