using ConvMVVM3.Core.Mvvm.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Attributes
{
    public enum InitializationMode
    {
        WhenAvailable,
        OnDemand
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ModuleAttribute : Attribute
    {
        #region Public Proprty
        public string Name { get; private set; }

        public string Version { get; private set; } = "1.0.0";

        // Attribute는 enum/primitive/string 가능
        public InitializationMode Mode { get; set; } = InitializationMode.WhenAvailable;


        // 배열도 가능 (상수 string 배열)
        public string[] DependsOn { get; set; } = new string[0];

        #endregion

        #region Constructor

        public ModuleAttribute(string name, string version = "1.0.0", InitializationMode mode = InitializationMode.WhenAvailable)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException($"Name {name}");
            if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException($"Version {version}");
            Name = name;
            Version = version;
            Mode = mode;
        }
        #endregion
    }
}
