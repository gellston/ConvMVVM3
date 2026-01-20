
using ConvMVVM3.WPF.Behaviors.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ConvMVVM3.WPF.Regions
{
    public class ContentControlBehavior : Behavior<ContentControl>
    {
        #region Protected Functions
        protected override void OnAttached()
        {
            base.OnAttached();


            this.AssociatedObject.Loaded -= AssociatedObject_Loaded;
            this.AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            this.AssociatedObject.Loaded += AssociatedObject_Loaded;
            this.AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }



        protected override void OnDetaching()
        {
            base.OnDetaching();


            this.AssociatedObject.Loaded -= AssociatedObject_Loaded;
            this.AssociatedObject.Unloaded -= AssociatedObject_Unloaded;

        }
        #endregion

        #region Event Handler
        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        private void AssociatedObject_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
        }
        #endregion
    }
}
