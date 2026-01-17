using System;
using System.Collections.Generic;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public sealed class Region : IRegion
    {
        private readonly List<object> _views = new List<object>();
        private readonly object _gate = new object();
        private object _activeView;
        private readonly bool _singleActive;

        public Region(string name, bool singleActive)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Region name is required.", nameof(name));

            Name = name;
            _singleActive = singleActive;
        }

        public string Name { get; private set; }

        public IReadOnlyList<object> Views
        {
            get
            {
                lock (_gate) return _views.ToArray();
            }
        }

        public object ActiveView
        {
            get
            {
                lock (_gate) return _activeView;
            }
        }

        public object SelectedItem
        {
            get { return ActiveView; }
            set
            {
                if (value == null) Deactivate(null);
                else Activate(value, null);
            }
        }

        public event EventHandler<RegionViewsChangedEventArgs> ViewsChanged;
        public event EventHandler<RegionActiveViewChangedEventArgs> ActiveViewChanged;

        public void Add(object view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));

            bool added;
            lock (_gate)
            {
                added = !_views.Contains(view);
                if (added) _views.Add(view);
            }

            if (added)
            {
                var handler = ViewsChanged;
                if (handler != null)
                    handler(this, new RegionViewsChangedEventArgs(new[] { view }, new object[0]));
            }
        }

        public bool Remove(object view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));

            bool removed;
            object oldActive = null;

            lock (_gate)
            {
                removed = _views.Remove(view);
                if (!removed) return false;

                if (object.ReferenceEquals(_activeView, view))
                {
                    oldActive = _activeView;
                    _activeView = null;
                }
            }

            var vh = ViewsChanged;
            if (vh != null)
                vh(this, new RegionViewsChangedEventArgs(new object[0], new[] { view }));

            if (oldActive != null)
            {
                var ah = ActiveViewChanged;
                if (ah != null)
                    ah(this, new RegionActiveViewChangedEventArgs(oldActive, null, null));
            }

            return true;
        }

        public void Activate(object view, INavigationContext context)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));

            object old;
            bool added = false;
            bool same;

            lock (_gate)
            {
                if (!_views.Contains(view))
                {
                    _views.Add(view);
                    added = true;
                }

                old = _activeView;
                same = object.ReferenceEquals(old, view);
                if (!same)
                    _activeView = view;
            }

            if (same) return;

            if (added)
            {
                var vh = ViewsChanged;
                if (vh != null)
                    vh(this, new RegionViewsChangedEventArgs(new[] { view }, new object[0]));
            }

            var ah2 = ActiveViewChanged;
            if (ah2 != null)
                ah2(this, new RegionActiveViewChangedEventArgs(old, view, context));

            if (_singleActive && old != null && !object.ReferenceEquals(old, view))
                Remove(old);
        }

        public void Deactivate(INavigationContext context)
        {
            object old;
            lock (_gate)
            {
                if (_activeView == null) return;
                old = _activeView;
                _activeView = null;
            }

            var ah = ActiveViewChanged;
            if (ah != null)
                ah(this, new RegionActiveViewChangedEventArgs(old, null, context));
        }
    }
}
