#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"
godot_version="$(tr -d '\r\n' < scripts/godot-version.txt)"

if [[ -z "${GODOT_BIN:-}" ]]; then
  tool_dir="$repo_root/.cache/godot-$godot_version"
  mkdir -p "$tool_dir"
  export XDG_DATA_HOME="$tool_dir/data"
  template_dir="$XDG_DATA_HOME/godot/export_templates/$godot_version.stable"
  export GODOT_BIN="$tool_dir/Godot_v$godot_version-stable_linux.x86_64"
  release_url="https://github.com/godotengine/godot-builds/releases/download/$godot_version-stable"

  if [[ ! -f "$GODOT_BIN" ]]; then
    curl --fail --location --retry 3 "$release_url/Godot_v$godot_version-stable_linux.x86_64.zip" -o "$tool_dir/editor.zip"
    unzip -o "$tool_dir/editor.zip" -d "$tool_dir"
    chmod +x "$GODOT_BIN"
  fi

  if [[ ! -f "$template_dir/web_release.zip" ]]; then
    curl --fail --location --retry 3 "$release_url/Godot_v$godot_version-stable_export_templates.tpz" -o "$tool_dir/templates.tpz"
    mkdir -p "$template_dir"
    unzip -j -o "$tool_dir/templates.tpz" templates/web_release.zip -d "$template_dir"
  fi
fi

node scripts/build-web.mjs
