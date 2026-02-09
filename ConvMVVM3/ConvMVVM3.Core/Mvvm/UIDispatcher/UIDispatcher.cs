using System;
using System.Threading;
using System.Threading.Tasks;
using ConvMVVM3.Core.Mvvm.UIDispatcher.Abstractions;

namespace ConvMVVM3.Core.Mvvm.UIDispatcher
{
    /// <summary>
    /// Platform-independent implementation of IUIDispatcher using SynchronizationContext.
    /// This is the default implementation for non-UI platforms or when no specific UI dispatcher is available.
    /// </summary>
    public class UIDispatcher : IUIDispatcher
    {
        #region Private Property
        private readonly SynchronizationContext _context;
        #endregion



        #region Constructor

        /// <summary>
        /// Initializes a new instance of the UIDispatcher class.
        /// </summary>
        /// <param name="context">The synchronization context to use. If null, uses SynchronizationContext.Current.</param>
        public UIDispatcher(SynchronizationContext context = null)
        {
            _context = context ?? SynchronizationContext.Current;
        }

        #endregion


        #region Static Property

        /// <summary>
        /// Gets the default UIDispatcher instance.
        /// </summary>
        public static UIDispatcher Default { get; } = new UIDispatcher();
        #endregion


        #region Public Functions

        /// <summary>
        /// Determines whether the calling thread has access to this object.
        /// </summary>
        /// <returns>true if the calling thread has access; otherwise, false.</returns>
        public bool CheckAccess()
        {
            // If there's no synchronization context, assume we're on the correct thread
            if (_context == null)
                return true;

            // Check if we're on the synchronization context's thread
            return SynchronizationContext.Current == _context;
        }

        /// <summary>
        /// Enforces that the calling thread has access to this object.
        /// </summary>
        /// <exception cref="InvalidOperationException">The calling thread does not have access to this object.</exception>
        public void VerifyAccess()
        {
            if (!CheckAccess())
            {
                throw new InvalidOperationException("The calling thread cannot access this object because a different thread owns it.");
            }
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
                // Use TaskCompletionSource to wait for completion
                InvokeAsync(callback).GetAwaiter().GetResult();
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
                // Use TaskCompletionSource to wait for completion
                return InvokeAsync(callback).GetAwaiter().GetResult();
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
                if (_context != null)
                {
                    var tcs = new TaskCompletionSource<object>();
                    _context.Post(_ =>
                    {
                        try
                        {
                            callback();
                            tcs.SetResult(null);
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                    }, null);

                    return tcs.Task;
                }
                else
                {
                    // No synchronization context available, run on thread pool
                    return Task.Run(callback);
                }
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
                if (_context != null)
                {
                    var tcs = new TaskCompletionSource<T>();
                    _context.Post(_ =>
                    {
                        try
                        {
                            var result = callback();
                            tcs.SetResult(result);
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                    }, null);

                    return tcs.Task;
                }
                else
                {
                    // No synchronization context available, run on thread pool
                    return Task.Run(callback);
                }
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
                await InvokeAsync(async () =>
                {
                    await callback();
                });
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
                return await InvokeAsync(async () =>
                {
                    return await callback();
                });
            }
        }

        #endregion
    }
}