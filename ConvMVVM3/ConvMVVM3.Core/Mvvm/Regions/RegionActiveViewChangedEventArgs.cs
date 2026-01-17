using System;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public sealed class RegionActiveViewChangedEventArgs : EventArgs
    {
        public RegionActiveViewChangedEventArgs(object oldView, object newView, INavigationContext context)
        {
            OldView = oldView;
            NewView = newView;
            Context = context;
        }

        /// <summary>May be null.</summary>
        public object OldView { get; private set; }

        /// <summary>May be null.</summary>
        public object NewView { get; private set; }

        /// <summary>May be null.</summary>
        public INavigationContext Context { get; private set; }
    }
}
