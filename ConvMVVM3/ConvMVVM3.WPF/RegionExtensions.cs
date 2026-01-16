using ConvMVVM3.Core.Navigation.Abstractions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ConvMVVM3.WPF
{
    /// <summary>
    /// Extension methods for IRegion to support WPF control associations.
    /// </summary>
    public static class RegionExtensions
    {
        private static readonly ConditionalWeakTable<IRegion, List<WeakReference>>
            _associatedControls = new ConditionalWeakTable<IRegion, List<WeakReference>>();

        /// <summary>
        /// Associates a control with the region using weak references.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="control">The control to associate.</param>
        public static void AssociateWithControl(this IRegion region, object control)
        {
            if (!_associatedControls.TryGetValue(region, out var controls))
            {
                controls = new List<WeakReference>();
                _associatedControls.Add(region, controls);
            }

            controls.Add(new WeakReference(control));
        }

        /// <summary>
        /// Removes the association between a control and the region.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="control">The control to disassociate.</param>
        public static void RemoveControlAssociation(this IRegion region, object control)
        {
            if (_associatedControls.TryGetValue(region, out var controls))
            {
                controls.RemoveAll(wr => wr.Target == null || wr.Target == control);
            }
        }

        /// <summary>
        /// Cleans up dead control references.
        /// </summary>
        /// <param name="region">The region.</param>
        public static void CleanupDeadControlReferences(this IRegion region)
        {
            if (_associatedControls.TryGetValue(region, out var controls))
            {
                controls.RemoveAll(wr => !wr.IsAlive);
            }
        }
    }
}