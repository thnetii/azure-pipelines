using System;
using System.Collections.Generic;
using System.Text;

namespace THNETII.AzureDevOps.Pipelines.VstsTaskSdk.Internal
{
    internal static class KeyValuePairExtensions
    {
        public static KeyValuePair<TKey, TValue> AsKeyValuePair<TKey, TValue>(this (TKey key, TValue value) tuple)
            => new KeyValuePair<TKey, TValue>(tuple.key, tuple.value);
    }
}
