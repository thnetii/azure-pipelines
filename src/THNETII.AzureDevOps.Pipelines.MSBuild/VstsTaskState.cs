using System.Runtime.Serialization;

namespace THNETII.AzureDevOps.Pipelines.MSBuild
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
