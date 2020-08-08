$Matrix = [PSCustomObject]@{ }
1..5 | ForEach-Object {
    $Row = [PSCustomObject]@{
        "Playground.GeneratedJob.Echo" = "Output from Matrix row $_";
        "Playground.GeneratedJob.DisplayName" = "Row $_";
        "Playground.GeneratedJob.TaskDisplayName" = "Matrix Row $_ Output";
    }
    $Matrix | Add-Member NoteProperty $_ $Row
}
$ValueString = $Matrix | ConvertTo-Json -Compress
$LoggingCommandFile = Resolve-Path -ErrorAction "Continue" $ENV:LoggingCommandFunctions
if (-not $LoggingCommandFile) {
    $LoggingCommandFile = ([System.IO.Path]::Combine(
        $PSScriptRoot, "..", "..", "azure-pipelines-task-lib", "powershell",
        "VstsTaskSdk", "LoggingCommandFunctions.ps1"
    ))
    $LoggingCommandFile = Resolve-Path -ErrorAction "Stop" $LoggingCommandFile
}
. $LoggingCommandFile
$command = Write-LoggingCommand -Area 'task' -Event 'setvariable' `
    -Data $ValueString -Properties @{
        'variable' = "legs";
        'isOutput' = "true";
    } -AsOutput
Write-Host $command
