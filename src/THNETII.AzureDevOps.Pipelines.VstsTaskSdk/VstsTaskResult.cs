using System.Runtime.Serialization;

namespace THNETII.AzureDevOps.Pipelines.VstsTaskSdk
{
    public enum VstsTaskResult
    {
        [EnumMember(Value = "")]
        Unspecified = default,
        Succeeded,
        SucceededWithIssues,
        Failed,
        Cancelled,
        Skipped,
    }
}
