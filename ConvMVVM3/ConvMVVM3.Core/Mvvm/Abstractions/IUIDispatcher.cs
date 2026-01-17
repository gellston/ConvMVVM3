using System;
using System.Threading.Tasks;

namespace ConvMVVM3.Core.Mvvm.Abstractions
{
    /// <summary>
    /// Provides UI thread dispatching services for cross-platform applications.
    /// Similar to Microsoft's Dispatcher but abstracted for different UI frameworks.
    /// </summary>
    public interface IUIDispatcher
    {
        /// <summary>
        /// Determines whether the calling thread has access to this object.
        /// </summary>
        /// <returns>true if the calling thread has access; otherwise, false.</returns>
        bool CheckAccess();

        /// <summary>
        /// Enforces that the calling thread has access to this object.
        /// </summary>
        /// <exception cref="InvalidOperationException">The calling thread does not have access to this object.</exception>
        void VerifyAccess();

        /// <summary>
        /// Executes the specified delegate synchronously on the UI thread.
        /// </summary>
        /// <param name="callback">The delegate to invoke.</param>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        void Invoke(Action callback);

        /// <summary>
        /// Executes the specified delegate synchronously on the UI thread and returns the result.
        /// </summary>
        /// <typeparam name="T">The return type of the delegate.</typeparam>
        /// <param name="callback">The delegate to invoke.</param>
        /// <returns>The result of the delegate execution.</returns>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        T Invoke<T>(Func<T> callback);

        /// <summary>
        /// Executes the specified delegate asynchronously on the UI thread.
        /// </summary>
        /// <param name="callback">The delegate to invoke.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        Task InvokeAsync(Action callback);

        /// <summary>
        /// Executes the specified delegate asynchronously on the UI thread and returns the result.
        /// </summary>
        /// <typeparam name="T">The return type of the delegate.</typeparam>
        /// <param name="callback">The delegate to invoke.</param>
        /// <returns>A task that contains the result of the delegate execution.</returns>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        Task<T> InvokeAsync<T>(Func<T> callback);

        /// <summary>
        /// Executes the specified asynchronous delegate on the UI thread.
        /// </summary>
        /// <param name="callback">The asynchronous delegate to invoke.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        Task InvokeAsync(Func<Task> callback);

        /// <summary>
        /// Executes the specified asynchronous delegate on the UI thread and returns the result.
        /// </summary>
        /// <typeparam name="T">The return type of the delegate.</typeparam>
        /// <param name="callback">The asynchronous delegate to invoke.</param>
        /// <returns>A task that contains the result of the delegate execution.</returns>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        Task<T> InvokeAsync<T>(Func<Task<T>> callback);
    }
}