using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Modules.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExampleModuleManager.ViewModels
{
    public partial class MainContentViewModel : ObservableObject
    {
        #region Private Property
        private readonly IRegionManager regionManager;
        private readonly IModuleManager moduleManager;
        #endregion

        #region Constructor
        public MainContentViewModel(IRegionManager regionManager,
                                    IModuleManager moduleManager) { 
        
            this.regionManager = regionManager;
            this.moduleManager = moduleManager;
        }
        #endregion

        #region Command
        [RelayCommand]
        private void AModule()
        {
            try
            {
                this.regionManager.RequestNavigate("SubContent", "/SubContent/AModuleView");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }


        [RelayCommand]
        private void BModule()
        {
            try
            {
                this.regionManager.RequestNavigate("SubContent", "/SubContent/BModuleView");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }


        [RelayCommand]
        private void CModuleLoad()
        {
            try
            {
                this.moduleManager.InitializeModule("CModule");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        #endregion
    }
}
