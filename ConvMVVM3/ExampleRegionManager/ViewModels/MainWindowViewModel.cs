using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ExampleRegionManager.Model;
using ExampleRegionManager.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExampleRegionManager.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {

        #region Private Property
        private readonly bool _isSwitched = false;
        private readonly IRegionManager regionManager;
        #endregion

        #region Constructor
        public MainWindowViewModel(IRegionManager regionManager) { 
        
            this.regionManager = regionManager;
        }
        #endregion


        #region Command
        [RelayCommand]
        public void SwitchContent()
        {
            try
            {
                this.regionManager.RequestNavigate<TestModel>("ContentView");
            }
            catch (Exception ex)
            {
            }

        }

        [RelayCommand]
        public void AddItems()
        {
            try
            {
                this.regionManager.RequestNavigate("SubView1", "ExampleApp/SubView/1", 5);
            }
            catch (Exception ex)
            {

            }
        }

        [RelayCommand]
        public void AddList()
        {
            try
            {
                this.regionManager.RequestNavigate("SubView2", "ExampleApp/SubView/2", 5);
                this.regionManager.RequestNavigate("SubView2", "TestModel", 5);
            }
            catch (Exception ex)
            {
            }
        }

        [RelayCommand]
        public void SelectList()
        {
            try
            {
                var region = this.regionManager.Regions["SubView2"];
                this.regionManager.Active("SubView2", 3);
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
    }
}
