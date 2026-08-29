#!/usr/bin/env bash
# One-command local dev loop: `dotnet watch` (ASP.NET Core Web API on port 5000) +
# Vite dev server (React SPA on port 3000), one terminal, one Ctrl+C stops both.
#
# Ordering: the frontend dev server starts once the backend API is listening,
# ensuring the proxy targets are immediately reachable.
#
# Cleanup: dotnet watch and bun/vite spawn descendant processes that standard
# kill does not reach without walking the tree. Walk each job's descendant tree
# explicitly with SIGINT first, followed by SIGKILL for clean port release.
#
# Windows twin: dev.ps1.

set -euo pipefail

cd "$(dirname "$0")/.."

descendants() {
  local pid="$1" kids
  kids="$(pgrep -P "$pid" 2>/dev/null)" || return 0
  local k
  for k in $kids; do
    echo "$k"
    descendants "$k"
  done
}

kill_tree() {
  local root="$1" sig="$2" p
  for p in $(descendants "$root"); do
    kill -"$sig" "$p" 2>/dev/null || true
  done
  kill -"$sig" "$root" 2>/dev/null || true
}

echo "scripts/dev.sh: ensuring frontend dependencies are installed..."
scripts/js.sh install

echo "scripts/dev.sh: starting ASP.NET Core Web API (src/Simando.Api)..."
dotnet watch run --project src/Simando.Api &
DOTNET_PID=$!
FRONTEND_PID=""

cleanup() {
  echo "scripts/dev.sh: shutting down dev processes..."
  kill_tree "$DOTNET_PID" INT
  [ -n "$FRONTEND_PID" ] && kill_tree "$FRONTEND_PID" INT
  sleep 1
  kill_tree "$DOTNET_PID" KILL
  [ -n "$FRONTEND_PID" ] && kill_tree "$FRONTEND_PID" KILL
}
trap cleanup EXIT INT TERM

PORT="$(grep -oP '"applicationUrl":\s*"http://localhost:\K[0-9]+' \
        src/Simando.Api/Properties/launchSettings.json 2>/dev/null | head -n1)"
PORT="${PORT:-5000}"

echo "scripts/dev.sh: waiting for backend API on port $PORT before launching frontend..."
while ! (exec 3<>"/dev/tcp/127.0.0.1/$PORT") 2>/dev/null; do
  if ! kill -0 "$DOTNET_PID" 2>/dev/null; then
    echo "scripts/dev.sh: dotnet watch exited before the backend API came up." >&2
    exit 1
  fi
  sleep 0.5
done
exec 3<&- 3>&- 2>/dev/null || true

echo "scripts/dev.sh: backend is ready on port $PORT. Starting frontend dev server (Vite)..."
scripts/js.sh dev &
FRONTEND_PID=$!

wait -n "$FRONTEND_PID" "$DOTNET_PID" || true

if kill -0 "$DOTNET_PID" 2>/dev/null; then
  echo "scripts/dev.sh: frontend dev server exited -- stopping backend API too." >&2
else
  echo "scripts/dev.sh: backend API exited -- stopping frontend dev server too." >&2
fi
