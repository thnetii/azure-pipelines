using System.Runtime.Serialization;

namespace THNETII.AzureDevOps.Pipelines.MSBuild
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
