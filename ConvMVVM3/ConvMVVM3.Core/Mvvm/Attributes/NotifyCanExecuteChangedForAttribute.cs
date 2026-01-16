using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class NotifyCanExecuteChangedForAttribute : Attribute
    {
        public string[] CommandNames { get; }

        public NotifyCanExecuteChangedForAttribute(params string[] commandNames)
        {
            CommandNames = commandNames;
        }
    }
}