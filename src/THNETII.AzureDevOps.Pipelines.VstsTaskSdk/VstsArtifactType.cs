using System.Runtime.Serialization;

namespace THNETII.AzureDevOps.Pipelines.VstsTaskSdk
{
    public enum VstsArtifactType
    {
        [EnumMember(Value = "container")]
        Container,
        [EnumMember(Value = "filepath")]
        FilePath,
        [EnumMember(Value = "versioncontrol")]
        VersionControl,
        [EnumMember(Value = "gitref")]
        GitRef,
        [EnumMember(Value = "tfvclabel")]
        TfvcLabel
    }
}
