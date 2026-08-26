#!/usr/bin/env bash
# One-command local dev loop: `dotnet watch` (C#/Razor hot reload) + Tailwind
# watcher (CSS), one terminal, one Ctrl+C stops both.
#
# Ordering: the CSS watcher starts only once the app is actually listening,
# not at the same moment as `dotnet watch`. Starting both together races the
# watcher's first write against dotnet watch's own pre-build Tailwind step
# (MSBuild's SimandoTailwindBuild target) over the same wwwroot/app.css --
# see the DOTNET_WATCH guard on that target in Simando.Web.csproj, which
# exists specifically so the two don't fight once both are running.
#
# Cleanup: dotnet watch's descendants (dotnet-watch -> dotnet run -> the app
# itself) share this script's process group -- they are not an isolated
# group of their own, so a plain `kill` of just the top PID does not reach
# them, and they linger holding the port for the next run. Walk each job's
# full descendant tree explicitly instead. SIGINT first (mirrors Ctrl+C,
# gives dotnet-watch a chance to shut down cleanly), SIGKILL fallback for
# whatever survives.
#
# Requires bash (`wait -n`, process-tree walk); js.sh/js.ps1 stay POSIX sh.
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

scripts/js.sh install

dotnet watch run --project src/Simando.Web &
DOTNET_PID=$!
CSS_PID=""

cleanup() {
  kill_tree "$DOTNET_PID" INT
  [ -n "$CSS_PID" ] && kill_tree "$CSS_PID" INT
  sleep 1
  kill_tree "$DOTNET_PID" KILL
  [ -n "$CSS_PID" ] && kill_tree "$CSS_PID" KILL
}
trap cleanup EXIT INT TERM

PORT="$(grep -oP '"applicationUrl":\s*"http://localhost:\K[0-9]+' \
        src/Simando.Web/Properties/launchSettings.json | head -n1)"
PORT="${PORT:-5100}"

echo "scripts/dev.sh: waiting for the app on port $PORT before starting the CSS watcher..."
while ! (exec 3<>"/dev/tcp/127.0.0.1/$PORT") 2>/dev/null; do
  if ! kill -0 "$DOTNET_PID" 2>/dev/null; then
    echo "scripts/dev.sh: dotnet watch exited before the app came up." >&2
    exit 1
  fi
  sleep 0.5
done
exec 3<&- 3>&- 2>/dev/null || true

scripts/js.sh watch &
CSS_PID=$!

wait -n "$CSS_PID" "$DOTNET_PID" || true

if kill -0 "$DOTNET_PID" 2>/dev/null; then
  echo "scripts/dev.sh: Tailwind watcher exited unexpectedly -- stopping dotnet watch too." >&2
else
  echo "scripts/dev.sh: dotnet watch exited -- stopping the Tailwind watcher too." >&2
fi
