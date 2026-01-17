using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ConvMVVM3.Core.Navigation.Abstractions;

namespace ConvMVVM3.Core.Navigation
{
    /// <summary>
    /// Default implementation of IRegionMetadata.
    /// Provides metadata for a region including registered view types and factories.
    /// </summary>
    public class RegionMetadata : IRegionMetadata
    {
        private readonly List<Type> _viewTypes;
        private readonly List<Func<object>> _viewFactories;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegionMetadata"/> class.
        /// </summary>
        /// <param name="name">The name of the region.</param>
        public RegionMetadata(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _viewTypes = new List<Type>();
            _viewFactories = new List<Func<object>>();
        }

        /// <summary>
        /// Gets the name of the region.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the types of views registered with the region.
        /// </summary>
        public IEnumerable<Type> ViewTypes => new ReadOnlyCollection<Type>(_viewTypes);

        /// <summary>
        /// Gets the view factories registered with the region.
        /// </summary>
        public IEnumerable<Func<object>> ViewFactories => new ReadOnlyCollection<Func<object>>(_viewFactories);

        /// <summary>
        /// Registers a view type with the region metadata.
        /// </summary>
        /// <param name="viewType">The type of the view to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when viewType is null.</exception>
        /// <exception cref="ArgumentException">Thrown when viewType is already registered.</exception>
        public void RegisterViewType(Type viewType)
        {
            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            if (_viewTypes.Contains(viewType))
                throw new ArgumentException($"View type '{viewType.FullName}' is already registered with region '{Name}'.");

            _viewTypes.Add(viewType);
        }

        /// <summary>
        /// Registers a view factory with the region metadata.
        /// </summary>
        /// <param name="viewFactory">The factory to create views.</param>
        /// <exception cref="ArgumentNullException">Thrown when viewFactory is null.</exception>
        /// <exception cref="ArgumentException">Thrown when viewFactory is already registered.</exception>
        public void RegisterViewFactory(Func<object> viewFactory)
        {
            if (viewFactory == null)
                throw new ArgumentNullException(nameof(viewFactory));

            if (_viewFactories.Contains(viewFactory))
                throw new ArgumentException("View factory is already registered with region '{Name}'.");

            _viewFactories.Add(viewFactory);
        }

        /// <summary>
        /// Removes a view type from the region metadata.
        /// </summary>
        /// <param name="viewType">The type of the view to remove.</param>
        /// <returns>true if the view type was removed; otherwise, false.</returns>
        internal bool RemoveViewType(Type viewType)
        {
            if (viewType == null)
                return false;

            return _viewTypes.Remove(viewType);
        }

        /// <summary>
        /// Removes a view factory from the region metadata.
        /// </summary>
        /// <param name="viewFactory">The factory to remove.</param>
        /// <returns>true if the view factory was removed; otherwise, false.</returns>
        internal bool RemoveViewFactory(Func<object> viewFactory)
        {
            if (viewFactory == null)
                return false;

            return _viewFactories.Remove(viewFactory);
        }

        /// <summary>
        /// Clears all registered view types and factories.
        /// </summary>
        internal void Clear()
        {
            _viewTypes.Clear();
            _viewFactories.Clear();
        }
    }
}