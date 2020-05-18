using System;

using THNETII.TypeConverter.Serialization;

namespace THNETII.AzureDevOps.Pipelines.VstsTaskSdk.Internal
{
    internal static class EnumMemberStringExtensions
    {
        public static string ToEnumMemberString<T>(this T value)
            where T : struct, Enum =>
            EnumMemberStringConverter.ToString(value);
    }
}
