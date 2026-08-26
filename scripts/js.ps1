# Windows twin of js.sh. Same logic: detect bun>pnpm>npm, install|build|watch.
param([Parameter(Mandatory)][ValidateSet("install","build","watch")][string]$Cmd)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Set-Location (Join-Path $PSScriptRoot "..\src\Simando.Web")

$PM = @("bun","pnpm","npm") | Where-Object { Get-Command $_ -ErrorAction SilentlyContinue } | Select-Object -First 1
if (-not $PM) {
    Write-Error "js.ps1: no JS package manager found (checked bun, pnpm, npm). Install one: bun.sh, pnpm.io, or nodejs.org (npm)."
    exit 1
}

if ($Cmd -eq "install") {
    & $PM install
    exit $LASTEXITCODE
}

if ($Cmd -eq "watch") {
    # Long-running; caller (scripts/dev.ps1) manages its lifetime.
    & $PM run watch:css
    exit $LASTEXITCODE
}

# build: installs first too, see js.sh comment on why (Restore hook unreliable on solution build)
& $PM install
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $PM run build:css
exit $LASTEXITCODE
