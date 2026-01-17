using System;
using System.Windows;
using ConvMVVM3.Core.Mvvm.Regions;

namespace ConvMVVM3.WPF.Abstractions
{
    public interface IRegionAdapter
    {
        Type TargetType { get; }

        IRegionAttachment Attach(DependencyObject target, IRegion region);
    }
}
