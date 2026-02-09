using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ExampleActivatorUtilities.Models
{
    public partial class TestModel : ObservableObject
    {
        #region Constructor
        public TestModel(string name, int age)
        {

            this.Name = name;
            this.Age = age;
        }
        #endregion

        #region Pulbic Property
        [ObservableProperty]
        private string _Name = "";

        [ObservableProperty]
        private int _Age = 0;
        #endregion
    }
}
