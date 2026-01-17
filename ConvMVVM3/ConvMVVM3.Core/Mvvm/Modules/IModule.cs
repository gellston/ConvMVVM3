using ConvMVVM3.Core.DependencyInjection.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Modules
{
    public interface IModule
    {
        void RegisterTypes(IServiceRegistry registry);
        void OnInitialized(IServiceContainer container);
    }
}
