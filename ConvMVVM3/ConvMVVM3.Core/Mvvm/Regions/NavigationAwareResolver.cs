using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using System.Collections.Generic;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    /// <summary>Default participant lookup for objects that implement INavigationAware directly.</summary>
    public sealed class NavigationAwareResolver : INavigationAwareResolver
    {
        public IEnumerable<INavigationAware> Resolve(object view)
        {
            var aware = view as INavigationAware;
            return aware == null ? new INavigationAware[0] : new[] { aware };
        }
    }
}
