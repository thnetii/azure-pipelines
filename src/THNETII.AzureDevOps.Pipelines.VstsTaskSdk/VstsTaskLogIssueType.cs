using System.Runtime.Serialization;

namespace THNETII.AzureDevOps.Pipelines.VstsTaskSdk
{
    public enum VstsTaskLogIssueType
    {
        [EnumMember(Value = "")]
        None = 0,
        [EnumMember(Value = "warning")]
        Warning,
        [EnumMember(Value = "error")]
        Error
    }
}
