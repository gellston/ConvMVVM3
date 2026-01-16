using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class NotifyCanExecuteChangedForAttribute : Attribute
    {
        public string[] CommandPropertyNames { get; }

        public NotifyCanExecuteChangedForAttribute(params string[] commandPropertyNames)
        {
            CommandPropertyNames = commandPropertyNames ?? throw new ArgumentNullException(nameof(commandPropertyNames));
        }
    }
}
