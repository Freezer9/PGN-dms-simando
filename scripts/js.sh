#!/bin/sh
# Tailwind build runner (src/Simando.Web). Usage: js.sh install|build|watch
# Picks first found: bun (primary, bun.lock committed) > pnpm > npm
# (fallbacks re-resolve from package.json, no lockfile match -- fine for
# 2 tiny stable pkgs, not a real dep tree). Windows twin: js.ps1.

set -eu

cd "$(dirname "$0")/../src/Simando.Web"

if command -v bun >/dev/null 2>&1; then
  PM=bun
elif command -v pnpm >/dev/null 2>&1; then
  PM=pnpm
elif command -v npm >/dev/null 2>&1; then
  PM=npm
else
  echo "js.sh: no JS package manager found (checked bun, pnpm, npm)." >&2
  echo "       Install one of these to build the app's Tailwind styles:" >&2
  echo "         bun  -- https://bun.sh" >&2
  echo "         pnpm -- https://pnpm.io" >&2
  echo "         npm  -- ships with Node.js, https://nodejs.org" >&2
  exit 1
fi

case "${1:-}" in
  install)
    exec "$PM" install
    ;;
  build)
    # installs too: AfterTargets="Restore" hook skips on solution-wide
    # `dotnet build` (separate implicit-restore pass). Cheap when up to date.
    "$PM" install
    exec "$PM" run build:css
    ;;
  watch)
    # Long-running. Caller (scripts/dev.sh) manages its lifetime; not wired
    # into MSBuild -- a background process spawned from a build target does
    # not reliably survive `dotnet watch`'s own restart-on-rebuild cycle.
    exec "$PM" run watch:css
    ;;
  *)
    echo "js.sh: usage: js.sh install|build|watch" >&2
    exit 1
    ;;
esac
