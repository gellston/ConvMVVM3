
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
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace ConvMVVM3.WPF.Regions
{
    public class SelectorBehaivor : Behavior<Selector>
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
            if (this.CurrentRegion == null) return;

            var viewsBinding = new Binding("Views")
            {
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Source = this.CurrentRegion
            };

            var selectorBinding = new Binding("SelectedItem")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Source = this.CurrentRegion
            };

            BindingOperations.SetBinding(this.AssociatedObject, Selector.ItemsSourceProperty, viewsBinding);
            BindingOperations.SetBinding(this.AssociatedObject, Selector.SelectedItemProperty, selectorBinding);

        }

        private void AssociatedObject_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.CurrentRegion == null) return;


            BindingOperations.ClearBinding(this.AssociatedObject, Selector.ItemsSourceProperty);
            BindingOperations.ClearBinding(this.AssociatedObject, Selector.SelectedItemProperty);

            if (this.CurrentRegion != null)
            {
                this.CurrentRegion.IsAttaced = false;
                this.CurrentRegion.Content = null;
                this.CurrentRegion.Views.Clear();
            }


            this.CurrentRegion = null;
        }
        #endregion
    }
}
