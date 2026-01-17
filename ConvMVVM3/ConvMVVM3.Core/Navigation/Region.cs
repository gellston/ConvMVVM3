using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ConvMVVM3.Core.Navigation.Abstractions;

namespace ConvMVVM3.Core.Navigation
{
    /// <summary>
    /// Default implementation of IRegion.
    /// Manages a collection of views and tracks the active view.
    /// </summary>
    public class Region : IRegion
    {
        private readonly List<object> _views;
        private object _activeView;
        private readonly string _name;
        private readonly RegionMetadata _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="Region"/> class.
        /// </summary>
        /// <param name="name">The name of the region.</param>
        public Region(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _metadata = new RegionMetadata(name);
            _views = new List<object>();
        }

        /// <summary>
        /// Gets the name of the region.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Gets the metadata associated with the region.
        /// </summary>
        public IRegionMetadata Metadata => _metadata;

        /// <summary>
        /// Gets the collection of views currently contained in the region.
        /// </summary>
        public IEnumerable<object> Views => new ReadOnlyCollection<object>(_views);

        /// <summary>
        /// Gets the currently active view in the region.
        /// </summary>
        public object ActiveView => _activeView;

        /// <summary>
        /// Occurs when a view is activated in the region.
        /// </summary>
        public event EventHandler<ViewActivatedEventArgs> ViewActivated;

        /// <summary>
        /// Occurs when a view is deactivated in the region.
        /// </summary>
        public event EventHandler<ViewDeactivatedEventArgs> ViewDeactivated;

        /// <summary>
        /// Adds a view to the region.
        /// </summary>
        /// <param name="view">The view to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when view is null.</exception>
        /// <exception cref="ArgumentException">Thrown when view is already in the region.</exception>
        public void Add(object view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (_views.Contains(view))
                throw new ArgumentException("View is already in the region.");

            _views.Add(view);
        }

        /// <summary>
        /// Removes a view from the region.
        /// </summary>
        /// <param name="view">The view to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when view is null.</exception>
        public void Remove(object view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (_views.Remove(view))
            {
                // If the removed view was active, deactivate it
                if (_activeView == view)
                {
                    Deactivate(view);
                }
            }
        }

        /// <summary>
        /// Activates a view in the region.
        /// </summary>
        /// <param name="view">The view to activate.</param>
        /// <exception cref="ArgumentNullException">Thrown when view is null.</exception>
        /// <exception cref="ArgumentException">Thrown when view is not in the region.</exception>
        public void Activate(object view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (!_views.Contains(view))
                throw new ArgumentException("View is not in the region.");

            // If this view is already active, do nothing
            if (_activeView == view)
                return;

            // Deactivate the current active view
            if (_activeView != null)
            {
                Deactivate(_activeView);
            }

            // Activate the new view
            _activeView = view;
            OnViewActivated(new ViewActivatedEventArgs(view));
        }

        /// <summary>
        /// Deactivates a view in the region.
        /// </summary>
        /// <param name="view">The view to deactivate.</param>
        /// <exception cref="ArgumentNullException">Thrown when view is null.</exception>
        /// <exception cref="ArgumentException">Thrown when view is not active.</exception>
        public void Deactivate(object view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (_activeView != view)
                throw new ArgumentException("View is not active.");

            _activeView = null;
            OnViewDeactivated(new ViewDeactivatedEventArgs(view));
        }

        /// <summary>
        /// Clears all views from the region.
        /// </summary>
        public void Clear()
        {
            // Deactivate current active view
            if (_activeView != null)
            {
                OnViewDeactivated(new ViewDeactivatedEventArgs(_activeView));
                _activeView = null;
            }

            // Clear all views
            _views.Clear();
        }

        /// <summary>
        /// Gets a value indicating whether the region contains the specified view.
        /// </summary>
        /// <param name="view">The view to check.</param>
        /// <returns>true if the region contains the view; otherwise, false.</returns>
        public bool ContainsView(object view)
        {
            return view != null && _views.Contains(view);
        }

        /// <summary>
        /// Gets a value indicating whether the specified view is active.
        /// </summary>
        /// <param name="view">The view to check.</param>
        /// <returns>true if the view is active; otherwise, false.</returns>
        public bool IsActive(object view)
        {
            return view != null && _activeView == view;
        }

        /// <summary>
        /// Raises the ViewActivated event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnViewActivated(ViewActivatedEventArgs e)
        {
            ViewActivated?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the ViewDeactivated event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnViewDeactivated(ViewDeactivatedEventArgs e)
        {
            ViewDeactivated?.Invoke(this, e);
        }
    }
}