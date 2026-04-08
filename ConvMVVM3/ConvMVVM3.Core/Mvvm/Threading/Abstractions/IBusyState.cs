using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Threading.Abstractions
{
    public interface IBusyState
    {
        bool IsBusy { get; set; }
    }
}
