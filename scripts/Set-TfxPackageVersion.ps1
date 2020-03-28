$repoRoot = Join-Path $PSScriptRoot ".."
$srcRoot = Join-Path $repoRoot "src"
Get-ChildItem -File -Recurse -Filter "vss-extension.json" -Path $srcRoot |
ForEach-Object {
    $vssExtensionContent = $_ | Get-Content | ConvertFrom-Json
    $vssExtensionVersion = $vssExtensionContent.version
    [System.IO.DirectoryInfo]$vssDirectory = $_.Directory
    $dirPath = [System.IO.Path]::GetRelativePath($repoRoot, $vssDirectory.FullName)
    Write-Host "${dirPath}: version: $vssExtensionVersion"

    $subDirectoryStack = [System.Collections.Generic.Stack[string]]::new()
    $subDirectoryStack.Push($vssDirectory.FullName)
    $packageInstallDirStack = [System.Collections.Generic.Stack[string]]::new()
    while ($subDirectoryStack.Count -gt 0) {
        $currentDir = $subDirectoryStack.Pop()

        $packagePath = Join-Path $currentDir "package.json"
        if (Test-Path -PathType Leaf $packagePath) {
            $packageItem = Get-Item $packagePath
            $packageRelPath = [System.IO.Path]::GetRelativePath($vssDirectory.FullName, $packageItem.FullName)
            $packageJson = $packageItem | Get-Content | ConvertFrom-Json
            $packageVersion = $packageJson.version
            if ($packageVersion -ne $vssExtensionVersion) {
                $packageJson.version = $vssExtensionVersion
                $packageJson | ConvertTo-Json -Compress:$false | Out-File -FilePath $packagePath -Force
                $packageVersion = "$packageVersion -> $vssExtensionVersion"
            }
            Write-Host "`t${packageRelPath}: version: $packageVersion"

            $packageInstallDirStack.Push($packageItem.Directory.FullName)
        }

        Get-ChildItem -Directory -Path $currentDir |
        Where-Object -Property Name -NotLike "node_modules" |
        ForEach-Object {
            $subDirectoryStack.Push($_.FullName)
        }
    }

    if ($packageInstallDirStack.Count -gt 0) {
        Write-Host "`t$('-' * 10)"
    }
    while ($packageInstallDirStack.Count -gt 0) {
        $packageDir = $packageInstallDirStack.Pop()
        $packageRelPath = [System.IO.Path]::GetRelativePath($vssDirectory.FullName, $packageDir)
        Push-Location $packageDir
        Write-Host "`t${packageRelPath}: npm install"
        & npm install
        Pop-Location
    }
}
