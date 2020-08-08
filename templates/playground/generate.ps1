$StepItems = 1..5 | ForEach-Object {
    [PSCustomObject]@{
        pwsh = "Write-Host `"Output from Step $_`"";
        displayName = "Generated Step $_"
    }
}
$ValueString = $StepItems | ConvertTo-Json -Compress
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
        'variable' = "items";
        'isOutput' = "true";
    } -AsOutput
Write-Host $command
