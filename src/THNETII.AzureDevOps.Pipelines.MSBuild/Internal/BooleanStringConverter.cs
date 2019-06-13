using System;
using System.Globalization;

namespace THNETII.AzureDevOps.Pipelines.MSBuild.Internal
{
    internal static class BooleanStringConverter
    {
        public static bool? ParseOrNull(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;
            if (string.Equals(s, bool.FalseString, StringComparison.OrdinalIgnoreCase))
                return false;
            if (string.Equals(s, bool.TrueString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (int.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out int intValue))
                return intValue != 0;
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out intValue))
                return intValue != 0;
            return null;
        }
    }
}
