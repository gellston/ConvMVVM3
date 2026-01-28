using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Modules.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Modules
{
    public class ModuleManager : IModuleManager
    {
        #region Private Property
        private readonly List<IModule> modules = new List<IModule>();
        private readonly List<ModuleCategory> categories = new List<ModuleCategory>();
        private readonly IServiceResolver serviceResolver;
        #endregion


        #region Constructor
        public ModuleManager(IServiceResolver serviceResolver, 
                             List<IModule> modules,
                             List<ModuleCategory> categories)
        {
            this.serviceResolver = serviceResolver;
            this.modules = modules;
            this.categories = categories;
        }
        #endregion


        #region Public Property
        public IList<ModuleCategory> Categories => categories;
        #endregion


        #region Public Functions
        public void LoadModule(string name)
        {
            try
            {
                var regionManager = this.serviceResolver.GetService<IRegionManager>();
                foreach (var moduleCategory in this.categories)
                {
                    if (moduleCategory.Name != name) continue;
                    if (moduleCategory.Mode != Attributes.InitializationMode.OnDemand) continue;
                    if (moduleCategory.Loaded == true) continue;

                    foreach (var module in this.modules)
                    {

                        var type = module.GetType();
                        var moduleAttribute = type.GetCustomAttribute<ModuleAttribute>(inherit: true);
                        if (moduleAttribute == null) continue;
                        if (moduleCategory.Name != name) continue;
                        module.ConfigureRegions(regionManager);
                        module.OnInitialized(this.serviceResolver);
                        moduleCategory.Loaded = true;
                       
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        

       
        public void InitializeModules()
        {
            try
            {
                foreach(var moduleCategory in this.categories)
                {
                    if (moduleCategory.Mode != Attributes.InitializationMode.WhenAvailable) continue;
                    if (moduleCategory.Loaded == true) continue;

                    foreach (var module in this.modules)
                    {
                        var type = module.GetType();
                        var moduleAttribute = type.GetCustomAttribute<ModuleAttribute>(inherit: true);
                        if (moduleAttribute == null) continue;
                        if (moduleCategory.Name != moduleAttribute.Name) continue;
                        module.OnInitialized(this.serviceResolver);
                        moduleCategory.Loaded = true;
                    }
                }

            }
            catch
            {
                throw;
            }
        }


        public void ConfigureRegions()
        {
            try
            {
                var regionManager = this.serviceResolver.GetService<IRegionManager>();
                foreach (var moduleCategory in this.categories)
                {
                    if (moduleCategory.Mode != Attributes.InitializationMode.WhenAvailable) continue;
                    if (moduleCategory.Loaded == true) continue;

                    foreach (var module in this.modules)
                    {
                        var type = module.GetType();
                        var moduleAttribute = type.GetCustomAttribute<ModuleAttribute>(inherit: true);
                        if (moduleAttribute == null) continue;
                        if (moduleCategory.Name != moduleAttribute.Name) continue;
                        module.ConfigureRegions(regionManager);
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        #endregion
    }
}
