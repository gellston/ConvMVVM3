using ConvMVVM3.WPF.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ConvMVVM3.WPF.Regions
{
    public static class RegionExtensions
    {

        #region Attached Property
        public static DependencyProperty RegionNameProperty = DependencyProperty.RegisterAttached("RegionName", typeof(string), typeof(RegionExtensions), new PropertyMetadata(OnRegionNameChanged));
        public static string GetRegionName(DependencyObject obj)
            => (string)obj.GetValue(RegionNameProperty);

        public static void SetRegionName(DependencyObject obj, string value)
            => obj.SetValue(RegionNameProperty, value);
        #endregion

        #region Event Handler
        public static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {


            if (!(d is FrameworkElement fe))
                return;



            var behaivorCollection = Interaction.GetBehaviors(d);


            if 
            
        }
        #endregion
    }
}
