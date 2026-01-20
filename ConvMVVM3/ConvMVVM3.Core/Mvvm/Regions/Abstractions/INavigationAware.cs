using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Regions.Abstractions
{
    public interface INavigationAware
    {
        void OnNavigatedTo(NavigationContext navigationContext);
        bool IsNavigationTarget(NavigationContext navigationContext);
        void OnNavigatedFrom(NavigationContext navigationContext);
    }
}
