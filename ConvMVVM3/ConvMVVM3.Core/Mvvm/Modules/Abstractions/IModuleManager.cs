using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Modules.Abstractions
{
    public interface IModuleManager
    {

        #region Public Functions
        void InitializeModules();
        void LoadModule(string name);
        void ConfigureRegions();
        #endregion

        #region Public Property
        IList<ModuleCategory> Categories
        {
            get;
        }
        #endregion
    }
}
