
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.Host.DependencyInjection;
using ConvMVVM3.WPF.Behaviors.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace ConvMVVM3.WPF.Regions
{
    public class ItemsControlBehavior : Behavior<ItemsControl>
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

        #region Public Property
        public Region CurrentRegion
        {
            get; set;
        }
        #endregion

        #region Event Handler
        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.AssociatedObject.Loaded -= AssociatedObject_Loaded;
            if (this.CurrentRegion == null) return;

            var viewsBinding = new Binding("Views")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Source = this.CurrentRegion
            };

            BindingOperations.SetBinding(this.AssociatedObject, ItemsControl.ItemsSourceProperty, viewsBinding);

        }

        private void AssociatedObject_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            if (this.CurrentRegion == null) return;


            BindingOperations.ClearBinding(this.AssociatedObject, ItemsControl.ItemsSourceProperty);


            this.CurrentRegion = null;
        }
        #endregion
    }
}
