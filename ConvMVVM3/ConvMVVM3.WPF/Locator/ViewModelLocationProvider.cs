using ConvMVVM3.Core.DependencyInjection.Abstractions;
using System;
using System.Collections.Generic;

namespace ConvMVVM3.WPF.Locator
{
    /// <summary>
    /// Prism-like ViewModel locator registry.
    /// - You can register explicit mappings (View -> ViewModel)
    /// - Or rely on a default convention resolver
    /// - Or register factories for special cases
    ///
    /// Thread-safe.
    /// </summary>
    public static class ViewModelLocationProvider
    {
        private static readonly object Gate = new object();

        private static readonly Dictionary<Type, Type> ViewToViewModel = new Dictionary<Type, Type>();
        private static readonly Dictionary<Type, Func<IServiceResolver, object>> ViewToFactory = new Dictionary<Type, Func<IServiceResolver, object>>();

        public static void Register(Type viewType, Type viewModelType)
        {
            if (viewType == null) throw new ArgumentNullException(nameof(viewType));
            if (viewModelType == null) throw new ArgumentNullException(nameof(viewModelType));

            lock (Gate)
            {
                ViewToViewModel[viewType] = viewModelType;
            }
        }

        public static void RegisterFactory(Type viewType, Func<IServiceResolver, object> factory)
        {
            if (viewType == null) throw new ArgumentNullException(nameof(viewType));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            lock (Gate)
            {
                ViewToFactory[viewType] = factory;
            }
        }

        public static Type ResolveViewModelType(Type viewType)
        {
            return ResolveViewModelType(viewType, ViewModelNamespaceConvention.Default, "Views", "ViewModels");
        }

        public static Type ResolveViewModelType(Type viewType, ViewModelNamespaceConvention convention, string replaceFrom, string replaceTo)
        {
            if (viewType == null) throw new ArgumentNullException(nameof(viewType));

            // 1) Attribute on the view wins
            var attr = (ViewModelForAttribute)Attribute.GetCustomAttribute(viewType, typeof(ViewModelForAttribute));
            if (attr != null)
                return attr.ViewModelType;

            lock (Gate)
            {
                // 2) Explicit mapping
                Type mapped;
                if (ViewToViewModel.TryGetValue(viewType, out mapped))
                    return mapped;

                // 3) Convention
                return ConventionResolver(viewType, convention, replaceFrom, replaceTo);
            }
        }

        public static bool TryResolveFactory(Type viewType, out Func<IServiceResolver, object> factory)
        {
            lock (Gate)
            {
                return ViewToFactory.TryGetValue(viewType, out factory);
            }
        }

        private static Type ConventionResolver(Type viewType, ViewModelNamespaceConvention convention, string replaceFrom, string replaceTo)
        {
            // Convention tries:
            //  - Namespace replace (Views -> ViewModels / ViewModel / custom)
            //  - Name suffix: FooView -> FooViewModel, or Foo -> FooViewModel
            var asm = viewType.Assembly;

            string ns = viewType.Namespace ?? string.Empty;
            string name = viewType.Name;

            string vmName;
            if (name.EndsWith("View", StringComparison.Ordinal))
                vmName = name + "Model";          // FooView -> FooViewModel
            else
                vmName = name + "ViewModel";      // Foo -> FooViewModel

            string ns2 = ns;

            if (convention == ViewModelNamespaceConvention.Default || convention == ViewModelNamespaceConvention.ViewsToViewModels)
            {
                ns2 = ReplaceNamespaceSegment(ns2, "Views", "ViewModels");
            }
            else if (convention == ViewModelNamespaceConvention.ViewsToViewModel)
            {
                ns2 = ReplaceNamespaceSegment(ns2, "Views", "ViewModel");
            }
            else if (convention == ViewModelNamespaceConvention.Custom)
            {
                ns2 = ReplaceNamespaceSegment(ns2, replaceFrom, replaceTo);
            }

            string full = string.IsNullOrEmpty(ns2) ? vmName : (ns2 + "." + vmName);

            var vmType = asm.GetType(full);
            return vmType; // may be null
        }

        private static string ReplaceNamespaceSegment(string ns, string from, string to)
        {
            if (string.IsNullOrEmpty(ns)) return ns;
            if (string.IsNullOrEmpty(from)) return ns;
            if (to == null) to = string.Empty;

            // Replace middle segments: ".Views." -> ".ViewModels."
            ns = ns.Replace("." + from + ".", "." + to + ".");

            // Replace suffix segment: "... .Views" -> "... .ViewModels"
            string suffix = "." + from;
            if (ns.EndsWith(suffix, StringComparison.Ordinal))
            {
                ns = ns.Substring(0, ns.Length - from.Length) + to;
            }

            // Replace entire namespace == from
            if (ns == from)
                ns = to;

            return ns;
        }
    }
}
