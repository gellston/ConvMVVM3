using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConvMVVM3.Core.DependencyInjection.Abstractions
{
    public interface IAddon
    {
        string Id { get; }

        object Invoke(string command, params object[] args);
        T Invoke<T>(string command, params object[] args);

        Task<object> InvokeAsync(string command, CancellationToken ct, params object[] args);
        Task<T> InvokeAsync<T>(string command, CancellationToken ct, params object[] args);
    }
}
