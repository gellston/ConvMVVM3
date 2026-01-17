using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using ConvMVVM3.Core.Mvvm.Abstractions;

namespace ConvMVVM3.WPF
{
    /// <summary>
    /// WPF-specific implementation of IUIDispatcher using System.Windows.Threading.Dispatcher.
    /// Provides optimized UI thread dispatching for WPF applications.
    /// </summary>
    public class WPFUIDispatcher : IUIDispatcher
    {
        private readonly System.Windows.Threading.Dispatcher _dispatcher;

        /// <summary>
        /// Initializes a new instance of WPFUIDispatcher class.
        /// </summary>
        /// <param name="dispatcher">The WPF dispatcher to use.</param>
        public WPFUIDispatcher(System.Windows.Threading.Dispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? System.Windows.Threading.Dispatcher.CurrentDispatcher;
        }



        /// <summary>
        /// Determines whether the calling thread has access to this object.
        /// </summary>
        /// <returns>true if the calling thread has access; otherwise, false.</returns>
        public bool CheckAccess()
        {
            return _dispatcher.CheckAccess();
        }

        /// <summary>
        /// Enforces that the calling thread has access to this object.
        /// </summary>
        /// <exception cref="InvalidOperationException">The calling thread does not have access to this object.</exception>
        public void VerifyAccess()
        {
            _dispatcher.VerifyAccess();
        }

        /// <summary>
        /// Executes the specified delegate synchronously on the UI thread.
        /// </summary>
        /// <param name="callback">The delegate to invoke.</param>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        public void Invoke(Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (CheckAccess())
            {
                callback();
            }
            else
            {
                _dispatcher.Invoke(callback);
            }
        }

        /// <summary>
        /// Executes the specified delegate synchronously on the UI thread and returns the result.
        /// </summary>
        /// <typeparam name="T">The return type of the delegate.</typeparam>
        /// <param name="callback">The delegate to invoke.</param>
        /// <returns>The result of the delegate execution.</returns>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        public T Invoke<T>(Func<T> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (CheckAccess())
            {
                return callback();
            }
            else
            {
                return _dispatcher.Invoke(callback);
            }
        }

        /// <summary>
        /// Executes the specified delegate asynchronously on the UI thread.
        /// </summary>
        /// <param name="callback">The delegate to invoke.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        public Task InvokeAsync(Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (CheckAccess())
            {
                try
                {
                    callback();
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    return Task.FromException(ex);
                }
            }
            else
            {
                return _dispatcher.InvokeAsync(callback).Task;
            }
        }

        /// <summary>
        /// Executes the specified delegate asynchronously on the UI thread and returns the result.
        /// </summary>
        /// <typeparam name="T">The return type of the delegate.</typeparam>
        /// <param name="callback">The delegate to invoke.</param>
        /// <returns>A task that contains the result of the delegate execution.</returns>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        public Task<T> InvokeAsync<T>(Func<T> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (CheckAccess())
            {
                try
                {
                    var result = callback();
                    return Task.FromResult(result);
                }
                catch (Exception ex)
                {
                    return Task.FromException<T>(ex);
                }
            }
            else
            {
                return _dispatcher.InvokeAsync(callback).Task;
            }
        }

        /// <summary>
        /// Executes the specified asynchronous delegate on the UI thread.
        /// </summary>
        /// <param name="callback">The asynchronous delegate to invoke.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        public async Task InvokeAsync(Func<Task> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (CheckAccess())
            {
                await callback();
            }
            else
            {
                _dispatcher.Invoke(callback);
            }
        }

        /// <summary>
        /// Executes the specified asynchronous delegate on the UI thread and returns the result.
        /// </summary>
        /// <typeparam name="T">The return type of the delegate.</typeparam>
        /// <param name="callback">The asynchronous delegate to invoke.</param>
        /// <returns>A task that contains the result of the delegate execution.</returns>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        public async Task<T> InvokeAsync<T>(Func<Task<T>> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (CheckAccess())
            {
                return await callback();
            }
            else
            {
                var tcs = new TaskCompletionSource<T>();
                _dispatcher.InvokeAsync(async () =>
                {
                    try
                    {
                        var result = await callback();
                        tcs.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });
                return await tcs.Task;
            }
        }

        /// <summary>
        /// Executes the specified delegate asynchronously on the UI thread with the specified priority.
        /// </summary>
        /// <param name="callback">The delegate to invoke.</param>
        /// <param name="priority">The priority with which to invoke the delegate.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        public Task InvokeAsync(Action callback, System.Windows.Threading.DispatcherPriority priority)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (CheckAccess())
            {
                try
                {
                    callback();
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    return Task.FromException(ex);
                }
            }
            else
            {
                return _dispatcher.InvokeAsync(callback, priority).Task;
            }
        }

        /// <summary>
        /// Executes the specified delegate asynchronously on the UI thread with the specified priority and returns the result.
        /// </summary>
        /// <typeparam name="T">The return type of the delegate.</typeparam>
        /// <param name="callback">The delegate to invoke.</param>
        /// <param name="priority">The priority with which to invoke the delegate.</param>
        /// <returns>A task that contains the result of the delegate execution.</returns>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        public Task<T> InvokeAsync<T>(Func<T> callback, System.Windows.Threading.DispatcherPriority priority)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (CheckAccess())
            {
                try
                {
                    var result = callback();
                    return Task.FromResult(result);
                }
                catch (Exception ex)
                {
                    return Task.FromException<T>(ex);
                }
            }
            else
            {
                return _dispatcher.InvokeAsync(callback, priority).Task;
            }
        }

        /// <summary>
        /// Begins execution of the specified delegate asynchronously on the UI thread.
        /// </summary>
        /// <param name="callback">The delegate to invoke.</param>
        /// <param name="priority">The priority with which to invoke the delegate.</param>
        /// <returns>A DispatcherOperation that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        public System.Windows.Threading.DispatcherOperation BeginInvoke(Action callback, System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Normal)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            return _dispatcher.BeginInvoke(callback, priority);
        }
    }
}