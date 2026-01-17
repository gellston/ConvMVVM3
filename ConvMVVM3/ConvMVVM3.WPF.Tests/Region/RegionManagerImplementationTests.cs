using System;
using System.Linq;
using System.Windows.Controls;
using ConvMVVM3.Core.Navigation;
using ConvMVVM3.Core.Navigation.Abstractions;
using ConvMVVM3.Host.DependencyInjection;
using ConvMVVM3.WPF;
using ConvMVVM3.Core.Navigation;
using RegionClass = ConvMVVM3.Core.Navigation.Region;
using RegionMetadataClass = ConvMVVM3.Core.Navigation.RegionMetadata;
using Xunit;

namespace ConvMVVM3.WPF.Tests.Region
{
    /// <summary>
    /// Tests for RegionManager concrete implementation.
    /// </summary>
    public class RegionManagerImplementationTests : IDisposable
    {
        private readonly ServiceContainer _serviceContainer;

        public RegionManagerImplementationTests()
        {
            // Setup DI container with RegionManager
            var services = new ServiceCollection();
            services.AddRegionManager();
            _serviceContainer = new ServiceContainer(services);
            ServiceLocator.Initialize(_serviceContainer);
        }

        public void Dispose()
        {
            ServiceLocator.Initialize(null);
        }

        [Fact]
        public void RegionManager_Can_Be_Resolved_From_DI()
        {
            // Act
            var regionManager = _serviceContainer.GetService<IRegionManager>();

            // Assert
            Assert.NotNull(regionManager);
            Assert.IsType<ConvMVVM3.Core.Navigation.RegionManager>(regionManager);
        }

        [Fact]
        public void RegionManager_Can_Create_Region()
        {
            // Arrange
            var regionManager = _serviceContainer.GetService<IRegionManager>();
            var regionName = "TestRegion";

            // Act
            var region = regionManager.CreateRegion(regionName);

            // Assert
            Assert.NotNull(region);
            Assert.Equal(regionName, region.Name);
        }

        [Fact]
        public void RegionManager_Add_Region_Works()
        {
            // Arrange
            var regionManager = _serviceContainer.GetService<IRegionManager>();
            var regionName = "TestRegion";
            var region = regionManager.CreateRegion(regionName);

            // Act
            regionManager.AddRegion(regionName, region);

            // Assert
            Assert.True(regionManager.ContainsRegion(regionName));
            Assert.Equal(region, regionManager[regionName]);
        }

        [Fact]
        public void Region_Navigate_With_ViewType_Works()
        {
            // Arrange
            var regionManager = _serviceContainer.GetService<IRegionManager>();
            var regionName = "TestRegion";
            var region = regionManager.CreateRegion(regionName);
            regionManager.AddRegion(regionName, region);

            // Act
            regionManager.Navigate(regionName, typeof(Button));

            // Assert
            Assert.NotNull(region.ActiveView);
            Assert.IsType<Button>(region.ActiveView);
        }

        [Fact]
        public void Region_Navigate_With_Factory_Works()
        {
            // Arrange
            var regionManager = _serviceContainer.GetService<IRegionManager>();
            var regionName = "TestRegion";
            var region = regionManager.CreateRegion(regionName);
            regionManager.AddRegion(regionName, region);

            // Act
            regionManager.Navigate(regionName, () => new TextBlock { Text = "Test" });

            // Assert
            Assert.NotNull(region.ActiveView);
            Assert.IsType<TextBlock>(region.ActiveView);
            Assert.Equal("Test", ((TextBlock)region.ActiveView).Text);
        }

        [Fact]
        public void Region_Register_View_With_Region_Works()
        {
            // Arrange
            var regionManager = _serviceContainer.GetService<IRegionManager>();
            var regionName = "TestRegion";

            // Act
            regionManager.RegisterViewWithRegion(regionName, typeof(Button));

            // Assert
            Assert.True(regionManager.ContainsRegion(regionName));
            var region = regionManager[regionName];
            Assert.NotNull(region.Metadata);
            Assert.Contains(typeof(Button), region.Metadata.ViewTypes);
        }

        [Fact]
        public void Region_GetRegionNames_Returns_Correct_Names()
        {
            // Arrange
            var regionManager = _serviceContainer.GetService<IRegionManager>();
            regionManager.CreateRegion("Region1");
            regionManager.CreateRegion("Region2");
            regionManager.AddRegion("Region1", regionManager.CreateRegion("Region1"));
            regionManager.AddRegion("Region2", regionManager.CreateRegion("Region2"));

            // Act
            var regionNames = regionManager.GetRegionNames();

            // Assert
            Assert.Contains("Region1", regionNames);
            Assert.Contains("Region2", regionNames);
        }

        [Fact]
        public void Region_Contains_View_Works()
        {
            // Arrange
            var region = new RegionClass("TestRegion");
            var view = new Button();

            // Act
            region.Add(view);

            // Assert
            Assert.True(region.Views.Contains(view));
        }

        [Fact]
        public void Region_Activate_Deactivate_Works()
        {
            // Arrange
            var region = new RegionClass("TestRegion");
            var view1 = new Button();
            var view2 = new TextBox();
            
            region.Add(view1);
            region.Add(view2);

            // Act - Activate view1
            region.Activate(view1);
            Assert.Equal(view1, region.ActiveView);

            // Act - Activate view2 (should deactivate view1)
            region.Activate(view2);
            Assert.Equal(view2, region.ActiveView);

            // Act - Deactivate view2
            region.Deactivate(view2);
            Assert.Null(region.ActiveView);
        }

        [Fact]
        public void RegionMetadata_Register_ViewType_Works()
        {
            // Arrange
            var metadata = new RegionMetadata("TestRegion");

            // Act
            metadata.RegisterViewType(typeof(Button));

            // Assert
            Assert.Contains(typeof(Button), metadata.ViewTypes);
        }

        [Fact]
        public void RegionMetadata_Register_ViewFactory_Works()
        {
            // Arrange
            var metadata = new RegionMetadata("TestRegion");
            var factory = new Func<object>(() => new Button());

            // Act
            metadata.RegisterViewFactory(factory);

            // Assert
            Assert.Contains(factory, metadata.ViewFactories);
        }
    }
}