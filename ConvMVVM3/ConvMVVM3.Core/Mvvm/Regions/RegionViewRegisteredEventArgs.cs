using System;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public sealed class RegionViewRegisteredEventArgs : EventArgs
    {
        public RegionViewRegisteredEventArgs(string regionName, Type viewType)
        {
            RegionName = regionName;
            ViewType = viewType;
        }

        public string RegionName { get; private set; }
        public Type ViewType { get; private set; }
    }
}
