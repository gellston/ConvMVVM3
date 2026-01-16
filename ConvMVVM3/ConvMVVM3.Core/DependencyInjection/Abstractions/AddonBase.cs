using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConvMVVM3.Core.DependencyInjection.Abstractions
{
    public abstract class AddonBase : IAddon
    {
        public abstract string Id { get; }

        public abstract Task<object> InvokeAsync(string command, CancellationToken ct, params object[] args);

        public virtual async Task<T> InvokeAsync<T>(string command, CancellationToken ct, params object[] args)
        {
            var raw = await InvokeAsync(command, ct, args).ConfigureAwait(false);
            return ReturnValue.As<T>(raw);
        }

        public virtual object Invoke(string command, params object[] args)
        {
            // 동기 호출은 필요할 때만 사용(Deadlock 주의)
            return InvokeAsync(command, CancellationToken.None, args).GetAwaiter().GetResult();
        }

        public virtual T Invoke<T>(string command, params object[] args)
        {
            var raw = Invoke(command, args);
            return ReturnValue.As<T>(raw);
        }
    }
}
