using ConvMVVM3.Core.Mvvm.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Modules
{
    public enum ModuleState { Discovered, Registered, Initialized, Failed }
    public sealed class ModuleDescriptor
    {
        #region Public Property
        public string Name { get; private set; }
        public string AssemblyPath { get; private set; }          // null 가능
        public string ModuleTypeName { get; private set; }        // AssemblyQualifiedName 권장
        public InitializationMode Mode { get; private set; }
        public string[] DependsOn { get; private set; }           // never null
        public Version Version { get; private set; }              // AssemblyName.Version 등
        public ModuleState State { get; internal set; }
        #endregion


        #region Constructor

        public ModuleDescriptor(
            string name, string moduleTypeName,
            InitializationMode mode, string[] dependsOn,
            string assemblyPath, Version version)
        {
            Name = name;
            ModuleTypeName = moduleTypeName;
            Mode = mode;
            DependsOn = dependsOn ?? new string[0];
            AssemblyPath = assemblyPath;
            Version = version ?? new Version(0, 0, 0, 0);
            State = ModuleState.Discovered;
        }
        #endregion
    }
}
