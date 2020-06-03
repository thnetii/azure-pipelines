using System.Runtime.Serialization;

namespace THNETII.AzureDevOps.Pipelines.VstsTaskSdk
{
    public enum VstsTaskState
    {
        [EnumMember(Value = "")]
        Unspecified = default,
        Initialized,
        InProgress,
        Completed,
        Unknown = -1,
    }
}
