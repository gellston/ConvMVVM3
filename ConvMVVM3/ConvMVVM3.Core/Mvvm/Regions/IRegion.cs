using System;
using System.Collections.Generic;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public interface IRegion
    {
        string Name { get; }

        IReadOnlyList<object> Views { get; }

        /// <summary>May be null.</summary>
        object ActiveView { get; }

        /// <summary>Alias of ActiveView. Useful for Selector-based regions (ListBox/TabControl).</summary>
        object SelectedItem { get; set; }

        event EventHandler<RegionViewsChangedEventArgs> ViewsChanged;

        event EventHandler<RegionActiveViewChangedEventArgs> ActiveViewChanged;

        void Add(object view);

        bool Remove(object view);

        void Activate(object view, INavigationContext context = null);

        void Deactivate(INavigationContext context = null);
    }
}
