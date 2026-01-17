using System;
using System.Collections.Generic;
using System.Linq;
using ConvMVVM3.Core.Navigation.Abstractions;

namespace ConvMVVM3.Core.Navigation
{
    /// <summary>
    /// Default implementation of IRegionManager.
    /// Manages regions and provides navigation capabilities.
    /// </summary>
    public class RegionManager : IRegionManager
    {
        private readonly Dictionary<string, IRegion> _regions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegionManager"/> class.
        /// </summary>
        public RegionManager()
        {
            _regions = new Dictionary<string, IRegion>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the region with the specified name.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <returns>The region, or null if not found.</returns>
        public IRegion this[string regionName]
        {
            get
            {
                if (regionName == null)
                    return null;

                _regions.TryGetValue(regionName, out var region);
                return region;
            }
        }

        /// <summary>
        /// Creates a new region with the specified name.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <returns>The created region.</returns>
        /// <exception cref="ArgumentException">Thrown when regionName is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a region with the same name already exists.</exception>
        public IRegion CreateRegion(string regionName)
        {
            if (string.IsNullOrWhiteSpace(regionName))
                throw new ArgumentException("Region name cannot be null or empty.", nameof(regionName));

            if (_regions.ContainsKey(regionName))
                throw new InvalidOperationException($"A region with name '{regionName}' already exists.");

            var region = new Region(regionName);
            return region;
        }

        /// <summary>
        /// Navigates to a view in the specified region.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <param name="viewType">The type of the view to navigate to.</param>
        /// <exception cref="ArgumentException">Thrown when regionName is null or empty, or viewType is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the region is not found.</exception>
        public void Navigate(string regionName, Type viewType)
        {
            Navigate(regionName, () => CreateViewInstance(viewType));
        }

        /// <summary>
        /// Navigates to a view in the specified region using a view name lookup.
        /// Note: This method requires a mapping from view name to type or factory.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <param name="viewName">The name of the view to navigate to.</param>
        /// <exception cref="NotSupportedException">Thrown when view name to type mapping is not implemented.</exception>
        public void Navigate(string regionName, string viewName)
        {
            throw new NotSupportedException("Navigation by view name is not supported. Use viewType or viewFactory instead.");
        }

        /// <summary>
        /// Navigates to a view in the specified region using a factory.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <param name="viewFactory">The factory to create the view.</param>
        /// <exception cref="ArgumentException">Thrown when regionName is null or empty, or viewFactory is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the region is not found.</exception>
        public void Navigate(string regionName, Func<object> viewFactory)
        {
            if (string.IsNullOrWhiteSpace(regionName))
                throw new ArgumentException("Region name cannot be null or empty.", nameof(regionName));

            if (viewFactory == null)
                throw new ArgumentNullException(nameof(viewFactory));

            var region = this[regionName];
            if (region == null)
                throw new InvalidOperationException($"Region '{regionName}' not found.");

            try
            {
                var view = viewFactory();
                if (view == null)
                    throw new InvalidOperationException("View factory returned null.");

                // Add view to region if not already present
                if (!region.Views.Contains(view))
                {
                    region.Add(view);
                }

                // Activate the view
                region.Activate(view);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to navigate to view in region '{regionName}'.", ex);
            }
        }

        /// <summary>
        /// Registers a view type with a region.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <param name="viewType">The type of the view.</param>
        /// <exception cref="ArgumentException">Thrown when regionName is null or empty, or viewType is null.</exception>
        public void RegisterViewWithRegion(string regionName, Type viewType)
        {
            if (string.IsNullOrWhiteSpace(regionName))
                throw new ArgumentException("Region name cannot be null or empty.", nameof(regionName));

            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            var region = this[regionName];
            if (region == null)
            {
                // Create region if it doesn't exist
                region = CreateRegion(regionName);
                AddRegion(regionName, region);
            }

            // Register view type with metadata
            if (region.Metadata is RegionMetadata metadata)
            {
                metadata.RegisterViewType(viewType);
            }
        }

        /// <summary>
        /// Registers a view factory with a region.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <param name="viewFactory">The factory to create the view.</param>
        /// <exception cref="ArgumentException">Thrown when regionName is null or empty, or viewFactory is null.</exception>
        public void RegisterViewWithRegion(string regionName, Func<object> viewFactory)
        {
            if (string.IsNullOrWhiteSpace(regionName))
                throw new ArgumentException("Region name cannot be null or empty.", nameof(regionName));

            if (viewFactory == null)
                throw new ArgumentNullException(nameof(viewFactory));

            var region = this[regionName];
            if (region == null)
            {
                // Create region if it doesn't exist
                region = CreateRegion(regionName);
                AddRegion(regionName, region);
            }

            // Register view factory with metadata
            if (region.Metadata is RegionMetadata metadata)
            {
                metadata.RegisterViewFactory(viewFactory);
            }
        }

        /// <summary>
        /// Adds a region to the manager.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <param name="region">The region to add.</param>
        /// <exception cref="ArgumentException">Thrown when regionName is null or empty, or region is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a region with the same name already exists.</exception>
        public void AddRegion(string regionName, IRegion region)
        {
            if (string.IsNullOrWhiteSpace(regionName))
                throw new ArgumentException("Region name cannot be null or empty.", nameof(regionName));

            if (region == null)
                throw new ArgumentNullException(nameof(region));

            if (_regions.ContainsKey(regionName))
                throw new InvalidOperationException($"A region with name '{regionName}' already exists.");

            _regions[regionName] = region;
        }

        /// <summary>
        /// Removes a region from the manager.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        public void RemoveRegion(string regionName)
        {
            if (!string.IsNullOrWhiteSpace(regionName))
            {
                _regions.Remove(regionName);
            }
        }

        /// <summary>
        /// Gets the names of all regions.
        /// </summary>
        /// <returns>The region names.</returns>
        public IEnumerable<string> GetRegionNames()
        {
            return _regions.Keys.ToList().AsReadOnly();
        }

        /// <summary>
        /// Determines whether a region with the specified name exists.
        /// </summary>
        /// <param name="regionName">The name of the region.</param>
        /// <returns>true if the region exists; otherwise, false.</returns>
        public bool ContainsRegion(string regionName)
        {
            if (string.IsNullOrWhiteSpace(regionName))
                return false;

            return _regions.ContainsKey(regionName);
        }

        /// <summary>
        /// Creates an instance of the specified view type.
        /// </summary>
        /// <param name="viewType">The type of the view to create.</param>
        /// <returns>The created view instance.</returns>
        /// <exception cref="ArgumentException">Thrown when viewType is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the view type cannot be instantiated.</exception>
        private static object CreateViewInstance(Type viewType)
        {
            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            try
            {
                // Try to create instance using parameterless constructor
                return Activator.CreateInstance(viewType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create instance of view type '{viewType.FullName}'. Ensure the type has a parameterless constructor.", ex);
            }
        }

        /// <summary>
        /// Clears all regions from the manager.
        /// </summary>
        public void Clear()
        {
            _regions.Clear();
        }

        /// <summary>
        /// Gets the count of regions managed by this region manager.
        /// </summary>
        public int Count => _regions.Count;
    }
}