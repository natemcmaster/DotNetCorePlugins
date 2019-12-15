$ErrorActionPreference = 'Stop'

Push-Location $PSScriptRoot
try {
    $publish_dir = "$PSScriptRoot/bin/plugins/TimestampedPlugin/"

    function log {
        Write-Host -NoNewline -ForegroundColor Yellow "run.ps1: "
        Write-Host $args
    }

    function publish {
        Write-Host ""
        dotnet publish --no-restore TimestampedPlugin/ -o $publish_dir -nologo
        Write-Host ""
    }

    log "Compiling apps"

    & dotnet build HotReloadApp -nologo -clp:NoSummary
    & publish

    log "Use CTRL+C to exit"

    $bg_args = @("run", "--no-build", "--project", "HotReloadApp", "$publish_dir/TimestampedPlugin.dll")
    $host_process = Start-Process -NoNewWindow -FilePath dotnet -ArgumentList $bg_args
    try {
        while ($true) {
            Start-Sleep 5
            log "Rebuilding plugin..."
            publish
        }
    }
    finally {
        $host_process.Kill()
    }
}
finally {
    Pop-Location
}
