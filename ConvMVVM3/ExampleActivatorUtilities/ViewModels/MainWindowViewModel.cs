using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Host.DependencyInjection;
using ExampleActivatorUtilities.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ExampleActivatorUtilities.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {

        #region Private Property
        private readonly IServiceResolver serviceResolver;
        #endregion

        #region Constructor
        public MainWindowViewModel(IServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver;

        }
        #endregion

        #region Collection
        [ObservableProperty]
        private ObservableCollection<TestModel> _TestModelCollection = new ObservableCollection<TestModel>();
        #endregion

        #region Command
        [AsyncRelayCommand]
        public async Task GenerateModel()
        {

            try
            {
                var model = ActivatorUtilities.CreateInstance<TestModel>(this.serviceResolver, "Test", 25);
                this.TestModelCollection.Add(model);

            }catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
        #endregion
    }
}
