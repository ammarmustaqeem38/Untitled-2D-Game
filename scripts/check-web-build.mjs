import assert from "node:assert/strict";
import { readFileSync, statSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

export function validateWebBuild(directory) {
  const html = readFileSync(path.join(directory, "index.html"), "utf8");
  const match = html.match(/const GODOT_CONFIG = (\{[^\n]+\});/);
  assert(match, "Missing Godot configuration in game/index.html");
  const config = JSON.parse(match[1]);
  assert.equal(config.executable, "index", "Godot export must be named index.html");
  assert.equal(config.canvasResizePolicy, 2, "Game must resize to its browser frame");
  assert(html.includes("const GODOT_THREADS_ENABLED = true;"), "Threaded audio support must be enabled");
  assert(config.fileSizes["index.wasm"] > 0, "Missing WebAssembly manifest entry");
  assert(config.fileSizes["index.pck"] > 0, "Missing game pack manifest entry");
  for (const [file, size] of Object.entries(config.fileSizes)) {
    assert(size > 0, `Empty export file: ${file}`);
    assert.equal(statSync(path.join(directory, file)).size, size, `Incomplete export file: ${file}`);
  }
  for (const file of ["index.js", "index.audio.worklet.js", "index.audio.position.worklet.js"]) {
    assert(statSync(path.join(directory, file)).size > 0, `Missing Godot runtime: ${file}`);
  }
  console.log("Verified Godot HTML, runtime, WebAssembly, and game pack.");
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  validateWebBuild(fileURLToPath(new URL("../docs/game/", import.meta.url)));
}
