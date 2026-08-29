# Windows twin of dev.sh. One-command local dev loop: `dotnet watch` (ASP.NET Core Web API on port 5000) +
# Vite dev server (React SPA on port 3000), one terminal, Ctrl+C stops both.
#
# Ordering: the frontend dev server starts once the backend API is listening.
#
# Cleanup: `dotnet watch` and bun/vite each spawn descendant processes.
# Use `taskkill /T /F` to terminate each tree, mirroring dev.sh's explicit descendant walk.

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Set-Location (Join-Path $PSScriptRoot "..")

function Stop-Tree($proc) {
    if ($proc -and -not $proc.HasExited) {
        & taskkill /PID $proc.Id /T /F 2>$null | Out-Null
    }
}

Write-Host "scripts/dev.ps1: ensuring frontend dependencies are installed..."
powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\js.ps1" install

Write-Host "scripts/dev.ps1: starting ASP.NET Core Web API (src\Simando.Api)..."
$dotnetProc = Start-Process dotnet `
    -ArgumentList "watch", "run", "--project", "src\Simando.Api" `
    -PassThru -NoNewWindow

$frontendProc = $null

try {
    $port = 5000
    if (Test-Path "src\Simando.Api\Properties\launchSettings.json") {
        $launchSettings = Get-Content "src\Simando.Api\Properties\launchSettings.json" -Raw
        if ($launchSettings -match '"applicationUrl":\s*"http://localhost:(\d+)') {
            $port = [int]$Matches[1]
        }
    }

    Write-Host "scripts/dev.ps1: waiting for backend API on port $port before launching frontend..."
    while ($true) {
        if ($dotnetProc.HasExited) {
            Write-Error "scripts/dev.ps1: dotnet watch exited before the backend API came up."
            exit 1
        }
        $client = New-Object System.Net.Sockets.TcpClient
        try {
            $client.Connect("127.0.0.1", $port)
            if ($client.Connected) { $client.Close(); break }
        } catch {
            # not up yet
        } finally {
            $client.Dispose()
        }
        Start-Sleep -Milliseconds 500
    }

    Write-Host "scripts/dev.ps1: backend is ready on port $port. Starting frontend dev server (Vite)..."
    $frontendProc = Start-Process powershell `
        -ArgumentList "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", "scripts\js.ps1", "dev" `
        -PassThru -NoNewWindow

    while (-not $frontendProc.HasExited -and -not $dotnetProc.HasExited) {
        Start-Sleep -Seconds 1
    }
    if ($dotnetProc.HasExited) {
        Write-Warning "scripts/dev.ps1: backend API exited -- stopping frontend dev server too."
    } else {
        Write-Warning "scripts/dev.ps1: frontend dev server exited -- stopping backend API too."
    }
}
finally {
    Stop-Tree $dotnetProc
    Stop-Tree $frontendProc
}
