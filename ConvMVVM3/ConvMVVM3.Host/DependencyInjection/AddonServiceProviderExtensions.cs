using ConvMVVM3.Core.DependencyInjection.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConvMVVM3.Host.DependencyInjection
{
    public static class AddonServiceProviderExtensions
    {
        /// <summary>
        /// Id로 Addon을 가져온다. 없으면 null.
        /// </summary>
        public static IAddon GetAddon(this IServiceContainer sp, string id)
        {
            if (sp == null) throw new ArgumentNullException(nameof(sp));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id is null/empty.", nameof(id));

            return FindSingleAddon(sp, id, required: false);
        }

        /// <summary>
        /// Id로 Addon을 가져온다. 없으면 예외.
        /// </summary>
        public static IAddon GetRequiredAddon(this IServiceContainer sp, string id)
        {
            if (sp == null) throw new ArgumentNullException(nameof(sp));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id is null/empty.", nameof(id));

            return FindSingleAddon(sp, id, required: true);
        }

        /// <summary>
        /// Id로 Addon을 가져온다. 없으면 false.
        /// </summary>
        public static bool TryGetAddon(this IServiceContainer sp, string id, out IAddon addon)
        {
            addon = null;

            if (sp == null) return false;
            if (string.IsNullOrWhiteSpace(id)) return false;

            var found = FindSingleAddon(sp, id, required: false);
            if (found == null) return false;

            addon = found;
            return true;
        }

        /// <summary>
        /// Id로 Addon을 가져오고 특정 타입으로 캐스팅. 없거나 타입이 다르면 예외.
        /// </summary>
        public static TAddon GetRequiredAddon<TAddon>(this IServiceContainer sp, string id)
            where TAddon : class, IAddon
        {
            var addon = GetRequiredAddon(sp, id);
            var typed = addon as TAddon;
            if (typed != null) return typed;

            throw new InvalidCastException(
                string.Format(
                    "Addon id '{0}' exists but is not '{1}'. Actual: '{2}'.",
                    id,
                    typeof(TAddon).FullName,
                    addon.GetType().FullName));
        }

        /// <summary>
        /// 시작 시점에 한 번 호출해서 Addon Id 중복을 잡아내기 좋음.
        /// </summary>
        public static void ValidateAddonIdsUnique(this IServiceContainer sp)
        {
            if (sp == null) throw new ArgumentNullException(nameof(sp));

            var addons = sp.GetServices<IAddon>();
            if (addons == null) return;

            var list = addons.ToList();
            if (list.Count == 0) return;

            var groups = list
                .Select(a => new AddonWithId(a, ResolveAddonId(a)))
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .ToList();

            if (groups.Count == 0) return;

            var msg = string.Join(Environment.NewLine, groups.Select(g =>
                string.Format("- '{0}': {1}", g.Key, string.Join(", ", g.Select(x => x.Addon.GetType().FullName)))));

            throw new InvalidOperationException("Duplicate Addon Id detected:" + Environment.NewLine + msg);
        }

        private static IAddon FindSingleAddon(IServiceContainer sp, string id, bool required)
        {
            var addons = sp.GetServices<IAddon>();
            if (addons == null) addons = Enumerable.Empty<IAddon>();

            var matches = addons
                .Select(a => new AddonWithId(a, ResolveAddonId(a)))
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .Where(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Addon)
                .ToList();

            if (matches.Count == 1)
                return matches[0];

            if (matches.Count > 1)
                throw new InvalidOperationException(
                    string.Format(
                        "Multiple IAddon found for id '{0}'. Types: {1}",
                        id,
                        string.Join(", ", matches.Select(m => m.GetType().FullName))));

            if (!required)
                return null;

            throw new KeyNotFoundException(
                string.Format(
                    "IAddon with id '{0}' not found. (Tip: ensure addon is registered as IAddon in DI)",
                    id));
        }

        private static string ResolveAddonId(IAddon addon)
        {
            // 1) 런타임 인스턴스의 Id 우선
            var id = addon.Id;
            if (!string.IsNullOrWhiteSpace(id)) return id;

            // 2) 보조: Attribute에서 읽기 (Id 구현 누락/레거시 대응)
            var attr = addon.GetType().GetCustomAttribute(typeof(AddonIdAttribute), inherit: false) as AddonIdAttribute;
            return attr != null ? attr.Id : null;
        }

        private sealed class AddonWithId
        {
            public readonly IAddon Addon;
            public readonly string Id;

            public AddonWithId(IAddon addon, string id)
            {
                Addon = addon;
                Id = id;
            }
        }
    }
}
