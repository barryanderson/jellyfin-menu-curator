#!/usr/bin/env bash
# Copies the Release build into a dev instance's plugins directory and restarts it.
# Usage: dev/install.sh official|lsio
set -euo pipefail

profile="${1:?usage: dev/install.sh official|lsio}"
here="$(cd "$(dirname "$0")" && pwd)"
version="$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$here/../Directory.Build.props")"
build="$here/../Jellyfin.Plugin.MenuCurator/bin/Release/net9.0"

# The two images keep plugins in different places under /config.
case "$profile" in
  official) plugins="$here/config-official/plugins" ;;
  lsio)     plugins="$here/config-lsio/data/plugins" ;;
  *) echo "unknown profile: $profile" >&2; exit 1 ;;
esac

[ -f "$build/Jellyfin.Plugin.MenuCurator.dll" ] || { echo "build first: dotnet build -c Release" >&2; exit 1; }

# The plugins directory belongs to the container's user; run with sudo if this fails.
rm -rf "$plugins"/MenuCurator_*
target="$plugins/MenuCurator_$version"
mkdir -p "$target"
cp "$build/Jellyfin.Plugin.MenuCurator.dll" "$target/"
echo "Installed to $target"

docker compose -f "$here/compose.yaml" --profile "$profile" restart
