# Windows twin of js.sh. Same logic: detect bun>pnpm>npm, install|build|dev|check|test|codegen.
param(
    [Parameter(Position=0)]
    [string]$Cmd = "install",
    [Parameter(ValueFromRemainingArguments=$true)]
    [string[]]$ArgsList
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Set-Location (Join-Path $PSScriptRoot "..\frontend")

$PM = @("bun","pnpm","npm") | Where-Object { Get-Command $_ -ErrorAction SilentlyContinue } | Select-Object -First 1
if (-not $PM) {
    Write-Error "js.ps1: no JS package manager found (checked bun, pnpm, npm). Install one: bun.sh, pnpm.io, or nodejs.org (npm)."
    exit 1
}

switch ($Cmd) {
    "install" {
        & $PM install
        exit $LASTEXITCODE
    }
    "build" {
        & $PM run build
        exit $LASTEXITCODE
    }
    "dev" {
        & $PM run dev
        exit $LASTEXITCODE
    }
    "check" {
        & $PM run check
        exit $LASTEXITCODE
    }
    "test" {
        & $PM run test
        exit $LASTEXITCODE
    }
    "test:watch" {
        & $PM run test:watch
        exit $LASTEXITCODE
    }
    "format" {
        & $PM run format
        exit $LASTEXITCODE
    }
    "lint" {
        & $PM run lint
        exit $LASTEXITCODE
    }
    "codegen" {
        & $PM run codegen
        exit $LASTEXITCODE
    }
    "generate-routes" {
        & $PM run generate-routes
        exit $LASTEXITCODE
    }
    default {
        if ($ArgsList) {
            & $PM $Cmd @ArgsList
        } else {
            & $PM $Cmd
        }
        exit $LASTEXITCODE
    }
}
