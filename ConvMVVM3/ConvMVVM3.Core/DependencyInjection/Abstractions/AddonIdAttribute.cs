using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.DependencyInjection.Abstractions
{
    /// <summary>
    /// Addon을 인스턴스 생성 없이 Id로 찾기 위한 메타데이터.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class AddonIdAttribute : Attribute
    {
        public string Id { get; }

        public AddonIdAttribute(string id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }
    }
}
