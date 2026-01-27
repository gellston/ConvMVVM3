using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExampleModuleManager.ViewModels
{


    public partial class MainWindowViewModel : ObservableObject
    {
        #region Constructor
        public MainWindowViewModel()
        {

        }
        #endregion

        #region Public Property

        #endregion

        #region Command
        [RelayCommand]
        private void LoadModule()
        {

        }


        [RelayCommand]
        private void RunModule()
        {

        }
        #endregion
    }
}
