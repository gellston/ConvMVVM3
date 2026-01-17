using System;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public interface IViewFactory
    {
        object Create(Type viewType);
    }
}
