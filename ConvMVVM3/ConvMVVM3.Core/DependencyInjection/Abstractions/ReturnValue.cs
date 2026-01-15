using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace ConvMVVM3.Core.DependencyInjection.Abstractions
{
    internal static class ReturnValue
    {
        public static T As<T>(object value)
        {
            if (value == null)
            {
                // T가 reference type이면 null 반환 가능하지만,
                // C# 7.3에서는 nullable annotation이 없으니 호출자가 주의.
                return default(T);
            }

            if (value is T) return (T)value;

            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            try
            {
// JsonElement 처리 제거 - 호환성 문제 해결
                // JsonElement는 System.Text.Json에 의존하므로
                // .NET Standard 2.0 환경에서 호환성 문제 발생 가능
                // 기본 형 변환만으로 단순화

                // 기본 형 변환
                if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(targetType))
                {
                    var converted = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                    return (T)converted;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(
                    "Cannot convert return value type '" + value.GetType().FullName +
                    "' to '" + typeof(T).FullName + "'.", ex);
            }

            throw new InvalidCastException(
                "Return value type '" + value.GetType().FullName +
                "' is not assignable to '" + typeof(T).FullName + "'.");
        }
    }
}
