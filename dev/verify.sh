#!/usr/bin/env bash
# Hits /web/ConfigurationPages on the dev instance in each form the dashboard uses.
# Reads JELLYFIN_DEV_PORT and JELLYFIN_API_KEY from dev/.env.
set -euo pipefail

here="$(cd "$(dirname "$0")" && pwd)"
# shellcheck disable=SC1091
source "$here/.env"
: "${JELLYFIN_API_KEY:?set JELLYFIN_API_KEY in dev/.env}"
base="http://localhost:${JELLYFIN_DEV_PORT:-8097}/web/ConfigurationPages"
auth="Authorization: MediaBrowser Token=\"$JELLYFIN_API_KEY\""

show() {
  echo "== $1"
  shift
  curl -sS --fail-with-body -H "$auth" "$@"
  echo
}

show "Sidebar (rewritten)"           "$base?enableInMainMenu=true"
show "Full list (must be stock)"     "$base"
show "Not in menu (must be stock)"   "$base?enableInMainMenu=false"
show "camelCase profile (rewritten)" -H 'Accept: application/json; profile="CamelCase"' "$base?enableInMainMenu=true"
show "gzip/br (rewritten, decoded)"  --compressed -H 'Accept-Encoding: gzip, br' "$base?enableInMainMenu=true"
echo "== Content-Encoding with gzip, br"
curl -sS -o /dev/null -D - -H "$auth" -H 'Accept-Encoding: gzip, br' "$base?enableInMainMenu=true" | grep -i '^content-encoding' || echo "(none)"
