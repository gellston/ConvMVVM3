using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.Host.DependencyInjection;
using ConvMVVM3.WPF.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ConvMVVM3.WPF.Regions
{
    public static class RegionPlugin
    {

        #region Attached Property
        public static DependencyProperty RegionNameProperty = DependencyProperty.RegisterAttached("RegionName", typeof(string), typeof(RegionPlugin), new PropertyMetadata(OnRegionNameChanged));
        public static string GetRegionName(DependencyObject obj)
            => (string)obj.GetValue(RegionNameProperty);

        public static void SetRegionName(DependencyObject obj, string value)
            => obj.SetValue(RegionNameProperty, value);
        #endregion

        #region Event Handler
        public static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {

            if (!(d is FrameworkElement fe))
                return;

            var name = e.NewValue as string;
            if (string.IsNullOrEmpty(name)) return;

            var regionManager = ServiceLocator.GetService<IRegionManager>();
            var behaivorCollection = Interaction.GetBehaviors(d);

            if (d is ContentControl content)
            {
                regionManager.RegisterViewWithRegion(name, RegionType.SingleView);
                var region = regionManager.Regions[name];
                var contentControlBehaivor = new ContentControlBehavior();
                contentControlBehaivor.CurrentRegion = (Region)region;
                contentControlBehaivor.CurrentRegion.IsAttaced = true;
                behaivorCollection.Add(contentControlBehaivor);
                return;
            }

            if (d is ItemsControlBehavior itemsControl)
            {
                regionManager.RegisterViewWithRegion(name, RegionType.MultiView);
                var region = regionManager.Regions[name];
                var contentControlBehaivor = new ItemsControlBehavior();
                contentControlBehaivor.CurrentRegion = (Region)region;
                contentControlBehaivor.CurrentRegion.IsAttaced = true;
                behaivorCollection.Add(contentControlBehaivor);
                return;
            }

            if (d is Selector selector)
            {
                regionManager.RegisterViewWithRegion(name, RegionType.MultiView);
                var region = regionManager.Regions[name];
                var contentControlBehaivor = new SelectorBehaivor();
                contentControlBehaivor.CurrentRegion = (Region)region;
                contentControlBehaivor.CurrentRegion.IsAttaced = true;
                behaivorCollection.Add(contentControlBehaivor);
                return;
            }
        }
        #endregion
    }
}
