using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Abstractions;
using ConvMVVM3.Core.Mvvm.Regions.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

namespace ConvMVVM3.Core.Mvvm.Regions
{
    public sealed class RegionManager : IRegionManager
    {
        #region Private Property
        private readonly IServiceContainer serviceContainer;
        private readonly INavigationAwareResolver navigationAwareResolver;
        private readonly Dictionary<string, IRegion> regions = new Dictionary<string, IRegion>();
        #endregion

        #region Constructor
        public RegionManager(IServiceContainer serviceContainer)
        {
            this.serviceContainer = serviceContainer ?? throw new ArgumentNullException(nameof(serviceContainer));
            this.navigationAwareResolver = serviceContainer.GetService<INavigationAwareResolver>()
                ?? new NavigationAwareResolver();
            
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


                switch (regionType)
                {
                    case RegionType.SingleView:
                        {
                            region.Content = view;
                            break;
                        }

                    case RegionType.MultiView:
                        {
                            region.Views.Add(view);
                            break;
                        }
                }

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

                switch (regionType)
                {
                    case RegionType.SingleView:
                        {
                            region.Content = view;
                            break;
                        }

                    case RegionType.MultiView:
                        {
                            region.Views.Add(view);
                            break;
                        }
                }

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
                switch (regionType)
                {
                    case RegionType.SingleView:
                        {
                            region.Content = view;
                            break;
                        }

                    case RegionType.MultiView:
                        {
                            region.Views.Add(view);
                            break;
                        }
                }


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
            NavigateSingle(name, () => this.serviceContainer.GetRequiredService(viewType), parameters, result);
        }

        public void RequestNavigate<T>(string name, NavigationParameters parameters = null, Action<NavigationContext> result = null)
        {
            RequestNavigate(name, typeof(T), parameters, result);
        }

        public void RequestNavigate(string name, string typeName, NavigationParameters parameters = null, Action<NavigationContext> result = null)
        {
            NavigateSingle(name, () => this.serviceContainer.GetRequiredService(typeName), parameters, result);
        }

        private void NavigateSingle(string name, Func<object> createView, NavigationParameters parameters, Action<NavigationContext> result)
        {
            if (!this.regions.ContainsKey(name))
                throw new InvalidOperationException($"Invalid region name : {name}");

            var region = this.regions[name];
            if (region.RegionType != RegionType.SingleView)
                throw new InvalidOperationException("Invalid region type");

            var navigation = new NavigationContext
            {
                Parameters = parameters ?? new NavigationParameters()
            };

            // Prepare the destination before notifying or replacing the current view.
            // WPF's resolver ensures AutoWireViewModel is complete without waiting for Loaded.
            var view = createView();
            var targets = GetNavigationParticipants(view);
            var previousTargets = GetNavigationParticipants(region.Content);
            region.NavigationContext = navigation;
            foreach (var target in previousTargets)
                target.OnNavigatedFrom(navigation);

            region.Content = view;
            foreach (var target in targets)
            {
                // Preserve the existing per-participant gate. This is not a navigation veto
                // or a Prism-style search for reusable view instances.
                if (target.IsNavigationTarget(navigation))
                    target.OnNavigatedTo(navigation);
            }

            result?.Invoke(navigation);
        }

        private List<INavigationAware> GetNavigationParticipants(object view)
        {
            var participants = new List<INavigationAware>();
            if (view == null) return participants;

            foreach (var participant in this.navigationAwareResolver.Resolve(view))
            {
                if (participant == null) continue;
                var duplicate = false;
                foreach (var existing in participants)
                {
                    if (ReferenceEquals(existing, participant))
                    {
                        duplicate = true;
                        break;
                    }
                }
                if (!duplicate) participants.Add(participant);
            }
            return participants;
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
                    var view = this.serviceContainer.GetRequiredService(typeName);
                    region.Views.Add(view);
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


                if(region.Content is IDestructible destructor)
                {
                    destructor.Destroy();
                }

                region.Content = null;


                foreach(var view in region.Views)
                {
                    if(view is IDestructible viewDestructor)
                    {
                        viewDestructor.Destroy();
                    }
                }

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

        public void Active(string name, int index)
        {
            try
            {
                if (!this.regions.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Invalid region name : {name}");
                }

                
                var region = this.regions[name];
                if (index >= 0 && region.Views.Count > index)
                    region.SelectedItem = region.Views[index];
            }
            catch
            {
                throw;
            }
        }
        #endregion

    }
}
