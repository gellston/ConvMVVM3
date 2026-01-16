using ConvMVVM3.Core.DependencyInjection.Abstractions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ConvMVVM3.Host.DependencyInjection
{
    public static class ActivatorUtilities
    {
        #region Public Types
        public delegate object ObjectFactory(IServiceResolver resolver, object[] arguments);
        #endregion

        #region Public Functions
        public static object CreateInstance(IServiceResolver resolver, Type instanceType, params object[] arguments)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (instanceType == null) throw new ArgumentNullException(nameof(instanceType));

            arguments = arguments ?? new object[0];

            var selection = SelectBestConstructor(instanceType, arguments);

            if (selection == null)
                throw new InvalidOperationException("No suitable public constructor found for " + instanceType.FullName);

            var ctor = selection.Constructor;
            var parameterValues = BuildParameterValues(resolver, ctor, selection.ArgumentToParameterMap, arguments, instanceType);

            try
            {
                return ctor.Invoke(parameterValues);
            }
            catch (TargetInvocationException tie)
            {
                // unwrap
                throw tie.InnerException ?? tie;
            }
        }

        public static T CreateInstance<T>(IServiceResolver resolver, params object[] arguments)
        {
            return (T)CreateInstance(resolver, typeof(T), arguments);
        }

        public static object GetServiceOrCreateInstance(IServiceResolver resolver, Type instanceType)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (instanceType == null) throw new ArgumentNullException(nameof(instanceType));

            var existing = resolver.GetService(instanceType);
            if (existing != null) return existing;

            return CreateInstance(resolver, instanceType);
        }

        public static T GetServiceOrCreateInstance<T>(IServiceResolver resolver)
        {
            return (T)GetServiceOrCreateInstance(resolver, typeof(T));
        }

        /// <summary>
        /// 팩토리를 미리 생성 (Ctor 매칭/맵핑을 한 번 해두고, 이후 빠르게 생성)
        /// argumentTypes는 "사용자가 제공할 런타임 인자들의 타입" 목록.
        /// </summary>
        public static ObjectFactory CreateFactory(Type instanceType, Type[] argumentTypes)
        {
            if (instanceType == null) throw new ArgumentNullException(nameof(instanceType));
            argumentTypes = argumentTypes ?? new Type[0];

            // factory 생성 시점에 ctor를 고정 선택
            var selection = SelectBestConstructor(instanceType, argumentTypes);
            if (selection == null)
                throw new InvalidOperationException("No suitable public constructor found for " + instanceType.FullName);

            var ctor = selection.Constructor;
            var argToParam = selection.ArgumentToParameterMap;

            return (resolver, args) =>
            {
                if (resolver == null) throw new ArgumentNullException(nameof(resolver));
                args = args ?? new object[0];

                if (args.Length != argumentTypes.Length)
                    throw new ArgumentException("Argument count mismatch. Expected: " + argumentTypes.Length + ", Actual: " + args.Length);

                // 런타임 args가 argumentTypes와 호환되는지 검증
                for (int i = 0; i < argumentTypes.Length; i++)
                {
                    if (!IsCompatibleArgument(argumentTypes[i], args[i]))
                        throw new ArgumentException("Argument[" + i + "] is not compatible with " + argumentTypes[i].FullName);
                }

                var parameterValues = BuildParameterValues(resolver, ctor, argToParam, args, instanceType);

                try
                {
                    return ctor.Invoke(parameterValues);
                }
                catch (TargetInvocationException tie)
                {
                    throw tie.InnerException ?? tie;
                }
            };
        }
        #endregion

        #region Private Functions (Constructor Selection)
        private static ConstructorSelection SelectBestConstructor(Type instanceType, object[] arguments)
        {
            var argTypes = new Type[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
                argTypes[i] = arguments[i] == null ? null : arguments[i].GetType();

            return SelectBestConstructor(instanceType, argTypes);
        }

        private static ConstructorSelection SelectBestConstructor(Type instanceType, Type[] argumentTypes)
        {
            var ctors = instanceType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (ctors.Length == 0) return null;

            ConstructorSelection best = null;

            for (int i = 0; i < ctors.Length; i++)
            {
                var ctor = ctors[i];
                var parameters = ctor.GetParameters();

                // map: argIndex -> paramIndex (각 arg는 최대 1개 param에 매칭)
                int[] map;
                if (!TryBuildArgumentToParameterMap(parameters, argumentTypes, out map))
                    continue;

                // 스코어: "총 파라미터 수" 우선(더 많은 의존성/옵션을 커버하는 ctor 선호)
                // 타이브레이커: 런타임 인자 매칭 개수
                // 그 다음: 파라미터 수가 같으면 deterministic(메타데이터 토큰)
                int paramCount = parameters.Length;
                int matchedArgs = CountMappedArgs(map);

                var candidate = new ConstructorSelection(ctor, map, paramCount, matchedArgs);

                if (best == null)
                {
                    best = candidate;
                    continue;
                }

                if (candidate.ParameterCount > best.ParameterCount)
                {
                    best = candidate;
                    continue;
                }

                if (candidate.ParameterCount == best.ParameterCount && candidate.MatchedArgumentCount > best.MatchedArgumentCount)
                {
                    best = candidate;
                    continue;
                }

                if (candidate.ParameterCount == best.ParameterCount &&
                    candidate.MatchedArgumentCount == best.MatchedArgumentCount &&
                    candidate.Constructor.MetadataToken < best.Constructor.MetadataToken)
                {
                    best = candidate;
                }
            }

            return best;
        }

        private static bool TryBuildArgumentToParameterMap(ParameterInfo[] parameters, Type[] argumentTypes, out int[] argToParamMap)
        {
            argToParamMap = new int[argumentTypes.Length];
            for (int i = 0; i < argToParamMap.Length; i++) argToParamMap[i] = -1;

            // paramIndex 사용 여부
            var usedParam = new bool[parameters.Length];

            // 각 argument를 "들어갈 수 있는 param"에 1:1로 할당
            // 간단한 그리디(대부분 케이스 충분). 더 복잡한 최적 매칭이 필요하면 나중에 개선 가능.
            for (int argIndex = 0; argIndex < argumentTypes.Length; argIndex++)
            {
                var argType = argumentTypes[argIndex];

                int chosenParam = -1;

                for (int p = 0; p < parameters.Length; p++)
                {
                    if (usedParam[p]) continue;

                    var paramType = parameters[p].ParameterType;
                    if (IsCompatibleType(paramType, argType))
                    {
                        chosenParam = p;
                        break;
                    }
                }

                if (chosenParam < 0)
                    return false;

                usedParam[chosenParam] = true;
                argToParamMap[argIndex] = chosenParam;
            }

            return true;
        }

        private static int CountMappedArgs(int[] map)
        {
            int c = 0;
            for (int i = 0; i < map.Length; i++)
                if (map[i] >= 0) c++;
            return c;
        }
        #endregion

        #region Private Functions (Create Parameters)
        private static object[] BuildParameterValues(
            IServiceResolver resolver,
            ConstructorInfo ctor,
            int[] argToParamMap,
            object[] arguments,
            Type instanceTypeForError)
        {
            var ps = ctor.GetParameters();
            var values = new object[ps.Length];

            // 1) 런타임 인자 배치
            for (int argIndex = 0; argIndex < argToParamMap.Length; argIndex++)
            {
                int paramIndex = argToParamMap[argIndex];
                if (paramIndex < 0) continue;

                values[paramIndex] = arguments[argIndex];
            }

            // 2) 나머지는 DI에서 주입(or optional default)
            for (int p = 0; p < ps.Length; p++)
            {
                // 이미 런타임 인자가 채웠으면 skip
                if (HasValueAssigned(values[p], ps[p].ParameterType)) continue;

                var param = ps[p];
                var paramType = param.ParameterType;

                // DI에서 Resolve (필수)
                var service = resolver.GetService(paramType);
                if (service != null)
                {
                    values[p] = service;
                    continue;
                }

                // optional/default value 지원
                if (param.HasDefaultValue)
                {
                    var dv = param.DefaultValue;
                    if (dv == null && paramType.IsValueType && Nullable.GetUnderlyingType(paramType) == null)
                        values[p] = CreateDefaultValue(paramType);
                    else
                        values[p] = dv;

                    continue;
                }

                // 실패
                throw new InvalidOperationException(
                    "ActivatorUtilities failed to create " + instanceTypeForError.FullName +
                    ". Cannot resolve parameter '" + param.Name + "' (" + paramType.FullName + ").");
            }

            return values;
        }

        private static bool HasValueAssigned(object value, Type paramType)
        {
            if (value != null) return true;

            // null이 들어가도 되는 타입이면 "할당됨"으로 취급할지?
            // 여기서는 "런타임 인자가 null로 지정된 경우"도 허용해야 하므로
            // null이 허용되는 타입일 경우엔 값이 null이어도 할당된 것으로 보지 않고,
            // 런타임 인자 배치 단계에서 이미 null을 넣었는지를 구분해야 하는데
            // 단순 구현에서는 values[p]==null이면 미할당으로 간주.
            // (런타임에서 null 주입이 꼭 필요하면, argumentTypes로 CreateFactory를 쓰는 쪽이 안전)
            return false;
        }

        private static bool IsCompatibleType(Type parameterType, Type argumentType)
        {
            // argType이 null이면 (실제 arg가 null이거나) -> parameter가 null 허용이면 OK
            if (argumentType == null)
                return IsNullableParameter(parameterType);

            return parameterType.IsAssignableFrom(argumentType);
        }

        private static bool IsCompatibleArgument(Type expectedType, object arg)
        {
            if (arg == null)
                return IsNullableParameter(expectedType);

            return expectedType.IsAssignableFrom(arg.GetType());
        }

        private static bool IsNullableParameter(Type t)
        {
            if (!t.IsValueType) return true; // reference type
            return Nullable.GetUnderlyingType(t) != null; // Nullable<T>
        }

        private static object CreateDefaultValue(Type t)
        {
            // value type 기본값
            return Activator.CreateInstance(t);
        }
        #endregion

        #region Private Types
        private sealed class ConstructorSelection
        {
            public readonly ConstructorInfo Constructor;
            public readonly int[] ArgumentToParameterMap;
            public readonly int ParameterCount;
            public readonly int MatchedArgumentCount;

            public ConstructorSelection(ConstructorInfo ctor, int[] map, int paramCount, int matchedArgCount)
            {
                Constructor = ctor;
                ArgumentToParameterMap = map;
                ParameterCount = paramCount;
                MatchedArgumentCount = matchedArgCount;
            }
        }
        #endregion
    }
}
