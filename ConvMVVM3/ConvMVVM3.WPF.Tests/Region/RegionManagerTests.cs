using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace ConvMVVM3.WPF.Tests
{
    /// <summary>
    /// Simple STA thread helper for WPF tests.
    /// </summary>
    public static class StaThreadHelper
    {
        /// <summary>
        /// Executes an action on a new STA thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void Run(Action action)
        {
            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
            {
                throw new Exception("STA thread exception", exception);
            }
        }
    }

    /// <summary>
    /// Tests for RegionManager with STA thread support.
    /// </summary>
    public class RegionManagerTests
    {
        [Fact]
        public void RegionName_AttachedProperty_Can_Be_Set_And_Get()
        {
            // Arrange
            Button element = null;
            var regionName = "TestRegion";

            StaThreadHelper.Run(() =>
            {
                element = new Button();
                // Act
                RegionManagerExtensions.SetRegionName(element, regionName);
                var retrievedName = RegionManagerExtensions.GetRegionName(element);
                
                // Assert
                Assert.Equal(regionName, retrievedName);
            });
        }

        [Fact]
        public void RegionName_AttachedProperty_Defaults_To_Null()
        {
            // Arrange
            TextBlock element = null;
            string retrievedName = null;

            StaThreadHelper.Run(() =>
            {
                element = new TextBlock();
                // Act
                retrievedName = RegionManagerExtensions.GetRegionName(element);
            });

            // Assert
            Assert.Null(retrievedName);
        }

        [Fact]
        public void SelectorRegionAdapter_Can_Be_Created()
        {
            // Arrange
            ListBox listBox = null;
            SelectorRegionAdapter adapter = null;

            StaThreadHelper.Run(() =>
            {
                listBox = new ListBox();
                // Act
                adapter = new SelectorRegionAdapter();
            });

            // Assert
            Assert.NotNull(adapter);
            // Note: Full integration test would require actual region setup
        }

        [Fact]
        public void ContentControlRegionAdapter_Can_Be_Created()
        {
            // Arrange
            ContentControl contentControl = null;
            ContentControlRegionAdapter adapter = null;

            StaThreadHelper.Run(() =>
            {
                contentControl = new ContentControl();
                // Act
                adapter = new ContentControlRegionAdapter();
            });

            // Assert
            Assert.NotNull(adapter);
            // Note: Full integration test would require actual region setup
        }
    }
}