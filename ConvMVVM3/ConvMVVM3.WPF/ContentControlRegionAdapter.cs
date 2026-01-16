using ConvMVVM3.Core.Navigation.Abstractions;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace ConvMVVM3.WPF
{
    /// <summary>
    /// Region adapter for ContentControl-based controls.
    /// Supports single view display with memory leak prevention.
    /// </summary>
    public class ContentControlRegionAdapter : IRegionAdapter
    {
        /// <summary>
        /// Initializes the region with the target content control.
        /// </summary>
        /// <param name="region">The region to initialize.</param>
        /// <param name="target">The target content control.</param>
        public void Initialize(IRegion region, DependencyObject target)
        {
            if (!(target is ContentControl contentControl))
                return;

            // Setup Content binding
            SetupContentBinding(region, contentControl);
        }

        /// <summary>
        /// Sets up Content binding between region ActiveView and content control.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="contentControl">The content control.</param>
        private void SetupContentBinding(IRegion region, ContentControl contentControl)
        {
            var weakRegion = new WeakReference(region);
            var weakContentControl = new WeakReference(contentControl);

            // Region Views changed -> update Content
            region.ViewActivated += (s, e) =>
            {
                if (weakContentControl.IsAlive)
                {
                    var targetControl = (ContentControl)weakContentControl.Target;
                    targetControl.Content = e.View;
                }
            };

            // Initial content setting
            if (region.ActiveView != null && weakContentControl.IsAlive)
            {
                var targetControl = (ContentControl)weakContentControl.Target;
                targetControl.Content = region.ActiveView;
            }

            // Setup cleanup
            if (contentControl is FrameworkElement fe)
            {
                fe.Unloaded += (s, e) =>
                {
                    // No specific cleanup needed for ContentControl binding
                    // since we're not using Binding objects
                };
            }
        }
    }
}