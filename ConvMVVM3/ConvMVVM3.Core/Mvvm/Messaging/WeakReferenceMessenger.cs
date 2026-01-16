using ConvMVVM3.Core.Mvvm.Messaging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Messaging
{
    public sealed class WeakReferenceMessenger : IMessenger
    {
        public static WeakReferenceMessenger Default { get; } = new WeakReferenceMessenger();

        private static readonly object DefaultToken = new object();
        private readonly object _gate = new object();

        // messageType -> token -> subscriptions
        private readonly Dictionary<Type, Dictionary<object, List<Subscription>>> _map = new Dictionary<Type, Dictionary<object, List<Subscription>>>();

        private static object NormalizeToken(object token)
        {
            return token ?? DefaultToken;
        }

        public void Register<TRecipient, TMessage>(
            TRecipient recipient,
            Action<TRecipient, TMessage> handler,
            object token = null)
            where TRecipient : class
        {
            if (recipient == null) throw new ArgumentNullException(nameof(recipient));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var messageType = typeof(TMessage);
            var tokenKey = NormalizeToken(token);

            lock (_gate)
            {
                var tokenDict = GetOrCreateTokenDict(messageType);
                var list = GetOrCreateList(tokenDict, tokenKey);

                list.RemoveAll(s => !s.IsAlive);

                // 같은 recipient + 같은 message + 같은 token 중복 방지
                if (list.Any(s => s.MatchesRecipient(recipient)))
                    return;

                list.Add(new HandlerSubscription<TRecipient, TMessage>(recipient, handler));
            }
        }

        public void Unregister<TRecipient, TMessage>(TRecipient recipient, object token = null)
            where TRecipient : class
        {
            if (recipient == null) throw new ArgumentNullException(nameof(recipient));

            var messageType = typeof(TMessage);
            var tokenKey = NormalizeToken(token);

            lock (_gate)
            {
                Dictionary<object, List<Subscription>> tokenDict;
                if (!_map.TryGetValue(messageType, out tokenDict))
                    return;

                List<Subscription> list;
                if (!tokenDict.TryGetValue(tokenKey, out list))
                    return;

                list.RemoveAll(s => !s.IsAlive || s.MatchesRecipient(recipient));
                CleanupEmpty_NoLock(messageType, tokenKey, tokenDict, list);
            }
        }

        public void Register<TMessage>(IRecipient<TMessage> recipient, object token = null)
        {
            if (recipient == null) throw new ArgumentNullException(nameof(recipient));

            var messageType = typeof(TMessage);
            var tokenKey = NormalizeToken(token);

            lock (_gate)
            {
                var tokenDict = GetOrCreateTokenDict(messageType);
                var list = GetOrCreateList(tokenDict, tokenKey);

                list.RemoveAll(s => !s.IsAlive);

                if (list.Any(s => s.MatchesRecipient(recipient)))
                    return;

                list.Add(new RecipientSubscription<TMessage>(recipient));
            }
        }

        public void Unregister<TMessage>(IRecipient<TMessage> recipient, object token = null)
        {
            if (recipient == null) throw new ArgumentNullException(nameof(recipient));

            var messageType = typeof(TMessage);
            var tokenKey = NormalizeToken(token);

            lock (_gate)
            {
                Dictionary<object, List<Subscription>> tokenDict;
                if (!_map.TryGetValue(messageType, out tokenDict))
                    return;

                List<Subscription> list;
                if (!tokenDict.TryGetValue(tokenKey, out list))
                    return;

                list.RemoveAll(s => !s.IsAlive || s.MatchesRecipient(recipient));
                CleanupEmpty_NoLock(messageType, tokenKey, tokenDict, list);
            }
        }

        public void UnregisterAll(object recipient)
        {
            if (recipient == null) throw new ArgumentNullException(nameof(recipient));

            lock (_gate)
            {
                var messageTypes = _map.Keys.ToList();
                foreach (var mt in messageTypes)
                {
                    var tokenDict = _map[mt];
                    var tokens = tokenDict.Keys.ToList();

                    foreach (var tk in tokens)
                    {
                        var list = tokenDict[tk];
                        list.RemoveAll(s => !s.IsAlive || s.MatchesRecipient(recipient));

                        if (list.Count == 0)
                            tokenDict.Remove(tk);
                    }

                    if (tokenDict.Count == 0)
                        _map.Remove(mt);
                }
            }
        }

        public void Reset()
        {
            lock (_gate)
            {
                _map.Clear();
            }
        }

        public void Send<TMessage>(TMessage message, object token = null)
        {
            var messageType = typeof(TMessage);
            var tokenKey = NormalizeToken(token);

            Subscription[] snapshot = null;

            lock (_gate)
            {
                Dictionary<object, List<Subscription>> tokenDict;
                if (!_map.TryGetValue(messageType, out tokenDict))
                    return;

                List<Subscription> list;
                if (!tokenDict.TryGetValue(tokenKey, out list))
                    return;

                if (list.Count == 0)
                    return;

                snapshot = list.ToArray();
            }

            var needCleanup = false;

            for (int i = 0; i < snapshot.Length; i++)
            {
                var sub = snapshot[i];
                if (!sub.IsAlive)
                {
                    needCleanup = true;
                    continue;
                }

                sub.Invoke(message);
            }

            if (needCleanup)
                Cleanup();
        }

        public void Cleanup()
        {
            lock (_gate)
            {
                var messageTypes = _map.Keys.ToList();
                foreach (var mt in messageTypes)
                {
                    var tokenDict = _map[mt];
                    var tokens = tokenDict.Keys.ToList();

                    foreach (var tk in tokens)
                    {
                        var list = tokenDict[tk];
                        list.RemoveAll(s => !s.IsAlive);

                        if (list.Count == 0)
                            tokenDict.Remove(tk);
                    }

                    if (tokenDict.Count == 0)
                        _map.Remove(mt);
                }
            }
        }

        private Dictionary<object, List<Subscription>> GetOrCreateTokenDict(Type messageType)
        {
            Dictionary<object, List<Subscription>> tokenDict;
            if (!_map.TryGetValue(messageType, out tokenDict))
            {
                tokenDict = new Dictionary<object, List<Subscription>>();
                _map[messageType] = tokenDict;
            }
            return tokenDict;
        }

        private static List<Subscription> GetOrCreateList(Dictionary<object, List<Subscription>> tokenDict, object tokenKey)
        {
            List<Subscription> list;
            if (!tokenDict.TryGetValue(tokenKey, out list))
            {
                list = new List<Subscription>();
                tokenDict[tokenKey] = list;
            }
            return list;
        }

        private void CleanupEmpty_NoLock(Type messageType, object tokenKey, Dictionary<object, List<Subscription>> tokenDict, List<Subscription> list)
        {
            if (list.Count == 0)
                tokenDict.Remove(tokenKey);

            if (tokenDict.Count == 0)
                _map.Remove(messageType);
        }

        private abstract class Subscription
        {
            public abstract bool IsAlive { get; }
            public abstract bool MatchesRecipient(object recipient);
            public abstract void Invoke(object message);
        }

        private sealed class RecipientSubscription<TMessage> : Subscription
        {
            private readonly WeakReference _recipientRef;

            public RecipientSubscription(IRecipient<TMessage> recipient)
            {
                _recipientRef = new WeakReference(recipient);
            }

            public override bool IsAlive
            {
                get { return _recipientRef.IsAlive && _recipientRef.Target != null; }
            }

            public override bool MatchesRecipient(object recipient)
            {
                return ReferenceEquals(_recipientRef.Target, recipient);
            }

            public override void Invoke(object message)
            {
                var target = _recipientRef.Target as IRecipient<TMessage>;
                if (target == null) return;

                target.Receive((TMessage)message);
            }
        }

        private sealed class HandlerSubscription<TRecipient, TMessage> : Subscription
            where TRecipient : class
        {
            private readonly WeakReference _recipientRef;
            private readonly WeakReference _handlerTargetRef; // 캡처 람다면 존재
            private readonly MethodInfo _method;

            public HandlerSubscription(TRecipient recipient, Action<TRecipient, TMessage> handler)
            {
                _recipientRef = new WeakReference(recipient);
                _method = handler.Method;

                if (handler.Target != null)
                    _handlerTargetRef = new WeakReference(handler.Target);
            }

            public override bool IsAlive
            {
                get
                {
                    if (!_recipientRef.IsAlive || _recipientRef.Target == null) return false;

                    if (_handlerTargetRef != null)
                        return _handlerTargetRef.IsAlive && _handlerTargetRef.Target != null;

                    return true;
                }
            }

            public override bool MatchesRecipient(object recipient)
            {
                return ReferenceEquals(_recipientRef.Target, recipient);
            }

            public override void Invoke(object message)
            {
                var recipient = _recipientRef.Target as TRecipient;
                if (recipient == null) return;

                object handlerTarget = null;
                if (_handlerTargetRef != null)
                {
                    handlerTarget = _handlerTargetRef.Target;
                    if (handlerTarget == null) return;
                }

                _method.Invoke(handlerTarget, new object[] { recipient, (TMessage)message });
            }
        }
    }
}
