using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Host.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ConvMVVM3.WPF.Locator
{
    /// <summary>
    /// Prism-like ViewModelLocator.
    ///
    /// Usage:
    ///   regions:ViewModelLocator.AutoWireViewModel="True"
    ///
    /// Additional options:
    /// - Mode (Auto/Manual)
    /// - ViewModelType (manual override)
    /// - NamespaceConvention (Views -> ViewModels / ViewModel / Custom)
    /// - NamespaceReplaceFrom/NamespaceReplaceTo (for Custom)
    /// </summary>
    public static class ViewModelLocator
    {
        public static readonly DependencyProperty AutoWireViewModelProperty =
            DependencyProperty.RegisterAttached(
                "AutoWireViewModel",
                typeof(bool),
                typeof(ViewModelLocator),
                new PropertyMetadata(false, OnAutoWireChanged));

        public static void SetAutoWireViewModel(DependencyObject element, bool value)
        {
            element.SetValue(AutoWireViewModelProperty, value);
        }

        public static bool GetAutoWireViewModel(DependencyObject element)
        {
            return (bool)element.GetValue(AutoWireViewModelProperty);
        }

        /// <summary>
        /// Auto: use registry/convention
        /// Manual: use ViewModelType attached property only
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.RegisterAttached(
                "Mode",
                typeof(ViewModelLocatorMode),
                typeof(ViewModelLocator),
                new PropertyMetadata(ViewModelLocatorMode.Auto));

        public static void SetMode(DependencyObject element, ViewModelLocatorMode value)
        {
            element.SetValue(ModeProperty, value);
        }

        public static ViewModelLocatorMode GetMode(DependencyObject element)
        {
            return (ViewModelLocatorMode)element.GetValue(ModeProperty);
        }

        /// <summary>
        /// Manual override: specify ViewModel type in XAML (x:Type).
        /// </summary>
        public static readonly DependencyProperty ViewModelTypeProperty =
            DependencyProperty.RegisterAttached(
                "ViewModelType",
                typeof(Type),
                typeof(ViewModelLocator),
                new PropertyMetadata(null));

        public static void SetViewModelType(DependencyObject element, Type value)
        {
            element.SetValue(ViewModelTypeProperty, value);
        }

        public static Type GetViewModelType(DependencyObject element)
        {
            return (Type)element.GetValue(ViewModelTypeProperty);
        }

        public static readonly DependencyProperty NamespaceConventionProperty =
            DependencyProperty.RegisterAttached(
                "NamespaceConvention",
                typeof(ViewModelNamespaceConvention),
                typeof(ViewModelLocator),
                new PropertyMetadata(ViewModelNamespaceConvention.Default));

        public static void SetNamespaceConvention(DependencyObject element, ViewModelNamespaceConvention value)
        {
            element.SetValue(NamespaceConventionProperty, value);
        }

        public static ViewModelNamespaceConvention GetNamespaceConvention(DependencyObject element)
        {
            return (ViewModelNamespaceConvention)element.GetValue(NamespaceConventionProperty);
        }

        public static readonly DependencyProperty NamespaceReplaceFromProperty =
            DependencyProperty.RegisterAttached(
                "NamespaceReplaceFrom",
                typeof(string),
                typeof(ViewModelLocator),
                new PropertyMetadata("Views"));

        public static void SetNamespaceReplaceFrom(DependencyObject element, string value)
        {
            element.SetValue(NamespaceReplaceFromProperty, value);
        }

        public static string GetNamespaceReplaceFrom(DependencyObject element)
        {
            return (string)element.GetValue(NamespaceReplaceFromProperty);
        }

        public static readonly DependencyProperty NamespaceReplaceToProperty =
            DependencyProperty.RegisterAttached(
                "NamespaceReplaceTo",
                typeof(string),
                typeof(ViewModelLocator),
                new PropertyMetadata("ViewModels"));

        public static void SetNamespaceReplaceTo(DependencyObject element, string value)
        {
            element.SetValue(NamespaceReplaceToProperty, value);
        }

        public static string GetNamespaceReplaceTo(DependencyObject element)
        {
            return (string)element.GetValue(NamespaceReplaceToProperty);
        }

        private static void OnAutoWireChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = d as FrameworkElement;
            if (fe == null) return;

            if ((bool)e.NewValue)
            {
                if (fe.IsLoaded) Wire(fe);
                else
                {
                    RoutedEventHandler loaded = null;
                    loaded = delegate(object s, RoutedEventArgs args)
                    {
                        fe.Loaded -= loaded;
                        Wire(fe);
                    };
                    fe.Loaded += loaded;
                }
            }
        }

        private static void Wire(FrameworkElement fe)
        {
            if (fe.DataContext != null)
                return;

            var viewType = fe.GetType();

            // Manual mode: only use attached ViewModelType
            var mode = GetMode(fe);
            Type vmType = GetViewModelType(fe);

            if (mode == ViewModelLocatorMode.Manual)
            {
                if (vmType == null) return;
                fe.DataContext = Create(vmType);
                return;
            }

            // Auto mode:
            // 1) Explicit ViewModelType attached property wins (useful for per-view override)
            if (vmType != null)
            {
                fe.DataContext = Create(vmType);
                return;
            }

            // 2) Factory mapping wins if present
            Func<IServiceResolver, object> factory;
            if (ViewModelLocationProvider.TryResolveFactory(viewType, out factory) &&
                ServiceLocator.Container != null)
            {
                fe.DataContext = factory(ServiceLocator.Container);
                return;
            }

            // 3) Registry / convention
            var conv = GetNamespaceConvention(fe);
            var from = GetNamespaceReplaceFrom(fe);
            var to = GetNamespaceReplaceTo(fe);

            vmType = ViewModelLocationProvider.ResolveViewModelType(viewType, conv, from, to);
            if (vmType == null)
                return;

            fe.DataContext = Create(vmType);
        }

        private static object Create(Type vmType)
        {
            var sp = ServiceLocator.Container;
            if (sp != null)
            {
                var obj = sp.GetService(vmType);
                if (obj != null) return obj;
            }

            return Activator.CreateInstance(vmType);
        }

        
    }
}
