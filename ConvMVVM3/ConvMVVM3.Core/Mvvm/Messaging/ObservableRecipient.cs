using ConvMVVM3.Core.Mvvm.Messaging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Messaging
{
    public abstract class ObservableRecipient : ObservableObject, IDisposable
    {
        private bool _isActive;
        private bool _disposed;

        private Type[] _messageTypes;

        protected ObservableRecipient()
            : this(WeakReferenceMessenger.Default)
        {
        }

        protected ObservableRecipient(IMessenger messenger)
        {
            if (messenger == null) throw new ArgumentNullException(nameof(messenger));
            Messenger = messenger;
        }

        protected IMessenger Messenger { get; }

        /// <summary>
        /// 이 Recipient가 사용할 채널 토큰.
        /// - null이면 기본 채널(전체 브로드캐스트)
        /// - 값이 있으면 해당 토큰 채널로만 등록/수신
        ///
        /// 주의: IsActive=true 상태에서 토큰을 바꾸면, 다시 등록이 필요하므로
        /// 기본 구현은 예외를 던지게 해 두었다.
        /// </summary>
        public object Token
        {
            get { return _token; }
            set
            {
                if (ReferenceEquals(_token, value)) return;

                // 활성 상태에서 채널 바꾸는 건 흔히 버그로 이어짐
                if (_isActive)
                    throw new InvalidOperationException("Cannot change Token while IsActive is true. Deactivate first.");

                _token = value;
                OnTokenChanged();
            }
        }
        private object _token;

        protected virtual void OnTokenChanged() { }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive == value) return;
                SetProperty(ref _isActive, value, OnIsActiveChanged);
            }
        }

        private void OnIsActiveChanged()
        {
            if (_disposed) return;

            if (_isActive) Activate();
            else Deactivate();
        }

        protected virtual void Activate()
        {
            EnsureMessageTypesCached();
            RegisterAllRecipients();
            OnActivated();
        }

        protected virtual void Deactivate()
        {
            OnDeactivating();
            Messenger.UnregisterAll(this);
            OnDeactivated();
        }

        protected virtual void OnActivated() { }
        protected virtual void OnDeactivating() { }
        protected virtual void OnDeactivated() { }

        private void EnsureMessageTypesCached()
        {
            if (_messageTypes != null) return;

            _messageTypes = GetType()
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRecipient<>))
                .Select(i => i.GetGenericArguments()[0])
                .Distinct()
                .ToArray();
        }

        private void RegisterAllRecipients()
        {
            if (_messageTypes == null || _messageTypes.Length == 0) return;

            for (int i = 0; i < _messageTypes.Length; i++)
                RegisterByMessageType(_messageTypes[i]);
        }

        // IMessenger.Register<TMessage>(IRecipient<TMessage> recipient, object token = null)
        private static readonly MethodInfo RegisterRecipientMethod = FindRegisterRecipientMethod();

        private static MethodInfo FindRegisterRecipientMethod()
        {
            var methods = typeof(IMessenger).GetMethods();

            for (int i = 0; i < methods.Length; i++)
            {
                var m = methods[i];
                if (m.Name != "Register") continue;
                if (!m.IsGenericMethodDefinition) continue;

                var ps = m.GetParameters();
                if (ps.Length != 2) continue;

                var p0 = ps[0].ParameterType;
                if (!p0.IsGenericType) continue;
                if (p0.GetGenericTypeDefinition() != typeof(IRecipient<>)) continue;

                if (ps[1].ParameterType != typeof(object)) continue;

                return m;
            }

            throw new InvalidOperationException("IMessenger.Register<TMessage>(IRecipient<TMessage>, object) method not found.");
        }

        private void RegisterByMessageType(Type messageType)
        {
            var generic = RegisterRecipientMethod.MakeGenericMethod(messageType);
            generic.Invoke(Messenger, new object[] { this, _token /* token */ });
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_isActive)
            {
                try { Messenger.UnregisterAll(this); }
                catch { }
            }
        }
    }
}
