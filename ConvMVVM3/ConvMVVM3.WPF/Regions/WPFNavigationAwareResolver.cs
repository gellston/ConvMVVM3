using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.WPF.Locator;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ConvMVVM3.WPF.Regions
{
    /// <summary>Adapts WPF views and their attached-property-wired DataContexts to Core navigation.</summary>
    public sealed class WPFNavigationAwareResolver : INavigationAwareResolver
    {
        public IEnumerable<INavigationAware> Resolve(object view)
        {
            var participants = new List<INavigationAware>();
            var element = view as FrameworkElement;
            if (element != null)
            {
                element.VerifyAccess();
                if (ViewModelLocator.GetAutoWireViewModel(element) &&
                    !ViewModelLocator.EnsureViewModelConnected(element))
                {
                    throw new InvalidOperationException("AutoWireViewModel could not connect a DataContext for "
                        + element.GetType().FullName + ". Check ViewModel mapping, registration, and local DataContext values.");
                }
            }

            var viewAware = view as INavigationAware;
            if (viewAware != null) participants.Add(viewAware);

            var viewModelAware = element == null ? null : element.DataContext as INavigationAware;
            if (viewModelAware != null && !ReferenceEquals(viewAware, viewModelAware))
                participants.Add(viewModelAware);

            return participants;
        }
    }
}
