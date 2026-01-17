using ConvMVVM3.Core.Navigation.Abstractions;
using ConvMVVM3.Host.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace ConvMVVM3.WPF
{
    /// <summary>
    /// Provides attached properties for region management in WPF applications.
    /// Handles automatic region creation and binding with memory leak prevention.
    /// </summary>
    public static class RegionManagerExtensions
    {
        /// <summary>
        /// Identifies the RegionName attached property.
        /// </summary>
        public static readonly DependencyProperty RegionNameProperty =
            DependencyProperty.RegisterAttached("RegionName", typeof(string), typeof(RegionManager),
                new PropertyMetadata(null, OnRegionNameChanged));

        /// <summary>
        /// Gets the region name for the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The region name.</returns>
        public static string GetRegionName(DependencyObject element)
        {
            return (string)element.GetValue(RegionNameProperty);
        }

        /// <summary>
        /// Sets the region name for the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="value">The region name.</param>
        public static void SetRegionName(DependencyObject element, string value)
        {
            element.SetValue(RegionNameProperty, value);
        }

        /// <summary>
        /// Called when the RegionName property changes.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var regionName = (string)e.NewValue;
            if (string.IsNullOrEmpty(regionName)) return;

            try
            {
                // Get region manager from service locator
                var regionManager = ServiceLocator.GetService<IRegionManager>();
                if (regionManager == null) return;

                // Get or create region
                var region = regionManager[regionName];
                if (region == null)
                {
                    region = regionManager.CreateRegion(regionName);
                    regionManager.AddRegion(regionName, region);
                }

                // Setup memory-safe binding and region association
                SetupMemorySafeBinding(d, region);
                region.AssociateWithControl(d);
            }
            catch (Exception ex)
            {
                // Log error in production - for now, ignore to prevent crashes
                System.Diagnostics.Debug.WriteLine($"Region setup failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets up memory-safe binding between control and region.
        /// </summary>
        /// <param name="control">The control to bind.</param>
        /// <param name="region">The region to bind to.</param>
        private static void SetupMemorySafeBinding(DependencyObject control, IRegion region)
        {
            // Determine if control supports SelectedItem (Selector-based controls)
            var selectedItemProperty = GetSelectedItemProperty(control);

            if (selectedItemProperty != null)
            {
                // Setup TwoWay binding for SelectedItem
                SetupSelectedItemBinding(control, region, selectedItemProperty);
            }
            else
            {
                // For ContentControl or other controls, setup content binding
                SetupContentBinding(control, region);
            }
        }

        /// <summary>
        /// Gets the SelectedItem dependency property for selector-based controls.
        /// </summary>
        /// <param name="control">The control to check.</param>
        /// <returns>The SelectedItem property, or null if not supported.</returns>
        private static DependencyProperty GetSelectedItemProperty(DependencyObject control)
        {
            if (control is Selector)
                return Selector.SelectedItemProperty;
            if (control is ListBox)
                return ListBox.SelectedItemProperty;
            if (control is ComboBox)
                return ComboBox.SelectedItemProperty;
            if (control is TabControl)
                return TabControl.SelectedItemProperty;

            return null;
        }

        /// <summary>
        /// Sets up TwoWay binding for SelectedItem.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="region">The region.</param>
        /// <param name="selectedItemProperty">The SelectedItem property.</param>
        private static void SetupSelectedItemBinding(DependencyObject control, IRegion region, DependencyProperty selectedItemProperty)
        {
            // Create TwoWay binding
            var binding = new Binding("ActiveView")
            {
                Source = region,
                Mode = BindingMode.TwoWay
            };

            // Set binding and track it (cast to FrameworkElement for SetBinding)
            if (control is FrameworkElement fe)
            {
                var bindingExpression = fe.SetBinding(selectedItemProperty, binding);
                BindingTracker.TrackBinding(fe, bindingExpression);
            }

            // Setup cleanup on unload
            if (control is FrameworkElement frameworkElement)
            {
                frameworkElement.Unloaded += (s, e) =>
                {
                    BindingTracker.ClearAllBindings(frameworkElement);
                    region.RemoveControlAssociation(frameworkElement);
                };
            }
        }

        /// <summary>
        /// Sets up content binding for non-selector controls.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="region">The region.</param>
        private static void SetupContentBinding(DependencyObject control, IRegion region)
        {
            if (control is ContentControl contentControl)
            {
                // Bind Content to ActiveView
                var binding = new Binding("ActiveView")
                {
                    Source = region,
                    Mode = BindingMode.OneWay
                };

                var bindingExpression = contentControl.SetBinding(ContentControl.ContentProperty, binding);
                BindingTracker.TrackBinding(contentControl, bindingExpression);

                // Setup cleanup
                if (contentControl is FrameworkElement fe)
                {
                    fe.Unloaded += (s, e) =>
                    {
                        BindingTracker.ClearAllBindings(contentControl);
                        region.RemoveControlAssociation(contentControl);
                    };
                }
            }
        }

        /// <summary>
        /// Gets the region adapter for a control type.
        /// </summary>
        /// <param name="controlType">The control type.</param>
        /// <returns>The region adapter.</returns>
        private static IRegionAdapter GetRegionAdapter(Type controlType)
        {
            if (typeof(Selector).IsAssignableFrom(controlType) || typeof(ListBox).IsAssignableFrom(controlType) ||
                typeof(ComboBox).IsAssignableFrom(controlType) || typeof(TabControl).IsAssignableFrom(controlType))
            {
                return new SelectorRegionAdapter();
            }

            if (typeof(ContentControl).IsAssignableFrom(controlType))
            {
                return new ContentControlRegionAdapter();
            }

            // Default adapter
            return new ContentControlRegionAdapter();
        }
    }


}