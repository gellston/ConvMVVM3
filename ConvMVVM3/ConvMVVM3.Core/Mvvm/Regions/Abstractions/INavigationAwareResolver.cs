using System.Collections.Generic;

namespace ConvMVVM3.Core.Mvvm.Regions.Abstractions
{
    /// <summary>
    /// Finds navigation participants without introducing UI framework dependencies into Core.
    /// Implementations return a snapshot, in notification order, without duplicate instances.
    /// </summary>
    public interface INavigationAwareResolver
    {
        IEnumerable<INavigationAware> Resolve(object view);
    }
}
