using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Modules;
using ConvMVVM3.Core.Mvvm.Modules.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using ConvMVVM3.Core.Mvvm.UIDispatcher.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core
{
    public static class ModuleManagerExtensions
    {
        public static IServiceRegistry AddModuleManager(this IServiceRegistry services, List<IModule> modules, List<ModuleCategory> categories)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IModuleManager>((serviceResolver) =>
            {
                return new ModuleManager(serviceResolver, modules, categories);
            });
            return services;
        }
    }
}
