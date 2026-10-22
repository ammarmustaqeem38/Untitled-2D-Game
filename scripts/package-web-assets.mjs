import { createHash } from "node:crypto";
import { mkdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import path from "node:path";
import { gzipSync } from "node:zlib";

export const ASSET_PART_BYTES = 900_000;
const sha256 = (bytes, encoding = "hex") => createHash("sha256").update(bytes).digest(encoding);

export function packageWebAssets(directory) {
  const manifest = { version: 1, files: {} };
  mkdirSync(path.join(directory, "assets"));
  for (const [name, type] of [["index.wasm", "application/wasm"], ["index.pck", "application/octet-stream"]]) {
    const original = readFileSync(path.join(directory, name));
    const compressed = gzipSync(original, { level: 9 });
    // Already-compressed music/textures rarely benefit from another pass.
    const useGzip = compressed.length < original.length * 0.95;
    const payload = useGzip ? compressed : original;
    const parts = [];
    for (let offset = 0; offset < payload.length; offset += ASSET_PART_BYTES) {
      const bytes = payload.subarray(offset, offset + ASSET_PART_BYTES);
      const file = `assets/${sha256(bytes)}.bin`;
      writeFileSync(path.join(directory, file), bytes);
      parts.push({ file, size: bytes.length, integrity: `sha256-${sha256(bytes, "base64")}` });
    }
    manifest.files[name] = {
      type,
      encoding: useGzip ? "gzip" : "identity",
      size: original.length,
      downloadSize: payload.length,
      sha256: sha256(original),
      parts,
    };
    console.log(`${name}: ${(original.length / 1e6).toFixed(2)} MB -> ${(payload.length / 1e6).toFixed(2)} MB download in ${parts.length} cacheable parts`);
  }

  const source = readFileSync(new URL("./web-asset-loader.js", import.meta.url), "utf8");
  const loader = source.replace("__WEB_ASSET_MANIFEST__", JSON.stringify(manifest));
  const loaderName = `asset-loader.${sha256(loader).slice(0, 16)}.js`;
  writeFileSync(path.join(directory, loaderName), loader);
  writeFileSync(path.join(directory, "asset-manifest.json"), JSON.stringify(manifest, null, 2) + "\n");
  const htmlFile = path.join(directory, "index.html");
  const html = readFileSync(htmlFile, "utf8");
  const runtimeTag = '<script src="index.js"></script>';
  if (!html.includes(runtimeTag)) throw new Error("Cannot find the Godot runtime script in the exported HTML");
  writeFileSync(htmlFile, html.replace(runtimeTag, `<script src="${loaderName}"></script>\n\t\t${runtimeTag}`));

  // The native smoke test has already checked the raw pack. These large files
  // are reconstructed losslessly by the loader and are not published twice.
  for (const name of Object.keys(manifest.files)) rmSync(path.join(directory, name));
  return manifest;
}
