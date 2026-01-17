using System;
using System.Windows;
using ConvMVVM3.Core.Mvvm.Regions;

namespace ConvMVVM3.WPF.Regions
{
    internal static class NavigationAwareDispatcher
    {
        public static void DispatchNavigatedFrom(object viewOrVm, INavigationContext ctx)
        {
            var aware = viewOrVm as INavigationContextAware;
            if (aware != null)
                aware.OnNavigatedFrom(ctx);
        }

        public static void DispatchNavigatedTo(object viewOrVm, INavigationContext ctx)
        {
            var aware = viewOrVm as INavigationContextAware;
            if (aware != null)
                aware.OnNavigatedTo(ctx);
        }

        /// <summary>
        /// Dispatch navigation to the View and its DataContext (VM).
        /// Supports ViewModelLocator scenarios where DataContext arrives later.
        ///
        /// NOTE:
        /// FrameworkElement.DataContextChanged uses DependencyPropertyChangedEventArgs (a struct),
        /// so WeakEventManager<TSource, TEventArgs> cannot be used because it requires TEventArgs : EventArgs.
        ///
        /// This subscription is safe because it's a self-event on the view (source == fe),
        /// and we detach immediately after the first DataContext assignment (and also via returned IDisposable).
        /// </summary>
        public static IDisposable DispatchToViewAndViewModel(object view, INavigationContext ctx)
        {
            if (view == null) return Disposable.Empty;

            DispatchNavigatedTo(view, ctx);

            var fe = view as FrameworkElement;
            if (fe == null)
                return Disposable.Empty;

            if (fe.DataContext != null)
            {
                DispatchNavigatedTo(fe.DataContext, ctx);
                return Disposable.Empty;
            }

            DependencyPropertyChangedEventHandler handler = null;
            handler = delegate(object s, DependencyPropertyChangedEventArgs e)
            {
                fe.DataContextChanged -= handler;

                if (fe.DataContext != null)
                    DispatchNavigatedTo(fe.DataContext, ctx);
            };

            fe.DataContextChanged += handler;

            return new Disposable(delegate
            {
                fe.DataContextChanged -= handler;
            });
        }

        internal sealed class Disposable : IDisposable
        {
            public static readonly IDisposable Empty = new Disposable(null);

            private Action _dispose;

            public Disposable(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                var d = _dispose;
                _dispose = null;
                if (d != null) d();
            }
        }
    }
}
