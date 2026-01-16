using System;
using System.Collections.Generic;

namespace ConvMVVM3.Core.Navigation.Abstractions
{
    /// <summary>
    /// Defines the interface for a region manager.
    /// </summary>
    public interface IRegionManager
    {
        /// <summary>
        /// Gets the region with the specified name.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <returns>The region.</returns>
        IRegion this[string regionName] { get; }

        /// <summary>
        /// Creates a new region with the specified name.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <returns>The created region.</returns>
        IRegion CreateRegion(string regionName);

        /// <summary>
        /// Navigates to a view in the specified region.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <param name="viewType">The type of the view to navigate to.</param>
        void Navigate(string regionName, Type viewType);

        /// <summary>
        /// Navigates to a view in the specified region.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <param name="viewName">The name of the view to navigate to.</param>
        void Navigate(string regionName, string viewName);

        /// <summary>
        /// Navigates to a view in the specified region using a factory.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <param name="viewFactory">The factory to create the view.</param>
        void Navigate(string regionName, Func<object> viewFactory);

        /// <summary>
        /// Registers a view type with a region.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <param name="viewType">The type of the view.</param>
        void RegisterViewWithRegion(string regionName, Type viewType);

        /// <summary>
        /// Registers a view factory with a region.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <param name="viewFactory">The factory to create the view.</param>
        void RegisterViewWithRegion(string regionName, Func<object> viewFactory);

        /// <summary>
        /// Adds a region to the manager.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <param name="region">The region to add.</param>
        void AddRegion(string regionName, IRegion region);

        /// <summary>
        /// Removes a region from the manager.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        void RemoveRegion(string regionName);

        /// <summary>
        /// Gets the names of all regions.
        /// </summary>
        /// <returns>The region names.</returns>
        IEnumerable<string> GetRegionNames();

        /// <summary>
        /// Determines whether a region with the specified name exists.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <returns>true if the region exists; otherwise, false.</returns>
        bool ContainsRegion(string regionName);
    }
}