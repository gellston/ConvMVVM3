using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public class RegionManager : IRegionManager
    {
        #region Private Property
        private readonly IServiceContainer serviceContainer;
        private readonly Dictionary<string, IRegion> regions = new Dictionary<string, IRegion>();
        #endregion

        #region Constructor
        public RegionManager(IServiceContainer serviceContainer)
        {
            this.serviceContainer = serviceContainer;
            
        }
        #endregion


        #region Public Property
        public ReadOnlyDictionary<string, IRegion> Regions => new ReadOnlyDictionary<string, IRegion>(this.regions);
        #endregion



        #region Public Functions

        public void RegisterViewWithRegion(string name, RegionType regionType = RegionType.SingleView)
        {
            try
            {
                if (!this.regions.ContainsKey(name))
                {
                    this.regions.Add(name, new Region()
                    {
                        Name = name,
                        RegionType = regionType,
                    });
                }

            }
            catch
            {
                throw;
            }
        }

        public void RegisterViewWithRegion(string name, Type viewType, RegionType regionType = RegionType.SingleView)
        {
            try
            {
                if (!this.regions.ContainsKey(name))
                {
                    this.regions.Add(name, new Region()
                    {
                        Name = name,
                        RegionType = regionType,
                    });
                }


                var view  = this.serviceContainer.GetRequiredService(viewType);
                var region = this.regions[name];
                region.Content = view;
                region.Views.Add(view);
                

            }
            catch
            {
                throw;
            }
        }

        public void RegisterViewWithRegion<T>(string name, RegionType regionType = RegionType.SingleView)
        {
            try
            {
                if (!this.regions.ContainsKey(name))
                {
                    this.regions.Add(name, new Region()
                    {
                        Name = name,
                        RegionType = regionType
                    });
                }


                var view = this.serviceContainer.GetRequiredService<T>();
                var region = this.regions[name];
                region.Content = view;
                region.Views.Add(view);


            }
            catch
            {
                throw;
            }
        }

        public void RegisterViewWithRegion(string name, params Type[] types)
        {
            try
            {
                if (!this.regions.ContainsKey(name))
                {
                    this.regions.Add(name, new Region()
                    {
                        Name = name,
                        RegionType = RegionType.MultiView
                    });
                }


                if(types == null || types.Length == 0)
                {
                    throw new InvalidOperationException("Invalid types information");
                }

                foreach (var type in types)
                {
                    var view = this.serviceContainer.GetRequiredService(type);
                    var region = this.regions[name];
                    region.Content = view;
                    region.Views.Add(view);
                }

            }
            catch
            {
                throw;
            }
        }

        public void RegisterViewWithRegion(string name, string typeName, RegionType regionType = RegionType.SingleView)
        {
            try
            {

                if (!this.regions.ContainsKey(name))
                {
                    this.regions.Add(name, new Region()
                    {
                        Name = name,
                        RegionType = regionType
                    });
                }


                var view = this.serviceContainer.GetRequiredService(typeName);
                var region = this.regions[name];
                region.Content = view;
                region.Views.Add(view);


            }
            catch
            {
                throw;
            }
        }

        public void RegisterViewWithRegion(string name, params string[] typeNames)
        {
            try
            {
                if (!this.regions.ContainsKey(name))
                {
                    this.regions.Add(name, new Region()
                    {
                        Name = name,
                        RegionType = RegionType.MultiView
                    });
                }


                if (typeNames == null || typeNames.Length == 0)
                {
                    throw new InvalidOperationException("Invalid typeNames information");
                }

                foreach (var typeName in typeNames)
                {
                    var view = this.serviceContainer.GetRequiredService(typeName);
                    var region = this.regions[name];
                    region.Content = view;
                    region.Views.Add(view);
                }
            }
            catch
            {
                throw;
            }
        }



        public void RequestNavigate(string name, Type viewType, NavigationParameters parameters = null, Action<NavigationContext> result = null)
        {
            if (!this.regions.ContainsKey(name))
            {
                throw new InvalidOperationException($"Invalid region name : {name}");
            }

            var navigation = new NavigationContext();
            var region = this.regions[name];


            if (region.RegionType != RegionType.SingleView)
            {
                throw new InvalidOperationException("Invalid region type");
            }

            region.NavigationContext = navigation;
            var prevView = region.Content;
            if (prevView is INavigationAware prevNavigationAware)
            {
                prevNavigationAware.OnNavigatedFrom(navigation);
            }


            var view = this.serviceContainer.GetRequiredService(viewType);
            if (view is INavigationAware navigationAware)
            {
                navigation.Parameters = parameters;
                if (navigationAware.IsNavigationTarget(navigation))
                    navigationAware.OnNavigatedTo(navigation);
            }


            result?.Invoke(navigation);
        }

        public void RequestNavigate<T>(string name, NavigationParameters parameters = null, Action<NavigationContext> result = null)
        {
            try
            {
                if (!this.regions.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Invalid region name : {name}");
                }

                var navigation = new NavigationContext();
                var region = this.regions[name];


                if (region.RegionType != RegionType.SingleView)
                {
                    throw new InvalidOperationException("Invalid region type");
                }

                region.NavigationContext = navigation;
                var prevView = region.Content;
                if (prevView is INavigationAware prevNavigationAware)
                {
                    prevNavigationAware.OnNavigatedFrom(navigation);
                }


                var view = this.serviceContainer.GetRequiredService(typeof(T));
                if (view is INavigationAware navigationAware)
                {
                    navigation.Parameters = parameters;
                    if (navigationAware.IsNavigationTarget(navigation))
                        navigationAware.OnNavigatedTo(navigation);
                }

                result?.Invoke(navigation);

            }
            catch
            {
                throw;
            }
        }




        public void RequestNavigate(string name, string typeName, NavigationParameters parameters = null, Action<NavigationContext> result = null)
        {
            try
            {
                if (!this.regions.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Invalid region name : {name}");
                }

                var navigation = new NavigationContext();
                var region = this.regions[name];


                if (region.RegionType != RegionType.SingleView)
                {
                    throw new InvalidOperationException("Invalid region type");
                }

                region.NavigationContext = navigation;
                var prevView = region.Content;
                if(prevView is INavigationAware prevNavigationAware)
                {
                    prevNavigationAware.OnNavigatedFrom(navigation);
                }


                var view = this.serviceContainer.GetRequiredService(typeName);
                if(view is INavigationAware navigationAware)
                {
                    navigation.Parameters = parameters;
                    if(navigationAware.IsNavigationTarget(navigation))
                        navigationAware.OnNavigatedTo(navigation);
                }



                result?.Invoke(navigation);
            }
            catch
            {
                throw;
            }
        }

        public void RequestNavigate(string name, string[] typeNames)
        {
            try
            {
                if (!this.regions.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Invalid region name : {name}");
                }

                var region = this.regions[name];
                if (region.RegionType != RegionType.MultiView)
                {
                    throw new InvalidOperationException("Invalid region type");
                }

                foreach (var typeName in typeNames)
                {
                    var view = this.serviceContainer.GetRequiredService(typeName);
                    region.Content = view;
                    region.Views.Add(view);
                }

            }
            catch
            {
                throw;
            }
        }

        public void RequestNavigate(string name, string typeName, int repeat)
        {
            try
            {

                if (!this.regions.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Invalid region name : {name}");
                }

                var region = this.regions[name];
                if(region.RegionType != RegionType.MultiView)
                {
                    throw new InvalidOperationException("Invalid region type");
                }


                for (int count = 0; count < repeat; count++)
                {
                    this.RequestNavigate(name, typeName);
                }
            }
            catch
            {
                throw;
            }

        }

        public void ClearRegion(string name)
        {
            try
            {
                if (!this.regions.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Invalid region name : {name}");
                }

                var region = this.regions[name];
                region.SelectedItem = null;
                region.Content = null;
                region.Views.Clear();
                region.NavigationContext = null;
            }
            catch
            {
                throw;
            }
        }

        public void Active(string name, object view)
        {
            try
            {
                if (!this.regions.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Invalid region name : {name}");
                }

                var region = this.regions[name];
                region.SelectedItem = view;
            }
            catch
            {
                throw;
            }
        }


        public void Active(string name, string viewName)
        {
            try
            {
                if (!this.regions.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Invalid region name : {name}");
                }

                var view = this.serviceContainer.GetRequiredService(viewName);
                var region = this.regions[name];
                region.SelectedItem = view;
            }
            catch
            {
                throw;
            }
        }
        #endregion

    }
}
