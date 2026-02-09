using ConvMVVM3.Core.Mvvm.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Modules
{
    public sealed class ModuleCategory
    {
        #region Constructor
        public ModuleCategory(string name, string version, InitializationMode mode, string[] dependsOn)
        {
            this.Name = name;
            this.Version = version;
            this.Mode = mode;
            this.DependsOn = dependsOn;
        }
        #endregion

        #region Public Property
        public string Name { get; private set; }
        public string Version { get; private set; } = "1.0.0";
        public InitializationMode Mode { get; private set; } = InitializationMode.WhenAvailable;
        public string[] DependsOn { get; private set; } = new string[0];

        public bool IsInitialized { get; internal set; } = false;
        public bool IsRegistered { get; set; } = false;
        public bool IsRegionConfigured { get; internal set; } = false;
        #endregion
    }
}
