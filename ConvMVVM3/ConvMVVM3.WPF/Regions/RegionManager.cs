using System;
using System.Windows;
using System.Windows.Threading;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.WPF.Abstractions;

namespace ConvMVVM3.WPF.Regions
{
    /// <summary>
    /// WPF attached properties to:
    /// - define region name on a control (RegionName)
    /// - provide a Core IRegionManager instance via property inheritance (RegionManager)
    ///
    /// Adapters are resolved from IoC (RegionServices.Services) if configured,
    /// otherwise a built-in fallback adapter manager is used.
    /// </summary>
    public static class RegionManager
    {
        private static readonly RegionAdapterManager _fallbackAdapterManager = CreateFallbackAdapterManager();

        public static readonly DependencyProperty RegionNameProperty =
            DependencyProperty.RegisterAttached(
                "RegionName",
                typeof(string),
                typeof(RegionManager),
                new FrameworkPropertyMetadata(null, OnRegionNameChanged));

        public static void SetRegionName(DependencyObject element, string value)
        {
            element.SetValue(RegionNameProperty, value);
        }

        public static string GetRegionName(DependencyObject element)
        {
            return (string)element.GetValue(RegionNameProperty);
        }

        public static readonly DependencyProperty RegionManagerProperty =
            DependencyProperty.RegisterAttached(
                "RegionManager",
                typeof(IRegionManager),
                typeof(RegionManager),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetRegionManager(DependencyObject element, IRegionManager value)
        {
            element.SetValue(RegionManagerProperty, value);
        }

        public static IRegionManager GetRegionManager(DependencyObject element)
        {
            return (IRegionManager)element.GetValue(RegionManagerProperty);
        }

        private static readonly DependencyProperty RegionAttachmentProperty =
            DependencyProperty.RegisterAttached(
                "RegionAttachment",
                typeof(IRegionAttachment),
                typeof(RegionManager),
                new PropertyMetadata(null));

        private static void SetRegionAttachment(DependencyObject element, IRegionAttachment value)
        {
            element.SetValue(RegionAttachmentProperty, value);
        }

        private static IRegionAttachment GetRegionAttachment(DependencyObject element)
        {
            return (IRegionAttachment)element.GetValue(RegionAttachmentProperty);
        }

        private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = d as FrameworkElement;
            if (fe == null)
                return;

            var oldAtt = GetRegionAttachment(fe);
            if (oldAtt != null) oldAtt.Dispose();
            SetRegionAttachment(fe, null);

            var regionName = e.NewValue as string;
            if (string.IsNullOrWhiteSpace(regionName))
                return;

            if (!fe.IsLoaded)
            {
                RoutedEventHandler loaded = null;
                loaded = delegate(object s, RoutedEventArgs args)
                {
                    fe.Loaded -= loaded;
                    InitializeRegion(fe);
                };
                fe.Loaded += loaded;
                return;
            }

            InitializeRegion(fe);
        }

        private static void InitializeRegion(FrameworkElement fe)
        {
            var regionName = GetRegionName(fe);
            if (string.IsNullOrWhiteSpace(regionName))
                return;

            var manager = GetRegionManager(fe);
            if (manager == null)
            {
                throw new InvalidOperationException(
                    "No IRegionManager was found for element '" + fe.GetType().Name + "' with RegionName='" + regionName + "'. " +
                    "Set ConvMVVM3.WPF.Regions.RegionManager.RegionManager on a parent element (e.g., Window)."
                );
            }

            fe.Dispatcher.BeginInvoke((Action)delegate
            {
                var oldAtt = GetRegionAttachment(fe);
                if (oldAtt != null) oldAtt.Dispose();
                SetRegionAttachment(fe, null);

                var region = manager.GetOrCreateRegion(regionName);

                var adapterManager = ResolveAdapterManagerFromIoc();
                if (adapterManager == null)
                    adapterManager = _fallbackAdapterManager;

                var adapter = adapterManager.GetAdapter(fe);

                var attachment = adapter.Attach(fe, region);
                SetRegionAttachment(fe, attachment);

                RoutedEventHandler unloaded = null;
                unloaded = delegate(object s, RoutedEventArgs args)
                {
                    fe.Unloaded -= unloaded;
                    var att = GetRegionAttachment(fe);
                    if (att != null) att.Dispose();
                    SetRegionAttachment(fe, null);
                };
                fe.Unloaded += unloaded;

            }, DispatcherPriority.Loaded);
        }

        private static RegionAdapterManager ResolveAdapterManagerFromIoc()
        {
            var sp = RegionServices.Services;
            if (sp == null) return null;

            var obj = sp.GetService(typeof(RegionAdapterManager));
            return obj as RegionAdapterManager;
        }

        private static RegionAdapterManager CreateFallbackAdapterManager()
        {
            var m = new RegionAdapterManager();
            m.RegisterAdapter(new SelectorRegionAdapter());
            m.RegisterAdapter(new ContentControlRegionAdapter());
            m.RegisterAdapter(new ItemsControlRegionAdapter());
            m.RegisterAdapter(new PanelRegionAdapter());
            return m;
        }
    }
}
