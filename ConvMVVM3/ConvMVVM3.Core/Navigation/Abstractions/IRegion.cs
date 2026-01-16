using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ConvMVVM3.Core.Navigation.Abstractions
{
    /// <summary>
    /// Defines a region that can display views.
    /// </summary>
    public interface IRegion
    {
        /// <summary>
        /// Gets the name of the region.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the metadata associated with the region.
        /// </summary>
        IRegionMetadata Metadata { get; }

        /// <summary>
        /// Gets the collection of views currently contained in the region.
        /// </summary>
        IEnumerable<object> Views { get; }

        /// <summary>
        /// Gets the currently active view in the region.
        /// </summary>
        object ActiveView { get; }

        /// <summary>
        /// Adds a view to the region.
        /// </summary>
        /// <param name="view">The view to add.</param>
        void Add(object view);

        /// <summary>
        /// Removes a view from the region.
        /// </summary>
        /// <param name="view">The view to remove.</param>
        void Remove(object view);

        /// <summary>
        /// Activates a view in the region.
        /// </summary>
        /// <param name="view">The view to activate.</param>
        void Activate(object view);

        /// <summary>
        /// Deactivates a view in the region.
        /// </summary>
        /// <param name="view">The view to deactivate.</param>
        void Deactivate(object view);

        /// <summary>
        /// Occurs when a view is activated in the region.
        /// </summary>
        event EventHandler<ViewActivatedEventArgs> ViewActivated;

        /// <summary>
        /// Occurs when a view is deactivated in the region.
        /// </summary>
        event EventHandler<ViewDeactivatedEventArgs> ViewDeactivated;
    }

    /// <summary>
    /// Provides data for the ViewActivated event.
    /// </summary>
    public class ViewActivatedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewActivatedEventArgs"/> class.
        /// </summary>
        /// <param name="view">The activated view.</param>
        public ViewActivatedEventArgs(object view)
        {
            View = view;
        }

        /// <summary>
        /// Gets the activated view.
        /// </summary>
        public object View { get; }
    }

    /// <summary>
    /// Provides data for the ViewDeactivated event.
    /// </summary>
    public class ViewDeactivatedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewDeactivatedEventArgs"/> class.
        /// </summary>
        /// <param name="view">The deactivated view.</param>
        public ViewDeactivatedEventArgs(object view)
        {
            View = view;
        }

        /// <summary>
        /// Gets the deactivated view.
        /// </summary>
        public object View { get; }
    }
}