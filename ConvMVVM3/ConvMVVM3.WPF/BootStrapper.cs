using ConvMVVM3.Core;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Modules;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.Host.DependencyInjection;
using ConvMVVM3.WPF.UIDispatcher;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ConvMVVM3.WPF
{
    public abstract class Bootstrapper
    {
        #region Private Property
        private readonly Core.DependencyInjection.Abstractions.IServiceRegistry serviceRegistry = new ServiceCollection();
        private Core.DependencyInjection.Abstractions.IServiceContainer serviceContainer = null;
        #endregion

        public void Run(Application app)
        {

            this.RegisterServices(this.serviceRegistry);
            this.serviceRegistry.AddWPFUIDispatcher();
            this.serviceRegistry.AddRegionManager();


            // 여기서 Build (RegisterTypes 끝난 뒤)
            this.serviceContainer = new Host.DependencyInjection.ServiceContainer(this.serviceRegistry);
            ServiceLocator.Initialize(this.serviceContainer);
            var region = this.serviceContainer.GetService<IRegionManager>();


            ConfigureRegion(region);
            

            var shell = CreateShell(this.serviceContainer);
            InitializeShell(app, shell);


            OnInitialized(this.serviceContainer);
        }


        protected virtual void RegisterServices(Core.DependencyInjection.Abstractions.IServiceRegistry container)
        {
            // Service등록 위치
        }


        protected virtual void ConfigureRegion(IRegionManager regionManager)
        {
            // WPF RegionAdapterMappings / Behaviors 같은 거 등록 위치
        }

        protected abstract Window CreateShell(Core.DependencyInjection.Abstractions.IServiceContainer provider);

        protected virtual void InitializeShell(Application app, Window shell)
        {
            app.MainWindow = shell;
            shell.Show();
        }

        protected virtual void OnInitialized(Core.DependencyInjection.Abstractions.IServiceContainer provider) { }
    }
}
