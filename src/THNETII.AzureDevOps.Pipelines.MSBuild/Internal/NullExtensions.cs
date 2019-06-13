using System.Collections.Generic;
using System.Linq;

namespace THNETII.AzureDevOps.Pipelines.MSBuild.Internal
{
    internal static class NullExtensions
    {
        public static bool TryNotNull<T>(this T instance, out T notNull)
        {
            notNull = instance;
            return instance is T;
        }

        public static bool TryNotNullOrEmpty(this string str, out string notNullOrEmpty)
        {
            if (string.IsNullOrEmpty(str))
            {
                notNullOrEmpty = null;
                return false;
            }

            notNullOrEmpty = str;
            return true;
        }

        public static bool TryNotNullOrWhiteSpace(this string str, out string notNullOrEmpty)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                notNullOrEmpty = null;
                return false;
            }

            notNullOrEmpty = str;
            return true;
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> enumerable)
            => enumerable ?? Enumerable.Empty<T>();
    }
}
