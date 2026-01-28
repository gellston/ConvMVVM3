using ConvMVVM3.Core;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Attributes;
using ConvMVVM3.Core.Mvvm.Modules;
using ConvMVVM3.Core.Mvvm.Modules.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.Host.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace ConvMVVM3.WPF
{
    public abstract class Bootstrapper
    {
        #region Private Property
        private readonly Core.DependencyInjection.Abstractions.IServiceRegistry serviceRegistry = new ServiceCollection();
        private Core.DependencyInjection.Abstractions.IServiceContainer serviceContainer = null;


        private readonly List<IModule> modules = new List<IModule>();
        private readonly List<ModuleCategory> categories = new List<ModuleCategory>();


        private bool enableAutoModuleSearch = false;
        private HashSet<string> moduleLoadPaths = new HashSet<string>();
        private HashSet<string> assemblyNames = new HashSet<string>();
        private HashSet<string> moduleRejectNames = new HashSet<string>();
        private HashSet<string> loadedAssemblyNames = new HashSet<string>();
        #endregion


        #region Private Functions
        private void LoadAssemblyStart()
        {
            try
            {
                foreach (var modulePath in this.moduleLoadPaths)
                {
                    var moduleFiles = Directory.GetFiles(modulePath, "*.dll");

                    foreach (var moduleFile in moduleFiles)
                    {

                        var assemblyName = Path.GetFileNameWithoutExtension(moduleFile);

                        if (this.assemblyNames.Count(name => name == assemblyName) == 0) continue;
                        if (this.loadedAssemblyNames.Count(name => name == assemblyName) > 0) continue;

                        try
                        {
                            var assembly = Assembly.LoadFrom(moduleFile);
                            var pluginTypes = assembly.GetTypes().Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                            foreach (var pluginType in pluginTypes)
                            {
                                var moduleAttribute = pluginType.GetCustomAttribute<ModuleAttribute>(inherit: true);
                                if (moduleAttribute == null) continue;

                                if (this.categories.Count(_category => _category.Name == moduleAttribute.Name) > 0) continue;
                                if (this.moduleRejectNames.Contains(moduleAttribute.Name) == true) continue;

                                var plugin = (IModule)Activator.CreateInstance(pluginType);
                                this.modules.Add(plugin);


                                var category = new ModuleCategory(moduleAttribute.Name,
                                                                  moduleAttribute.Version,
                                                                  moduleAttribute.Mode,
                                                                  moduleAttribute.DependsOn);

                                this.categories.Add(category);
                                //this.OnModuleAddEvent?.Invoke(plugin.ModuleVersion, plugin.ModuleName);
                            }

                            loadedAssemblyNames.Add(assemblyName);
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            {

            }
        }
        #endregion


        #region Protected Functions
        protected void RegisterModule<T>(T module) where T : IModule
        {


            var type = module.GetType();
            var moduleAttribute = type.GetCustomAttribute<ModuleAttribute>(inherit: true);
            if (moduleAttribute == null) throw new InvalidOperationException("No module attribute information");

            if (this.categories.Count(category => category.Name == moduleAttribute.Name) == 0)
                throw new InvalidOperationException("Duplicate module category information");

            var moduleCategory = new ModuleCategory(moduleAttribute.Name,
                                                    moduleAttribute.Version,
                                                    moduleAttribute.Mode,
                                                    moduleAttribute.DependsOn);

            this.categories.Add(moduleCategory);
            this.modules.Add(module);
        }

        protected void RegisterModule<T>() where T : IModule
        {
            var module = (IModule)Activator.CreateInstance(typeof(T));
            this.RegisterModule(module);
        }

        protected void AddAssemblyName(string assemblyName)
        {
            if (this.assemblyNames.Count(name => name == assemblyName) > 0) return;

            this.assemblyNames.Add(assemblyName);

        }

        protected void AddModulePath(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    throw new InvalidOperationException("Path is not exists");

                this.moduleLoadPaths.Add(path);
            }
            catch
            {
                throw;
            }
        }

        protected void AddModuleRelativePath(string folder)
        {
            try
            {
                var currentPath = AppDomain.CurrentDomain.BaseDirectory;
                var path = Path.Combine(currentPath, folder);

                if (!Directory.Exists(path))
                    throw new InvalidOperationException("Path is not exists");

                this.moduleLoadPaths.Add(path);
            }
            catch
            {
                throw;
            }
        }

        protected void AddModuleCurrentPath()
        {
            try
            {
                var currentPath = AppDomain.CurrentDomain.BaseDirectory;
                this.moduleLoadPaths.Add(currentPath);
            }
            catch
            {
                throw;
            }

        }

        protected void RejectModule(string name)
        {
            try
            {
                this.moduleRejectNames.Add(name);
            }
            catch
            {
                throw;
            }
        }

        protected void EnableAutoModuleSearch(bool enable)
        {
            this.enableAutoModuleSearch = enable;
        }
        #endregion


        #region Pulbic Functions
        public void Run(Application app)
        {

            this.RegisterServices(this.serviceRegistry);
            this.serviceRegistry.AddWPFUIDispatcher();
            this.serviceRegistry.AddRegionManager();
            

            // Module Registeration
            RegisterModules();
            if (this.enableAutoModuleSearch)
            {
                LoadAssemblyStart();
            }

            this.serviceRegistry.AddModuleManager(this.modules, this.categories);


            // 여기서 Build (RegisterTypes 끝난 뒤)
            this.serviceContainer = new Host.DependencyInjection.ServiceContainer(this.serviceRegistry);
            ServiceLocator.Initialize(this.serviceContainer);
            var regionManager = this.serviceContainer.GetService<IRegionManager>();
            var moduleManager = this.serviceContainer.GetService<IModuleManager>();

            
            
            moduleManager.ConfigureRegions();
            ConfigureRegion(regionManager);

            var shell = CreateShell(this.serviceContainer);
            InitializeShell(app, shell);

            OnInitialized(this.serviceContainer);
            moduleManager.InitializeModules();
        }
        #endregion


        #region Protected Functions
        protected abstract void RegisterModules();
        protected abstract void RegisterServices(Core.DependencyInjection.Abstractions.IServiceRegistry container);
        protected abstract void ConfigureRegion(IRegionManager regionManager);
        protected abstract Window CreateShell(Core.DependencyInjection.Abstractions.IServiceContainer provider);
        protected virtual void InitializeShell(Application app, Window shell)
        {
            app.MainWindow = shell;
            shell.Show();
        }

        protected abstract void OnInitialized(Core.DependencyInjection.Abstractions.IServiceContainer provider);
        #endregion
    }
}
