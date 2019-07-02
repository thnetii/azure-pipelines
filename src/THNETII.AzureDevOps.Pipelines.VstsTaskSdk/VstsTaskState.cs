using System.Runtime.Serialization;

namespace THNETII.AzureDevOps.Pipelines.VstsTaskSdk
{
    public enum VstsTaskState
    {
        [EnumMember(Value = "")]
        Unspecified = default,
        Unknown = -1,
        Initialized,
        InProgress,
        Completed,
    }
}
