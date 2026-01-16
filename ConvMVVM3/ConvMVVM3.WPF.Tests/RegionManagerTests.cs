using ConvMVVM3.WPF;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace ConvMVVM3.WPF.Tests;

public class RegionManagerTests
{
    [Fact]
    public void RegionName_AttachedProperty_Can_Be_Set_And_Get()
    {
        // Arrange
        var element = new Button();
        var regionName = "TestRegion";

        // Act
        RegionManager.SetRegionName(element, regionName);
        var retrievedName = RegionManager.GetRegionName(element);

        // Assert
        Assert.Equal(regionName, retrievedName);
    }

    [Fact]
    public void RegionName_AttachedProperty_Defaults_To_Null()
    {
        // Arrange
        var element = new TextBlock();

        // Act
        var regionName = RegionManager.GetRegionName(element);

        // Assert
        Assert.Null(regionName);
    }

    [Fact]
    public void SelectorRegionAdapter_Can_Be_Created()
    {
        // Arrange
        var listBox = new ListBox();

        // Act
        var adapter = new SelectorRegionAdapter();

        // Assert
        Assert.NotNull(adapter);
        // Note: Full integration test would require actual region setup
    }

    [Fact]
    public void ContentControlRegionAdapter_Can_Be_Created()
    {
        // Arrange
        var contentControl = new ContentControl();

        // Act
        var adapter = new ContentControlRegionAdapter();

        // Assert
        Assert.NotNull(adapter);
        // Note: Full integration test would require actual region setup
    }
}