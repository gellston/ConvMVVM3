using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.Core.Mvvm.Attributes;

namespace ConvMVVM3.Core.Mvvm.Modules
{
    public sealed class ModuleManager : IModuleManager
    {
        private readonly IServiceRegistry _registry;
        private readonly IServiceContainer _container;

        // name -> descriptor
        private readonly Dictionary<string, ModuleDescriptor> _modules =
            new Dictionary<string, ModuleDescriptor>(StringComparer.OrdinalIgnoreCase);

        // name -> factory (Activator or provided instance)
        private readonly Dictionary<string, Func<IModule>> _factories =
            new Dictionary<string, Func<IModule>>(StringComparer.OrdinalIgnoreCase);

        // block by module name
        private readonly HashSet<string> _blocked =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly object _gate = new object();

        public ModuleManager(IServiceRegistry registry, IServiceContainer container)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (container == null) throw new ArgumentNullException("container");
            _registry = registry;
            _container = container;
        }

        public IReadOnlyCollection<ModuleDescriptor> Modules
        {
            get
            {
                lock (_gate)
                {
                    return new ReadOnlyCollection<ModuleDescriptor>(_modules.Values.ToList());
                }
            }
        }

        public void AddBlockName(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName)) return;

            lock (_gate)
            {
                _blocked.Add(blockName);

                // 이미 발견된 게 있다면, "아직 로드 전"이면 제거하는 정책(선택)
                ModuleDescriptor existing;
                if (_modules.TryGetValue(blockName, out existing))
                {
                    if (existing.State == ModuleState.Discovered)
                    {
                        _modules.Remove(blockName);
                        _factories.Remove(blockName);
                    }
                }
            }
        }

        public void DiscoverFromDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("directory");
            if (!Directory.Exists(directory)) return;

            var dlls = Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly);

            foreach (var path in dlls)
            {
                Assembly asm;
                try
                {
                    asm = Assembly.LoadFrom(path);
                }
                catch
                {
                    // 로그 남기는 걸 추천
                    continue;
                }

                var asmVersion = SafeGetAssemblyVersion(asm);

                foreach (var t in GetLoadableTypes(asm))
                {
                    if (t == null || t.IsAbstract) continue;
                    if (!typeof(IModule).IsAssignableFrom(t)) continue;

                    var attr = (ModuleAttribute)Attribute.GetCustomAttribute(t, typeof(ModuleAttribute));
                    if (attr == null) continue;

                    var name = attr.Name;

                    lock (_gate)
                    {
                        if (_blocked.Contains(name)) continue;
                        if (_modules.ContainsKey(name)) continue; // 중복 방지(정책에 따라 예외 던져도 됨)

                        var descriptor = new ModuleDescriptor(
                            name: name,
                            moduleTypeName: t.AssemblyQualifiedName,
                            mode: attr.Mode,
                            dependsOn: attr.DependsOn,
                            assemblyPath: path,
                            version: asmVersion
                        );

                        _modules.Add(name, descriptor);
                        _factories.Add(name, () => (IModule)Activator.CreateInstance(t));
                    }
                }
            }
        }

        public void Add(IModule module)
        {
            if (module == null) throw new ArgumentNullException("module");

            var t = module.GetType();
            var attr = (ModuleAttribute)Attribute.GetCustomAttribute(t, typeof(ModuleAttribute));
            if (attr == null)
                throw new InvalidOperationException("ModuleAttribute is required for Add(IModule). Type: " + t.FullName);

            var name = attr.Name;
            var asmVersion = SafeGetAssemblyVersion(t.Assembly);

            lock (_gate)
            {
                if (_blocked.Contains(name))
                    return; // 또는 throw

                if (_modules.ContainsKey(name))
                    throw new InvalidOperationException("Duplicate module name: " + name);

                var descriptor = new ModuleDescriptor(
                    name: name,
                    moduleTypeName: t.AssemblyQualifiedName,
                    mode: attr.Mode,
                    dependsOn: attr.DependsOn,
                    assemblyPath: null,
                    version: asmVersion
                );

                _modules.Add(name, descriptor);
                _factories.Add(name, () => module); // 제공된 인스턴스 재사용
            }
        }

        public bool IsLoaded(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            lock (_gate)
            {
                ModuleDescriptor d;
                if (!_modules.TryGetValue(name, out d)) return false;
                return d.State == ModuleState.Initialized;
            }
        }

        public void InitializeStartupModules()
        {
            List<string> targets;
            lock (_gate)
            {
                targets = _modules.Values
                    .Where(m => m.Mode == InitializationMode.WhenAvailable && !_blocked.Contains(m.Name))
                    .Select(m => m.Name)
                    .ToList();
            }

            LoadPlan(targets);
        }

        public void LoadModule(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name");

            lock (_gate)
            {
                if (_blocked.Contains(name))
                    throw new InvalidOperationException("Blocked module: " + name);

                if (!_modules.ContainsKey(name))
                    throw new KeyNotFoundException("Module not found: " + name);
            }

            LoadPlan(new[] { name });
        }

        // -----------------------------
        // Core loading pipeline
        // -----------------------------
        private void LoadPlan(IEnumerable<string> targetNames)
        {
            // 1) closure 수집(의존성 포함)
            Dictionary<string, ModuleDescriptor> closure;
            lock (_gate)
            {
                closure = CollectClosure_NoLock(targetNames);
            }

            // 2) 위상정렬
            var ordered = TopologicalSort(closure);

            // 3) 이번 로드에서 새로 생성된 모듈 인스턴스(2-phase 위해 보관)
            var created = new List<Tuple<ModuleDescriptor, IModule>>();

            // 3-Phase A: RegisterTypes
            foreach (var d in ordered)
            {
                // 로드 차단 재확인
                lock (_gate)
                {
                    if (_blocked.Contains(d.Name))
                        throw new InvalidOperationException("Blocked module: " + d.Name);

                    // 이미 초기화된 경우 스킵
                    if (d.State == ModuleState.Initialized) continue;

                    // 이미 Register 까지 된 경우도 스킵(정책에 따라 재등록 금지)
                    if (d.State == ModuleState.Registered) continue;
                }

                IModule module = null;
                try
                {
                    module = CreateModuleInstance(d.Name);
                    module.RegisterTypes(_registry);

                    lock (_gate)
                    {
                        d.State = ModuleState.Registered;
                    }

                    created.Add(Tuple.Create(d, module));
                }
                catch
                {
                    lock (_gate) { d.State = ModuleState.Failed; }
                    throw;
                }
            }

            // 3-Phase B: OnInitialized
            foreach (var pair in created)
            {
                var d = pair.Item1;
                var module = pair.Item2;

                try
                {
                    module.OnInitialized(_container);

                    lock (_gate)
                    {
                        d.State = ModuleState.Initialized;
                    }
                }
                catch
                {
                    lock (_gate) { d.State = ModuleState.Failed; }
                    throw;
                }
            }
        }

        private IModule CreateModuleInstance(string name)
        {
            Func<IModule> factory;
            lock (_gate)
            {
                if (!_factories.TryGetValue(name, out factory))
                    throw new InvalidOperationException("No factory for module: " + name);
            }

            var module = factory();
            if (module == null)
                throw new InvalidOperationException("Factory returned null for module: " + name);

            return module;
        }

        private Dictionary<string, ModuleDescriptor> CollectClosure_NoLock(IEnumerable<string> targets)
        {
            var result = new Dictionary<string, ModuleDescriptor>(StringComparer.OrdinalIgnoreCase);
            var stack = new Stack<string>(targets);

            while (stack.Count > 0)
            {
                var name = stack.Pop();
                if (result.ContainsKey(name)) continue;

                if (_blocked.Contains(name))
                    throw new InvalidOperationException("Blocked dependency module: " + name);

                ModuleDescriptor info;
                if (!_modules.TryGetValue(name, out info))
                    throw new KeyNotFoundException("Module dependency not found: " + name);

                result.Add(name, info);

                var deps = info.DependsOn ?? new string[0];
                for (int i = 0; i < deps.Length; i++)
                    stack.Push(deps[i]);
            }

            return result;
        }

        private static List<ModuleDescriptor> TopologicalSort(Dictionary<string, ModuleDescriptor> nodes)
        {
            // Kahn
            var indeg = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var next = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in nodes)
            {
                indeg[kv.Key] = 0;
                next[kv.Key] = new List<string>();
            }

            foreach (var kv in nodes)
            {
                var m = kv.Value;
                var deps = m.DependsOn ?? new string[0];

                for (int i = 0; i < deps.Length; i++)
                {
                    var dep = deps[i];
                    if (!nodes.ContainsKey(dep)) continue;

                    indeg[m.Name] = indeg[m.Name] + 1;
                    next[dep].Add(m.Name);
                }
            }

            var q = new Queue<string>();
            foreach (var kv in indeg)
                if (kv.Value == 0) q.Enqueue(kv.Key);

            var orderedNames = new List<string>(nodes.Count);
            while (q.Count > 0)
            {
                var n = q.Dequeue();
                orderedNames.Add(n);

                var outs = next[n];
                for (int i = 0; i < outs.Count; i++)
                {
                    var to = outs[i];
                    indeg[to] = indeg[to] - 1;
                    if (indeg[to] == 0) q.Enqueue(to);
                }
            }

            if (orderedNames.Count != nodes.Count)
            {
                var remain = new List<string>();
                foreach (var kv in indeg)
                    if (kv.Value > 0) remain.Add(kv.Key);

                throw new InvalidOperationException("Cyclic module dependencies: " + string.Join(", ", remain.ToArray()));
            }

            var ordered = new List<ModuleDescriptor>(orderedNames.Count);
            for (int i = 0; i < orderedNames.Count; i++)
                ordered.Add(nodes[orderedNames[i]]);

            return ordered;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly asm)
        {
            try { return asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x != null);
            }
        }

        private static Version SafeGetAssemblyVersion(Assembly asm)
        {
            try
            {
                var v = asm.GetName().Version;
                return v ?? new Version(0, 0, 0, 0);
            }
            catch
            {
                return new Version(0, 0, 0, 0);
            }
        }
    }
}
