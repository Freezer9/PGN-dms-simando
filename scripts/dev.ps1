# Windows twin of dev.sh. One-command local dev loop: `dotnet watch` +
# Tailwind watcher, one terminal, Ctrl+C stops both. If either dies on its
# own, the other is stopped too -- see dev.sh for why that matters.
#
# Ordering: the CSS watcher starts only once the app is actually listening,
# not at the same moment as `dotnet watch`. See dev.sh's header comment for
# why -- the same wwwroot/app.css race applies on Windows too.
#
# Cleanup: `dotnet watch` and the JS package manager each spawn descendant
# processes (dotnet-watch -> dotnet run -> the app itself; PM -> tailwindcss)
# that Stop-Process does not reach -- Windows has no implicit process-group
# kill. Use `taskkill /T /F` to terminate each tree, mirroring dev.sh's
# explicit descendant walk.
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Set-Location (Join-Path $PSScriptRoot "..")

function Stop-Tree($proc) {
    if ($proc -and -not $proc.HasExited) {
        & taskkill /PID $proc.Id /T /F 2>$null | Out-Null
    }
}

powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\js.ps1" install

$dotnetProc = Start-Process dotnet `
    -ArgumentList "watch", "run", "--project", "src\Simando.Web" `
    -PassThru -NoNewWindow

$cssProc = $null

try {
    $port = 5100
    $launchSettings = Get-Content "src\Simando.Web\Properties\launchSettings.json" -Raw
    if ($launchSettings -match '"applicationUrl":\s*"http://localhost:(\d+)') {
        $port = [int]$Matches[1]
    }

    Write-Warning "scripts/dev.ps1: waiting for the app on port $port before starting the CSS watcher..."
    while ($true) {
        if ($dotnetProc.HasExited) {
            Write-Error "scripts/dev.ps1: dotnet watch exited before the app came up."
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

    $cssProc = Start-Process powershell `
        -ArgumentList "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", "scripts\js.ps1", "watch" `
        -PassThru -NoNewWindow

    while (-not $cssProc.HasExited -and -not $dotnetProc.HasExited) {
        Start-Sleep -Seconds 1
    }
    if ($dotnetProc.HasExited) {
        Write-Warning "dotnet watch exited -- stopping the Tailwind watcher too."
    } else {
        Write-Warning "Tailwind watcher exited unexpectedly -- stopping dotnet watch too."
    }
}
finally {
    Stop-Tree $dotnetProc
    Stop-Tree $cssProc
}
