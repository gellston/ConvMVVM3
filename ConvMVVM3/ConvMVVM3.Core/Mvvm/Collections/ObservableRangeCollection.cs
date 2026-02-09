using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Collections
{
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification;

        public ObservableRangeCollection() { }

        public ObservableRangeCollection(IEnumerable<T> collection) : base(collection) { }

        /// <summary>
        /// 여러 항목을 한 번에 추가한다. (기본: Reset 알림 1회)
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            var list = items as IList<T> ?? items.ToList();
            if (list.Count == 0) return;

            using (DeferNotifications())
            {
                foreach (var item in list)
                    Items.Add(item);
            }

            RaiseReset();
        }

        /// <summary>
        /// 여러 항목을 한 번에 제거한다. (기본: Reset 알림 1회)
        /// </summary>
        public void RemoveRange(IEnumerable<T> items)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            var list = items as IList<T> ?? items.ToList();
            if (list.Count == 0) return;

            using (DeferNotifications())
            {
                // Items.Remove는 O(n)일 수 있으니 대량이면 HashSet으로 최적화
                var set = new HashSet<T>(list);
                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    if (set.Contains(Items[i]))
                        Items.RemoveAt(i);
                }
            }

            RaiseReset();
        }

        /// <summary>
        /// 컬렉션 전체를 새 항목들로 교체한다. (기본: Reset 알림 1회)
        /// </summary>
        public void ReplaceRange(IEnumerable<T> items)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            var list = items as IList<T> ?? items.ToList();

            using (DeferNotifications())
            {
                Items.Clear();
                foreach (var item in list)
                    Items.Add(item);
            }

            RaiseReset();
        }

        /// <summary>
        /// 모든 항목 제거 + Reset 알림
        /// </summary>
        public void ClearAndNotify()
        {
            if (Count == 0) return;

            using (DeferNotifications())
            {
                Items.Clear();
            }

            RaiseReset();
        }

        /// <summary>
        /// 알림을 잠시 멈추고, Dispose 시점에 다시 켤 수 있는 스코프.
        /// 스코프 밖에서 RaiseReset()을 호출하는 패턴을 권장.
        /// </summary>
        public IDisposable DeferNotifications()
            => new NotificationDeferral(this);

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_suppressNotification) return;
            base.OnCollectionChanged(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_suppressNotification) return;
            base.OnPropertyChanged(e);
        }

        private void RaiseReset()
        {
            // Count/Indexer 변경 + Reset 이벤트
            base.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            base.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private sealed class NotificationDeferral : IDisposable
        {
            private readonly ObservableRangeCollection<T> _owner;
            private bool _disposed;

            public NotificationDeferral(ObservableRangeCollection<T> owner)
            {
                _owner = owner;
                _owner._suppressNotification = true;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _owner._suppressNotification = false;
            }
        }
    }
}
