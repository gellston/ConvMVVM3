using ConvMVVM3.Core.DependencyInjection.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConvMVVM3.Host.DependencyInjection
{
    public sealed class ServiceContainer : IServiceContainer
    {
        #region Private Property
        private readonly Dictionary<Type, List<ServiceDescriptor>> _map;

        private readonly Dictionary<ServiceDescriptor, object> _singletons = new Dictionary<ServiceDescriptor, object>();
        private readonly List<IDisposable> _rootDisposables = new List<IDisposable>();

        private bool _disposed;
        #endregion

        #region Public Functions
        public ServiceContainer(IServiceRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));

            // IServiceRegistry에는 Descriptors 프로퍼티가 없으므로
            // ServiceCollection을 IServiceRegistry로 캐스팅하여 사용
            if (registry is ServiceCollection serviceCollection)
            {
                _map = serviceCollection.Descriptors
                    .GroupBy(d => d.ServiceType)
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
            else
            {
                throw new ArgumentException("Registry must be ServiceCollection", nameof(registry));
            }
        }

        public IServiceScope CreateScope()
        {
            ThrowIfDisposed();
            return new ServiceScope(this);
        }

        public object GetRequiredService(Type serviceType)
        {
            ThrowIfDisposed();

            var v = Resolve(serviceType, scope: null, chain: new Stack<Type>(), required: true);
            if (v == null) // 방어
                throw new InvalidOperationException("Service not registered: " + serviceType.FullName);

            return v;
        }

        public T GetRequiredService<T>()
        {
            return (T)GetRequiredService(typeof(T));
        }

        public object GetService(Type serviceType)
        {
            ThrowIfDisposed();
            // 관례: 없으면 null
            return Resolve(serviceType, scope: null, chain: new Stack<Type>(), required: false);
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public IEnumerable<T> GetServices<T>()
        {
            // 관례: 0개면 빈 IEnumerable
            var obj = GetService(typeof(IEnumerable<T>));
            var typed = obj as IEnumerable<T>;
            return typed ?? (IEnumerable<T>)new List<T>();
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            var obj = GetService(enumerableType);

            var list = obj as IList;
            if (list == null) return new object[0];

            var result = new object[list.Count];
            for (int i = 0; i < list.Count; i++)
                result[i] = list[i];

            return result;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            for (int i = _rootDisposables.Count - 1; i >= 0; i--)
                _rootDisposables[i].Dispose();

            _rootDisposables.Clear();
            _singletons.Clear();
        }
        #endregion

        #region Private Functions
        internal object Resolve(Type serviceType, ServiceScope scope, Stack<Type> chain, bool required)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            // 0) Built-in self services (관례)
            if (serviceType == typeof(IServiceResolver))
                return scope == null ? (object)this : (object)scope;

            if (serviceType == typeof(IServiceContainer))
                return this;

            if (serviceType == typeof(IServiceScope))
            {
                if (scope == null)
                {
                    if (required)
                        throw new InvalidOperationException("IServiceScope is only available inside a scope. Path: " + FormatPath(chain));
                    return null;
                }
                return scope;
            }

            // 1) IEnumerable<T> : 등록 0개면 빈 리스트 반환
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var itemType = serviceType.GetGenericArguments()[0];
                return ResolveAll(itemType, scope, chain);
            }

            // 2) 등록 조회
            List<ServiceDescriptor> list;
            if (!_map.TryGetValue(serviceType, out list))
            {
                // 관례: interface/abstract는 optional이면 null
                if (serviceType.IsInterface || serviceType.IsAbstract)
                {
                    if (required)
                        throw new InvalidOperationException("Service not registered: " + serviceType.FullName + ". Path: " + FormatPath(chain));
                    return null;
                }

                // 옵션: concrete 타입은 등록 없어도 생성 시도
                return CreateInstance(serviceType, scope, chain);
            }

            // 3) 마지막 등록 우선
            var descriptor = list[list.Count - 1];

            try
            {
                return ResolveDescriptor(descriptor, scope, chain, required);
            }
            catch (Exception ex)
            {
                if (!required) throw;

                throw new InvalidOperationException(
                    "Failed to resolve service: " + serviceType.FullName + ". Path: " + FormatPath(chain),
                    ex);
            }
        }

        private object ResolveAll(Type itemType, ServiceScope scope, Stack<Type> chain)
        {
            // 관례: IEnumerable<T>는 항상 빈 컬렉션이라도 반환
            var listObj = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));

            List<ServiceDescriptor> descriptors;
            if (_map.TryGetValue(itemType, out descriptors))
            {
                for (int i = 0; i < descriptors.Count; i++)
                {
                    // 요소 생성 실패는 예외 (디버깅 쉬움)
                    listObj.Add(ResolveDescriptor(descriptors[i], scope, chain, required: true));
                }
            }

            return listObj;
        }

        private object ResolveDescriptor(ServiceDescriptor d, ServiceScope scope, Stack<Type> chain, bool required)
        {
            switch (d.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    // Singleton은 root에서 생성 (scoped 캡처 방지)
                    return GetOrCreateSingleton(d, () => CreateFromDescriptor(d, scope: null, chain: chain));

                case ServiceLifetime.Scoped:
                    if (scope == null)
                    {
                        if (!required) return null;
                        throw new InvalidOperationException("Cannot resolve scoped service from root. CreateScope() first. Path: " + FormatPath(chain));
                    }
                    return scope.GetOrCreateScoped(d, () => CreateFromDescriptor(d, scope, chain));

                default:
                    // Transient
                    return CreateFromDescriptor(d, scope, chain);
            }
        }

        private object CreateFromDescriptor(ServiceDescriptor d, ServiceScope scope, Stack<Type> chain)
        {
            object created;

            if (d.Factory != null)
            {
                created = d.Factory(scope == null ? (IServiceResolver)this : (IServiceResolver)scope);
            }
            else if (d.ImplementationType != null)
            {
                created = CreateInstance(d.ImplementationType, scope, chain);
            }
            else
            {
                throw new InvalidOperationException("Invalid registration for " + d.ServiceType.FullName + ". Path: " + FormatPath(chain));
            }

            // Dispose 트래킹
            var disp = created as IDisposable;
            if (disp != null)
            {
                if (scope == null) _rootDisposables.Add(disp);
                else scope.TrackDisposable(disp);
            }

            return created;
        }

        private object GetOrCreateSingleton(ServiceDescriptor d, Func<object> create)
        {
            object existing;
            if (_singletons.TryGetValue(d, out existing))
                return existing;

            var created = create();
            _singletons[d] = created;
            return created;
        }

        private object CreateInstance(Type implType, ServiceScope scope, Stack<Type> chain)
        {
            DetectCycle(implType, chain);

            chain.Push(implType);
            try
            {
                var ctor = SelectConstructor(implType);
                var ps = ctor.GetParameters();
                var args = new object[ps.Length];

                for (int i = 0; i < ps.Length; i++)
                {
                    var p = ps[i];

                    try
                    {
                        // ctor 파라미터는 required=true
                        var value = Resolve(p.ParameterType, scope, chain, required: true);
                        if (value == null)
                        {
                            throw new InvalidOperationException(
                                "Cannot resolve parameter '" + p.Name + "' (" + p.ParameterType.FullName + ") for " + implType.FullName +
                                ". Path: " + FormatPath(chain));
                        }

                        args[i] = value;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            "Cannot resolve parameter '" + p.Name + "' (" + p.ParameterType.FullName + ") for " + implType.FullName +
                            ". Path: " + FormatPath(chain),
                            ex);
                    }
                }

                try
                {
                    return ctor.Invoke(args);
                }
                catch (TargetInvocationException tie)
                {
                    var inner = tie.InnerException ?? tie;
                    throw new InvalidOperationException(
                        "Constructor threw while creating " + implType.FullName + ". Path: " + FormatPath(chain),
                        inner);
                }
            }
            finally
            {
                chain.Pop();
            }
        }

        private static ConstructorInfo SelectConstructor(Type t)
        {
            var ctors = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (ctors.Length == 0)
                throw new InvalidOperationException("No public constructors: " + t.FullName);

            // 가장 파라미터 많은 생성자 우선
            ConstructorInfo best = ctors[0];
            int bestCount = best.GetParameters().Length;

            for (int i = 1; i < ctors.Length; i++)
            {
                int c = ctors[i].GetParameters().Length;
                if (c > bestCount)
                {
                    best = ctors[i];
                    bestCount = c;
                }
            }

            return best;
        }

        private static void DetectCycle(Type t, Stack<Type> chain)
        {
            if (chain.Contains(t))
            {
                var path = string.Join(" -> ", chain.Reverse().Select(x => x.Name).Concat(new[] { t.Name }).ToArray());
                throw new InvalidOperationException("Circular dependency detected: " + path);
            }
        }

        private static string FormatPath(Stack<Type> chain)
        {
            if (chain == null || chain.Count == 0) return "<root>";
            return string.Join(" -> ", chain.Reverse().Select(t => t.Name).ToArray());
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ServiceContainer));
        }
        #endregion

        #region Nested Types
        internal sealed class ServiceScope : IServiceScope
        {
            #region Private Property
            private readonly ServiceContainer _root;

            private readonly Dictionary<ServiceDescriptor, object> _scoped = new Dictionary<ServiceDescriptor, object>();
            private readonly List<IDisposable> _disposables = new List<IDisposable>();

            private bool _disposed;
            #endregion

            #region Public Property
            public IServiceResolver ServiceProvider
            {
                get { return this; }
            }
            #endregion

            #region Public Functions
            public ServiceScope(ServiceContainer root)
            {
                if (root == null) throw new ArgumentNullException(nameof(root));
                _root = root;
            }

            public object GetRequiredService(Type serviceType)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(ServiceScope));

                var v = _root.Resolve(serviceType, this, new Stack<Type>(), required: true);
                if (v == null) throw new InvalidOperationException("Service not registered: " + serviceType.FullName);
                return v;
            }

            public T GetRequiredService<T>()
            {
                return (T)GetRequiredService(typeof(T));
            }

            public object GetService(Type serviceType)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(ServiceScope));
                return _root.Resolve(serviceType, this, new Stack<Type>(), required: false);
            }

            public T GetService<T>()
            {
                return (T)GetService(typeof(T));
            }

            public IEnumerable<T> GetServices<T>()
            {
                var obj = GetService(typeof(IEnumerable<T>));
                var typed = obj as IEnumerable<T>;
                return typed ?? (IEnumerable<T>)new List<T>();
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

                var enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);
                var obj = GetService(enumerableType);

                var list = obj as IList;
                if (list == null) return new object[0];

                var result = new object[list.Count];
                for (int i = 0; i < list.Count; i++)
                    result[i] = list[i];

                return result;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                for (int i = _disposables.Count - 1; i >= 0; i--)
                    _disposables[i].Dispose();

                _disposables.Clear();
                _scoped.Clear();
            }
            #endregion

            #region Internal Functions
            internal object GetOrCreateScoped(ServiceDescriptor d, Func<object> create)
            {
                object existing;
                if (_scoped.TryGetValue(d, out existing))
                    return existing;

                var created = create();
                _scoped[d] = created;
                return created;
            }

            internal void TrackDisposable(IDisposable disp)
            {
                if (disp != null) _disposables.Add(disp);
            }
            #endregion
        }
        #endregion
    }
}
