using ConvMVVM3.Core.Navigation.Abstractions;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ConvMVVM3.WPF
{
    /// <summary>
    /// Region adapter for Selector-based controls (ListBox, ComboBox, TabControl, etc.).
    /// Supports SelectedItem binding with memory leak prevention.
    /// </summary>
    public class SelectorRegionAdapter : IRegionAdapter
    {
        /// <summary>
        /// Initializes the region with the target selector control.
        /// </summary>
        /// <param name="region">The region to initialize.</param>
        /// <param name="target">The target selector control.</param>
        public void Initialize(IRegion region, DependencyObject target)
        {
            if (!(target is Selector selector))
                return;

            // Setup ItemsSource binding
            SetupItemsSourceBinding(region, selector);

            // Setup SelectedItem synchronization
            SetupSelectedItemSynchronization(region, selector);
        }

        /// <summary>
        /// Sets up ItemsSource binding between region views and selector items.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="selector">The selector control.</param>
        private void SetupItemsSourceBinding(IRegion region, Selector selector)
        {
            // Bind ItemsSource to region Views
            var binding = new System.Windows.Data.Binding("Views")
            {
                Source = region,
                Mode = System.Windows.Data.BindingMode.OneWay
            };

            var bindingExpression = selector.SetBinding(ItemsControl.ItemsSourceProperty, binding);
            BindingTracker.TrackBinding(selector, bindingExpression);

            // Setup cleanup
            if (selector is FrameworkElement fe)
            {
                fe.Unloaded += (s, e) =>
                {
                    BindingTracker.ClearAllBindings(selector);
                };
            }
        }

        /// <summary>
        /// Sets up SelectedItem synchronization between region and selector.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="selector">The selector control.</param>
        private void SetupSelectedItemSynchronization(IRegion region, Selector selector)
        {
            var weakRegion = new WeakReference(region);
            var weakSelector = new WeakReference(selector);

            // Region ActiveView changed -> update SelectedItem
            region.ViewActivated += (s, e) =>
            {
                if (weakSelector.IsAlive && weakRegion.IsAlive)
                {
                    var targetSelector = (Selector)weakSelector.Target;
                    targetSelector.SelectedItem = e.View;
                }
            };

            // SelectedItem changed -> update Region ActiveView
            selector.SelectionChanged += (s, e) =>
            {
                if (weakRegion.IsAlive && weakSelector.IsAlive)
                {
                    var targetRegion = (IRegion)weakRegion.Target;
                    var targetSelector = (Selector)weakSelector.Target;

                    if (targetSelector.SelectedItem != null)
                    {
                        targetRegion.Activate(targetSelector.SelectedItem);
                    }
                }
            };
        }
    }
}