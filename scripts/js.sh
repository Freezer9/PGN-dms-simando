#!/bin/sh
# Frontend toolchain runner (frontend/).
# Usage: js.sh [install|build|dev|check|test|codegen|...]
# Picks first found: bun (primary, bun.lock committed) > pnpm > npm.
# Windows twin: js.ps1.

set -eu

cd "$(dirname "$0")/../frontend"

if command -v bun >/dev/null 2>&1; then
  PM=bun
elif command -v pnpm >/dev/null 2>&1; then
  PM=pnpm
elif command -v npm >/dev/null 2>&1; then
  PM=npm
else
  echo "js.sh: no JS package manager found (checked bun, pnpm, npm)." >&2
  echo "       Install one of these to run the frontend toolchain:" >&2
  echo "         bun  -- https://bun.sh" >&2
  echo "         pnpm -- https://pnpm.io" >&2
  echo "         npm  -- ships with Node.js, https://nodejs.org" >&2
  exit 1
fi

case "${1:-}" in
  ""|install)
    exec "$PM" install
    ;;
  build)
    exec "$PM" run build
    ;;
  dev)
    exec "$PM" run dev
    ;;
  check)
    exec "$PM" run check
    ;;
  test)
    exec "$PM" run test
    ;;
  test:watch)
    exec "$PM" run test:watch
    ;;
  format)
    exec "$PM" run format
    ;;
  lint)
    exec "$PM" run lint
    ;;
  codegen)
    exec "$PM" run codegen
    ;;
  generate-routes)
    exec "$PM" run generate-routes
    ;;
  *)
    exec "$PM" "$@"
    ;;
esac
