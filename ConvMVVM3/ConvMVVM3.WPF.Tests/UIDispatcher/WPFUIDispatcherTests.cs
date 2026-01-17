using System;
using System.Threading;
using System.Threading.Tasks;
using ConvMVVM3.Core.Mvvm.Abstractions;
using ConvMVVM3.WPF;
using Xunit;

namespace ConvMVVM3.WPF.Tests
{
    /// <summary>
    /// Tests for WPF UIDispatcher functionality.
    /// </summary>
    public class WPFUIDispatcherTests
    {
        private WPFUIDispatcher _dispatcher;

        public WPFUIDispatcherTests()
        {
            _dispatcher = new WPFUIDispatcher(System.Windows.Threading.Dispatcher.CurrentDispatcher);
        }

        [Fact]
        public void CheckAccess_Returns_True_On_UI_Thread()
        {
            // Arrange & Act
            var result = _dispatcher.CheckAccess();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckAccess_Returns_False_On_Background_Thread()
        {
            // Arrange
            bool result = false;

            var thread = new Thread(() =>
            {
                result = _dispatcher.CheckAccess();
            });
            thread.SetApartmentState(ApartmentState.MTA);
            thread.Start();
            thread.Join();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void VerifyAccess_Does_Not_Throw_On_UI_Thread()
        {
            // Act & Assert
            _dispatcher.VerifyAccess();
            Assert.True(true); // Should not throw
        }

        [Fact]
        public void VerifyAccess_Throws_On_Background_Thread()
        {
            // Act & Assert
            bool exceptionThrown = false;
            var thread = new Thread(() =>
            {
                try
                {
                    _dispatcher.VerifyAccess();
                }
                catch (InvalidOperationException)
                {
                    exceptionThrown = true;
                }
            });
            thread.SetApartmentState(ApartmentState.MTA);
            thread.Start();
            thread.Join();

            Assert.True(exceptionThrown, "VerifyAccess should throw InvalidOperationException on background thread");
        }

        [Fact]
        public void Invoke_Executes_Action_On_UI_Thread()
        {
            // Arrange
            bool executed = false;
            Action callback = () => executed = true;

            // Act
            _dispatcher.Invoke(callback);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void Invoke_Executes_Function_And_Returns_Result()
        {
            // Arrange
            Func<int> callback = () => 42;

            // Act
            var result = _dispatcher.Invoke(callback);

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public void Invoke_Throws_When_Callback_Is_Null()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _dispatcher.Invoke((Action)null));
        }

        [Fact]
        public async Task InvokeAsync_Executes_Action_On_UI_Thread()
        {
            // Arrange
            bool executed = false;
            Action callback = () => executed = true;

            // Act
            await _dispatcher.InvokeAsync(callback);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public async Task InvokeAsync_Executes_Function_And_Returns_Result()
        {
            // Arrange
            Func<int> callback = () => 42;

            // Act
            var result = await _dispatcher.InvokeAsync(callback);

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task InvokeAsync_Executes_Async_Callback()
        {
            // Arrange
            Func<Task<int>> callback = async () =>
            {
                await Task.Delay(10);
                return 42;
            };

            // Act
            var result = await _dispatcher.InvokeAsync(callback);

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task InvokeAsync_Handles_Exceptions()
        {
            // Arrange
            Action callback = () => throw new InvalidOperationException("Test exception");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _dispatcher.InvokeAsync(callback));
        }

        [Fact]
        public void Default_Instance_Uses_CurrentDispatcher()
        {
            // Default property removed - this test is no longer applicable
        }
    }
}