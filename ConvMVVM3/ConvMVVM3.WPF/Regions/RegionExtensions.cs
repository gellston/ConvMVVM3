using System;
using System.Windows;
using System.Windows.Threading;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.Host.DependencyInjection;
using ConvMVVM3.WPF.Abstractions;

namespace ConvMVVM3.WPF.Regions
{
    public static class RegionExtensions
    {
        public static readonly DependencyProperty RegionNameProperty =
            DependencyProperty.RegisterAttached(
                "RegionName",
                typeof(string),
                typeof(RegionExtensions),
                new FrameworkPropertyMetadata(null, OnRegionNameChanged));

        public static void SetRegionName(DependencyObject element, string value)
        {
            element.SetValue(RegionNameProperty, value);
        }

        public static string GetRegionName(DependencyObject element)
        {
            return (string)element.GetValue(RegionNameProperty);
        }

        private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = d as FrameworkElement;
            if (fe == null) return;

            var regionName = e.NewValue as string;
            if (string.IsNullOrWhiteSpace(regionName))
                return;

            if (!fe.IsLoaded)
            {
                RoutedEventHandler loaded = null;
                loaded = delegate (object s, RoutedEventArgs args)
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
            if (string.IsNullOrWhiteSpace(regionName)) return;

            fe.Dispatcher.BeginInvoke((Action)delegate
            {
                var regionManager = ResolveRequired<IRegionManager>();
                var adapterManager = ResolveRequired<RegionAdapterManager>();

                var region = regionManager.GetOrCreateRegion(regionName);

                var adapter = adapterManager.GetAdapter(fe);

                // 어댑터가 중복 attach 방지 + Unloaded 정리를 내부에서 책임진다
                adapter.Attach(fe, region);

            }, DispatcherPriority.Loaded);
        }

        private static T ResolveRequired<T>() where T : class
        {
            var sp = ServiceLocator.Container;
            if (sp == null)
                throw new InvalidOperationException("ServiceLocator.Container is null. IoC container is not initialized.");

            var obj = sp.GetService(typeof(T));
            var typed = obj as T;
            if (typed == null)
                throw new InvalidOperationException("Required service is not registered in IoC: " + typeof(T).FullName);

            return typed;
        }
    }
}