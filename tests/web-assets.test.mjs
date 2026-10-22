import assert from "node:assert/strict";
import { randomBytes } from "node:crypto";
import { mkdirSync, mkdtempSync, readFileSync, readdirSync, rmSync, writeFileSync } from "node:fs";
import path from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { runInNewContext } from "node:vm";
import { packageWebAssets } from "../scripts/package-web-assets.mjs";
import { readWebAsset } from "../scripts/check-web-build.mjs";

function fixture(t) {
  const cache = fileURLToPath(new URL("../.cache/", import.meta.url));
  mkdirSync(cache, { recursive: true });
  const directory = mkdtempSync(path.join(cache, "asset-test-"));
  t.after(() => {
    if (path.dirname(directory) === cache.replace(/[\\/]$/, "")) rmSync(directory, { recursive: true, force: true });
  });
  const original = {
    "index.wasm": Buffer.concat([Buffer.alloc(1_000_000, 65), randomBytes(1_000_000), Buffer.alloc(1_000_000)]),
    "index.pck": randomBytes(2_000_000),
  };
  for (const [file, bytes] of Object.entries(original)) writeFileSync(path.join(directory, file), bytes);
  writeFileSync(path.join(directory, "index.html"), '<script src="index.js"></script>');
  const manifest = packageWebAssets(directory);
  const loader = readdirSync(directory).find((file) => /^asset-loader\..*\.js$/.test(file));
  return { directory, original, manifest, source: readFileSync(path.join(directory, loader), "utf8") };
}

function installLoader(source, fetch) {
  const context = {
    URL, Response, ReadableStream, DecompressionStream, AbortController,
    location: { href: "https://game.example/game/index.html" },
    document: { currentScript: { src: "https://game.example/game/asset-loader.js" } },
    fetch,
  };
  runInNewContext(source, context);
  return context.fetch;
}

test("browser transport restores exact engine/pack bytes when chunks arrive out of order", async (t) => {
  const { directory, original, manifest, source } = fixture(t);
  assert.equal(manifest.files["index.wasm"].encoding, "gzip");
  assert.equal(manifest.files["index.pck"].encoding, "identity");
  assert(manifest.files["index.wasm"].parts.length > 1);
  let active = 0;
  let peak = 0;
  const received = [];
  const fetch = installLoader(source, async (input, options) => {
    const url = new URL(input);
    if (!url.pathname.includes("/assets/")) return new Response("passthrough");
    const file = url.pathname.slice("/game/".length);
    received.push(file);
    const part = Object.values(manifest.files).flatMap((entry) => entry.parts).find((entry) => entry.file === file);
    assert.equal(options.integrity, part.integrity);
    peak = Math.max(peak, ++active);
    await new Promise((resolve) => setTimeout(resolve, received.length === 1 ? 20 : 1));
    active--;
    return new Response(readFileSync(path.join(directory, file)));
  });
  for (const [file, bytes] of Object.entries(original)) {
    const response = await fetch(`https://game.example/game/${file}`);
    assert.equal(response.headers.get("content-length"), String(bytes.length));
    assert.equal(response.headers.get("content-type"), manifest.files[file].type);
    assert.deepEqual(Buffer.from(await response.arrayBuffer()), bytes);
    assert.deepEqual(readWebAsset(directory, manifest.files[file]), bytes);
  }
  assert(peak <= 4, "Chunk concurrency should stay bounded");
  assert.equal(await (await fetch("https://game.example/game/index.js")).text(), "passthrough");
  assert.equal(await (await fetch("https://game.example/game/index.wasm", { method: "HEAD" })).text(), "passthrough");
});

test("missing or corrupted chunks fail instead of starting an incomplete game", async (t) => {
  const { directory, manifest, source } = fixture(t);
  const fetch = installLoader(source, async () => new Response("Not found", { status: 404 }));
  const response = await fetch("https://game.example/game/index.pck");
  await assert.rejects(response.arrayBuffer(), /game download failed/);
  const entry = manifest.files["index.pck"];
  const file = path.join(directory, entry.parts[0].file);
  const corrupt = readFileSync(file);
  corrupt[0] ^= 1;
  writeFileSync(file, corrupt);
  assert.throws(() => readWebAsset(directory, entry), /does not match its contents/);
});
