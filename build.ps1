#!/usr/bin/env pwsh
[CmdletBinding(PositionalBinding = $false)]
param(
    [ValidateSet('Debug', 'Release')]
    $Configuration = $null,
    [switch]
    $ci,
    [switch]
    $sign,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$MSBuildArgs
)

Set-StrictMode -Version 1
$ErrorActionPreference = 'Stop'

Import-Module -Force -Scope Local "$PSScriptRoot/src/common.psm1"

#
# Main
#

if ($env:CI -eq 'true') {
    $ci = $true
}

if (!$Configuration) {
    $Configuration = if ($ci) { 'Release' } else { 'Debug' }
}

if ($ci) {
    $MSBuildArgs += '-p:CI=true'
}

$isPr = $env:APPVEYOR_PULL_REQUEST_HEAD_COMMIT -or ($env:BUILD_REASON -eq 'PullRequest')
if (-not (Test-Path variable:\IsCoreCLR)) {
    $IsWindows = $true
}

$CodeSign = $sign -or ($ci -and -not $isPr -and $IsWindows)
if ($CodeSign) {
    $toolsDir = "$PSScriptRoot/.build/tools"
    $AzureSignToolPath = "$toolsDir/azuresigntool"
    if ($IsWindows) {
        $AzureSignToolPath += ".exe"
    }

    if (-not (Test-Path $AzureSignToolPath)) {
        exec dotnet tool install --tool-path $toolsDir `
        AzureSignTool `
        --version 2.0.17
    }

    $nstDir = "$toolsDir/nugetsigntool/1.1.4"
    $NuGetKeyVaultSignToolPath = "$nstDir/tools/net471/NuGetKeyVaultSignTool.exe"
    if (-not (Test-Path $NuGetKeyVaultSignToolPath)) {
        New-Item $nstDir -ItemType Directory -ErrorAction Ignore | Out-Null
        Invoke-WebRequest https://github.com/onovotny/NuGetKeyVaultSignTool/releases/download/v1.1.4/NuGetKeyVaultSignTool.1.1.4.nupkg `
            -OutFile "$nstDir/NuGetKeyVaultSignTool.zip"
        Expand-Archive "$nstDir/NuGetKeyVaultSignTool.zip" -DestinationPath $nstDir
    }

    $MSBuildArgs += '-p:CodeSign=true'
    $MSBuildArgs += "-p:AzureSignToolPath=$AzureSignToolPath"
    $MSBuildArgs += "-p:NuGetKeyVaultSignToolPath=$NuGetKeyVaultSignToolPath"
}

$artifacts = "$PSScriptRoot/artifacts/"

Remove-Item -Recurse $artifacts -ErrorAction Ignore

exec dotnet build --configuration $Configuration '-warnaserror:CS1591' @MSBuildArgs
exec dotnet pack --no-restore --no-build --configuration $Configuration -o $artifacts @MSBuildArgs

[string[]] $testArgs=@()
if ($env:TF_BUILD) {
    $testArgs += '--logger', 'trx'
}

exec dotnet test --no-restore --no-build --configuration $Configuration '-clp:Summary' `
    "$PSScriptRoot/test/Plugins.Tests/McMaster.NETCore.Plugins.Tests.csproj" `
    @testArgs `
    @MSBuildArgs

write-host -f magenta 'Done'
