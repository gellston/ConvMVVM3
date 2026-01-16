using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Messaging.Abstractions
{
    public interface IRecipient
    {
    }

    public interface IRecipient<TMessage> : IRecipient
    {
        void Receive(TMessage message);
    }
}
