using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace THNETII.AzureDevOps.Pipelines.VstsTaskSdk.Internal
{
    internal static class EnumMemberStringExtensions
    {
        private static class EnumMemberValues<T> where T : struct, Enum
        {
            public static readonly Dictionary<T, string> StringValues = typeof(T)
                .GetFields(BindingFlags.Static)
                .Select(fi => (fi, enumMember: fi.GetCustomAttribute<EnumMemberAttribute>()))
                .Where(t => (t.enumMember?.IsValueSetExplicitly).HasValue)
                .ToDictionary(
                    t => (T)t.fi.GetValue(null),
                    t => t.enumMember.Value
                );
        }

        public static string ToEnumMemberString<T>(this T value)
            where T : struct, Enum
        {
            if (EnumMemberValues<T>.StringValues.TryGetValue(value, out string name))
                return name;
            return value.ToString();
        }
    }
}
