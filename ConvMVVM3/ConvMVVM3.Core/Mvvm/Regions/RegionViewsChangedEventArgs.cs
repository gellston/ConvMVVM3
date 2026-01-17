using System;
using System.Collections.Generic;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public sealed class RegionViewsChangedEventArgs : EventArgs
    {
        public RegionViewsChangedEventArgs(IReadOnlyList<object> added, IReadOnlyList<object> removed)
        {
            Added = added;
            Removed = removed;
        }

        public IReadOnlyList<object> Added { get; private set; }
        public IReadOnlyList<object> Removed { get; private set; }
    }
}
