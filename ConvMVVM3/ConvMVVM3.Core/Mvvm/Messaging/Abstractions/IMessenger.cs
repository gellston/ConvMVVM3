using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Messaging.Abstractions
{
    public interface IMessenger
    {

        #region Public Functions
        void Register<TRecipient, TMessage>(TRecipient recipient,Action<TRecipient, TMessage> handler,object token = null)
            where TRecipient : class;

        void Unregister<TRecipient, TMessage>(TRecipient recipient, object token = null)
            where TRecipient : class;

    
        void Register<TMessage>(IRecipient<TMessage> recipient, object token = null);
        void Unregister<TMessage>(IRecipient<TMessage> recipient, object token = null);

        void UnregisterAll(object recipient);

        void Send<TMessage>(TMessage message, object token = null);

        void Cleanup();
        #endregion
    }
}
