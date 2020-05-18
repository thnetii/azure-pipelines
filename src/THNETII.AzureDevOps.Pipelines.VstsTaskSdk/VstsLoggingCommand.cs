using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

using THNETII.AzureDevOps.Pipelines.VstsTaskSdk.Internal;
using THNETII.Common;
using THNETII.Common.Collections.Generic;
using THNETII.TypeConverter.Serialization;

namespace THNETII.AzureDevOps.Pipelines.VstsTaskSdk
{
    public static class VstsLoggingCommand
    {
        private const string loggingCommandPrefix = "##vso";
        private static readonly (string token, string replacement)[] loggingCommandEscapeMappings = new[]
        { // TODO: WHAT ABOUT "="? WHAT ABOUT "%"?
            (";", "%3B"),
            ("\r", "%0D"),
            ("\n", "%0A"),
            ("]", "%5D")
        };
        // TODO: BUG: Escape % ???
        // TODO: Add test to verify don't need to escape "=".

        /// <summary>
        /// Upload and attach attachment to current timeline record. These files
        /// are not available for download with logs. These can only be referred
        /// to by extensions using the type or name values.
        /// </summary>
        /// <param name="type">attachment type (Required)</param>
        /// <param name="name">attachment name (Required)</param>
        /// <param name="path" />
        public static string FormatTaskAddAttachment(string type, string name,
            string path) =>
            FormatLoggingCommand(area: "task", @event: "addattachment", path, new[]
                {
                    ("type", type).AsKeyValuePair(),
                    ("name", name).AsKeyValuePair()
                });

        /// <summary>
        /// Upload and attach summary markdown to current timeline record. This
        /// summary shall be added to the build/release summary and not available
        /// for download with logs.
        /// </summary>
        /// <param name="path"/>
        public static string FormatTaskUploadSummary(string path) =>
            FormatLoggingCommand(area: "task", @event: "uploadsummary", path);

        /// <summary>
        /// Set an endpoint field with given value. Value updated will be
        /// retained in the endpoint for the subsequent tasks that execute
        /// within the same job.
        /// </summary>
        /// <param name="id">endpoint id (Required)</param>
        /// <param name="field">field type (Required)</param>
        /// <param name="key">key (Required. Except for field=url)</param>
        /// <param name="value">value for key or url(Required)</param>
        public static string FormatTaskSetEndpoint(string id, string field,
            string key, string value) =>
            FormatLoggingCommand(area: "task", @event: "setendpoint", value, new[]
            {
                ("id", id).AsKeyValuePair(),
                ("field", field).AsKeyValuePair(),
                ("key", key).AsKeyValuePair()
            });

        /// <summary>
        /// Add a tag for current build.
        /// </summary>
        /// <param name="value"/>
        public static string FormatBuildAddBuildTag(string value) =>
            FormatLoggingCommand(area: "build", @event: "addbuildtag", value);

        /// <summary>
        /// Create an artifact link, artifact location is required to be a file
        /// container path, VC path or UNC share path.
        /// </summary>
        /// <param name="name">artifact name (Required)</param>
        /// <param name="path"/>
        /// <param name="type">artifact type (Required)</param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static string FormatArtifactAssociate(string name, string path,
            VstsArtifactType type,
            IEnumerable<KeyValuePair<string, string>> properties = default)
        {
            Dictionary<string, string> p =
                properties is null
                ? new Dictionary<string, string>(2, StringComparer.OrdinalIgnoreCase)
                : properties is IDictionary<string, string> propsDict
                ? new Dictionary<string, string>(propsDict, StringComparer.OrdinalIgnoreCase)
                : properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
            p["artifactname"] = name;
            p["artifacttype"] = EnumMemberStringConverter.ToString(type);
            return FormatLoggingCommand(area: "artifact", @event: "associate",
                path, p);
        }

        /// <summary>
        /// Create and update detail timeline records.
        /// </summary>
        /// <param name="id">Timeline record Guid (Required)</param>
        /// <param name="parentId">Parent timeline record Guid </param>
        /// <param name="type">Record type (Required for first time, can't overwrite)</param>
        /// <param name="name">Record name (Required for first time, can't overwrite)</param>
        /// <param name="order">order of timeline record (Required for first time, can't overwrite)</param>
        /// <param name="startTime">Datetime </param>
        /// <param name="finishTime">Datetime </param>
        /// <param name="progress">percentage of completion</param>
        /// <param name="state"/>
        /// <param name="result"/>
        /// <param name="message">current operation</param>
        /// <remarks>
        /// <para>
        /// The first time a log detail message is seen for a given task,
        /// a detailed timeline is created for that task.
        /// </para>
        /// <para>
        /// Nested timeline records are created and updated based on id and parentid.
        /// </para>
        /// <para>
        /// The task author needs to remember which Guid they used for each
        /// timeline record. The logging system tracks the Guid for each
        /// timeline record that has been created, so any new Guid results in a
        /// new timeline record.
        /// </para>
        /// </remarks>
        public static string FormatTaskLogDetail(Guid id, Guid? parentId = null,
            string type = null, string name = null, int? order = null,
            DateTime? startTime = null, DateTime? finishTime = null,
            int? progress = null, VstsTaskState state = VstsTaskState.Unspecified,
            VstsTaskResult result = VstsTaskResult.Unspecified, string message = null) =>
            FormatLoggingCommand(area: "task", @event: "logdetail", message, new[]
            {
                ("id", id.ToString(format: default, CultureInfo.InvariantCulture)).AsKeyValuePair(),
                ("parentid", parentId?.ToString(format: default, CultureInfo.InvariantCulture)).AsKeyValuePair(),
                ("type", type).AsKeyValuePair(),
                ("name", name).AsKeyValuePair(),
                ("order", order?.ToString(CultureInfo.InvariantCulture)).AsKeyValuePair(),
                ("starttime", startTime?.ToString("O", CultureInfo.InvariantCulture)).AsKeyValuePair(),
                ("finishtime", finishTime?.ToString("O", CultureInfo.InvariantCulture)).AsKeyValuePair(),
                ("progress", progress?.ToString(CultureInfo.InvariantCulture)).AsKeyValuePair(),
                ("state", state.ToEnumMemberString()).AsKeyValuePair(),
                ("result", result.ToEnumMemberString()).AsKeyValuePair()
            });


        /// <summary>
        /// Set progress and current operation for current task.
        /// </summary>
        /// <param name="percent">percentage of completion</param>
        /// <param name="currentOperation" />
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="percent"/> is less than <c>0</c> (zero) or greater than <c>100</c>.</exception>
        [SuppressMessage("Globalization", "CA1303: Do not pass literals as localized parameters")]
        public static string FormatTaskSetProgress(int percent, string currentOperation = null)
        {
            if (percent < 0 || percent > 100)
                throw new ArgumentOutOfRangeException(nameof(percent), percent,
                    "Argument value must be between 0 and 100.");
            return FormatLoggingCommand(area: "task", @event: "setprogress",
                currentOperation, new[]
                {
                    ("value", percent.ToString(CultureInfo.InvariantCulture)).AsKeyValuePair()
                });
        }

        /// <summary>
        /// Finish timeline record for current task, set task result and current
        /// operation. When result not specified, set result to succeeded.
        /// </summary>
        /// <param name="result"/>
        /// <param name="message"/>
        public static string FormatTaskSetResult(VstsTaskResult result,
            string message = null) =>
            FormatLoggingCommand(area: "task", @event: "complete", message, new[]
            {
                ("result", result.ToEnumMemberString()).AsKeyValuePair()
            });

        /// <summary />
        public static string FormatTaskSetSecret(string value) =>
            FormatLoggingCommand(area: "task", @event: "setsecret", value);

        /// <summary>
        /// Sets a variable in the variable service of taskcontext. The first
        /// task can set a variable, and following tasks in the same phase are
        /// able to use the variable. The variable is exposed to the following
        /// tasks as an environment variable. When <paramref name="isSecret"/>
        /// is set to <c>true</c>, the value of the variable will be saved as
        /// secret and masked out from log. Secret variables are not passed into
        /// tasks as environment variables and must be passed as inputs.
        /// </summary>
        public static string FormatTaskSetVariable(string name, string value = null,
            bool isSecret = false) =>
            FormatLoggingCommand(area: "task", @event: "setvariable", value, new[]
            {
                ("variable", name).AsKeyValuePair(),
                ("issecret", isSecret ? bool.TrueString : null).AsKeyValuePair()
            });

        /// <summary>
        /// Upload user interested file as additional log information to the
        /// current timeline record. The file shall be available for download
        /// along with task logs.
        /// </summary>
        public static string FormatTaskUploadFile(string path) =>
            FormatLoggingCommand(area: "task", @event: "uploadfile", path);

        /// <summary>
        /// Instruction for the agent to update the <c>PATH</c> environment variable.
        /// The specified directory is prepended to the <c>PATH</c>. The updated
        /// environment variable will be reflected in subsequent tasks.
        /// </summary>
        public static string FormatTaskPrependPath(string path) =>
            FormatLoggingCommand(area: "task", @event: "prependpath", path);

        /// <summary>
        /// Update build number for current build.
        /// </summary>
        public static string FormatBuildUpdateBuildNumber(string value) =>
            FormatLoggingCommand(area: "build", @event: "updatebuildnumber", value);

        /// <summary>
        /// Upload local file into a file container folder, create artifact if
        /// <paramref name="name"/> provided.
        /// </summary>
        /// <param name="containerFolder">folder that the file will upload to, folder will be created if needed. (Required)</param>
        /// <param name="name">artifact name</param>
        /// <param name="path"/>
        public static string FormatArtifactUpload(string containerFolder,
            string name, string path) =>
            FormatLoggingCommand(area: "artifact", @event: "upload", path, new[]
            {
                ("containerfolder", containerFolder).AsKeyValuePair(),
                ("artifactname", name).AsKeyValuePair()
            });

        /// <summary>
        /// Upload user interested log to build's container <c>"logs\tool"</c>
        /// folder.
        /// </summary>
        public static string FormatBuildUploadLog(string path) =>
            FormatLoggingCommand(area: "build", @event: "uploadlog", path);

        private static string FormatLoggingCommandData(string data, bool reverse = false)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;
            var replaceMappings = !reverse
                ? loggingCommandEscapeMappings
                : loggingCommandEscapeMappings.Select(m => (token: m.replacement, replacement: m.token));

            foreach (var (token, replacement) in replaceMappings)
            {
                data = data.Replace(token, replacement
                #if NETCOREAPP
                    , StringComparison.Ordinal
                #endif // NETCOREAPP
                    );
            }

            return data;
        }

        /// <summary>
        /// Update release name for current release.
        /// </summary>
        public static string FormatReleaseUpdateName(string name) =>
            FormatLoggingCommand(area: "release", @event: "updatereleasename", name);

        private static string FormatLoggingCommand(string area,
            string @event, string data = default,
            IEnumerable<KeyValuePair<string, string>> properties = default)
        {
            var builder = new StringBuilder();
            builder
                .Append(loggingCommandPrefix)
                .Append('[')
                .Append(area)
                .Append('.')
                .Append(@event)
                ;
            bool first = true;
            foreach (var kv in properties.EmptyIfNull())
            {
                if (FormatLoggingCommandData(kv.Value).TryNotNullOrWhiteSpace(out string value))
                {
                    if (first)
                    {
                        builder.Append(' ');
                        first = false;
                    }
                    else
                        builder.Append(';');
                    builder.Append(kv.Key).Append('=').Append(value);
                }
            }
            builder.Append(']');

            if (FormatLoggingCommandData(data).TryNotNullOrEmpty(out data))
                builder.Append(data);

            return builder.ToString();
        }

        /// <summary>
        /// Log error or warning issue to timeline record of current task.
        /// </summary>
        /// <param name="type">error or warning (Required)</param>
        /// <param name="message"/>
        /// <param name="errCode">error or warning code</param>
        /// <param name="sourcePath">source file location</param>
        /// <param name="lineNumber">line number</param>
        /// <param name="columnNumber">column number</param>
        public static string FormatTaskLogIssue(VstsTaskLogIssueType type,
            string message = null, string errCode = null,
            string sourcePath = null, int lineNumber = -1,
            int columnNumber = -1)
        {
            var (lineString, columnString) = GetLineAndColumnNumber();
            return FormatLoggingCommand(area: "task", @event: "logissue", message, new[]
            {
                ("type", type.ToEnumMemberString()).AsKeyValuePair(),
                ("code", errCode).AsKeyValuePair(),
                ("sourcepath", sourcePath).AsKeyValuePair(),
                ("linenumber", lineString).AsKeyValuePair(),
                ("columnnumber", columnString).AsKeyValuePair()
            });

            (string line, string column) GetLineAndColumnNumber()
            {
                if (lineNumber < 1)
                    return (null, null);
                string line = lineNumber.ToString(CultureInfo.InvariantCulture);
                if (columnNumber < 0)
                    return (line, null);
                return (line, columnNumber.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static string FormatTaskDebug(string message) =>
            FormatLoggingCommand(area: "task", @event: "debug", message);
    }
}
