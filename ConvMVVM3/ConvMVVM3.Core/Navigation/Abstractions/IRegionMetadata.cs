using System;
using System.Collections.Generic;

namespace ConvMVVM3.Core.Navigation.Abstractions
{
    /// <summary>
    /// Defines the metadata for a region.
    /// </summary>
    public interface IRegionMetadata
    {
        /// <summary>
        /// Gets the name of the region.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the types of views registered with the region.
        /// </summary>
        IEnumerable<Type> ViewTypes { get; }

        /// <summary>
        /// Gets the view factories registered with the region.
        /// </summary>
        IEnumerable<Func<object>> ViewFactories { get; }
    }
}