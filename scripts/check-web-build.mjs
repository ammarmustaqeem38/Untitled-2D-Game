import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { existsSync, readFileSync, statSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { gunzipSync } from "node:zlib";
import { ASSET_PART_BYTES } from "./package-web-assets.mjs";

const sha256 = (bytes, encoding = "hex") => createHash("sha256").update(bytes).digest(encoding);

export function readWebAsset(directory, entry) {
  assert(entry.parts.length > 0, "Missing asset download parts");
  const parts = entry.parts.map((part) => {
    assert(/^assets\/[a-f0-9]{64}\.bin$/.test(part.file), "Invalid asset part path");
    assert(part.size > 0 && part.size <= ASSET_PART_BYTES, "Asset part exceeds the download size limit");
    const bytes = readFileSync(path.join(directory, part.file));
    assert.equal(bytes.length, part.size, `Incomplete asset part: ${part.file}`);
    assert.equal(part.file, `assets/${sha256(bytes)}.bin`, "Asset part filename does not match its contents");
    assert.equal(part.integrity, `sha256-${sha256(bytes, "base64")}`, "Asset integrity check failed");
    return bytes;
  });
  const payload = Buffer.concat(parts);
  assert.equal(payload.length, entry.downloadSize, "Incomplete asset download");
  assert(["gzip", "identity"].includes(entry.encoding), "Unsupported asset compression");
  const original = entry.encoding === "gzip" ? gunzipSync(payload) : payload;
  assert.equal(original.length, entry.size, "Decompressed asset size does not match");
  assert.equal(sha256(original), entry.sha256, "Decompressed asset content does not match");
  return original;
}

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
  const manifestFile = path.join(directory, "asset-manifest.json");
  if (existsSync(manifestFile)) {
    const manifest = JSON.parse(readFileSync(manifestFile, "utf8"));
    assert.equal(manifest.version, 1, "Unsupported asset manifest version");
    for (const file of ["index.wasm", "index.pck"]) {
      const bytes = readWebAsset(directory, manifest.files[file]);
      assert.equal(bytes.length, config.fileSizes[file], `Incorrect Godot manifest size: ${file}`);
      assert(!existsSync(path.join(directory, file)), "Raw binaries should not be duplicated in the upload folder");
    }
    const loader = html.match(/<script src="(asset-loader\.[a-f0-9]{16}\.js)"><\/script>/);
    assert(loader, "Missing compressed asset loader");
    const source = readFileSync(path.join(directory, loader[1]), "utf8");
    assert.equal(loader[1], `asset-loader.${sha256(source).slice(0, 16)}.js`, "Incorrect asset loader fingerprint");
    assert(source.includes(JSON.stringify(manifest)), "Asset loader belongs to another game build");
    assert(html.indexOf(loader[0]) < html.indexOf('<script src="index.js">'), "Asset loader must run before Godot");
  } else {
    for (const [file, size] of Object.entries(config.fileSizes)) {
      assert(size > 0, `Empty export file: ${file}`);
      assert.equal(statSync(path.join(directory, file)).size, size, `Incomplete export file: ${file}`);
    }
  }
  for (const file of ["index.js", "index.audio.worklet.js", "index.audio.position.worklet.js"]) {
    assert(statSync(path.join(directory, file)).size > 0, `Missing Godot runtime: ${file}`);
  }
  console.log("Verified Godot HTML, runtime, WebAssembly, and game pack.");
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  validateWebBuild(fileURLToPath(new URL("../docs/game/", import.meta.url)));
}
