# Hosting on Netlify

## Deploy from Git

1. Push this repository, including `netlify.toml`, `scripts/`, and `docs/_headers`.
2. Import the repository into Netlify. Leave the base directory at the repository root.
3. Use build command `bash scripts/netlify-build.sh` and publish directory `docs` (both are configured in `netlify.toml`).
4. Deploy, then open the HTTPS site, click **Play**, and press **Enter**.

Netlify downloads the standard Godot editor and web export template for the version pinned in `scripts/godot-version.txt`, imports the game assets, exports all scenes and their dependencies, and checks the generated files before publishing. It also runs `tests/web-smoke.gd` against the exported game pack to check all rooms, movement, phone calls, objectives, dialogue choices, and music continuity. It needs no API keys, backend, npm packages, or .NET SDK. The initial build downloads the official export-template archive, which is approximately 1.3 GB; those tools are outside the published `docs` folder.

The website is served at `/`; the full game is at `/game/index.html`. Do not add an SPA catch-all redirect: missing game assets should return 404 instead of the website's HTML.

## Build locally or upload manually

Install the **standard, non-.NET Godot 4.7 editor** and matching export templates from **Editor > Manage Export Templates**. With Node.js 22 or newer:

```powershell
$env:GODOT_BIN = 'C:\path\to\Godot_v4.7-stable_win64_console.exe'
node scripts/build-web.mjs
node scripts/check-web-build.mjs
node scripts/serve-web.mjs
```

On Linux/macOS, set `GODOT_BIN` to the standard executable or put `godot` on your PATH. `bash scripts/netlify-build.sh` can install the Linux x86_64 editor/templates automatically, as it does on Netlify.

Open `http://localhost:8000`. After building, you can also drag the **entire `docs` folder**, including its `_headers` file and `game` subfolder, into Netlify's manual deploy area. Uploading only the repository sources or only the HTML file does not include the executable game. Manual uploads do not run the build command.

Generated `docs/game/` files and `.cache/` are ignored by Git. For Git-connected deployments, Netlify regenerates the game on every build. The C# files are retained in the source repository; every active scene uses its GDScript equivalent.

## Download size and caching

Always use `scripts/build-web.mjs` to prepare an upload. It performs these steps after the normal Godot export:

- Include every scene and its resource dependencies, excluding unused fonts and other unreferenced assets.
- Compress the engine with gzip without changing its bytes after decompression. Assets already compressed as MP3 or textures are kept as they are when another compression pass would not help.
- Split both binaries into parts no larger than 900,000 bytes under `game/assets/`. Their names contain a SHA-256 content hash, and `_headers` gives them one year of immutable browser caching. Unchanged parts can be reused on later visits and later deploys.
- Add a small loader that streams the parts back into Godot's normal startup process, validates each download with browser subresource integrity, and decompresses the engine with the browser's `DecompressionStream` API.

The raw `.wasm` and `.pck` files are deliberately absent from the upload folder. They are reconstructed losslessly from the parts. Keep the entire generated `game` folder together, including `asset-manifest.json`, the fingerprinted loader, and every file under `assets/`. The build verifies the reconstructed bytes against the original binaries before publishing. Tests, screenshots, editor downloads, and `.cache` are never published.

For the current project, the engine download is about 10.2 MB and the game pack about 23.0 MB, down from a combined 68.8 MB to 33.1 MB. Audio and image quality are unchanged. This reduces the required transfer; actual loading time still depends on the connection, CDN delivery, and device. The complete game pack is still loaded before play.

Do not add `Content-Encoding: br` to uncompressed files. A header does not compress data, and [Netlify normally ignores custom Content-Encoding headers](https://docs.netlify.com/manage/routing/headers/#limitations). This build's compression happens before upload and needs no special compression header from Netlify. HTML and stable runtime filenames retain default cache revalidation; only content-hashed files use immutable caching.

## Audio, input, and browser requirements

The Web preset uses threads and streamed audio to preserve the generated phone ringtone, positional audio, and audio playback positions across scenes. Godot's default sample playback does not support all of these features. Both the parent website and embedded game must be served with:

```text
Cross-Origin-Opener-Policy: same-origin
Cross-Origin-Embedder-Policy: require-corp
```

`docs/_headers` supplies these headers for Netlify, including manual uploads. The loader supplies the correct WebAssembly MIME type to Godot after decompression. Keep the iframe on the same origin as the site. The preview server supplies the same isolation and caching headers locally.

Use an up-to-date browser supporting WebGL 2, WebAssembly, SharedArrayBuffer, and `DecompressionStream` over HTTPS (localhost also works). The game retains its keyboard controls; a keyboard is needed. Press Enter or click inside the game if the browser waits for input before playing audio. A full-window game link is provided below the controls.

These settings follow the [Godot web export documentation](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_web.html) and [Netlify custom header documentation](https://docs.netlify.com/manage/routing/headers/).

## Before sharing a deploy

- Play from the title screen and confirm title music and the start sound.
- Move with the arrow keys, interact with E, and advance dialogue with X.
- Answer with F and decline with H; verify the ringtone and retry behavior.
- Visit the balcony, living room, Machi exterior/interior, and Amun's room.
- Collect Zyn, enter Mangi Baithak, try the X/C/V choices, and return; confirm music continues.
- Open the full-window link and confirm that controls and audio also work there.

If you see `SharedArrayBuffer` or cross-origin isolation errors, verify that `_headers` was published and that both `/` and `/game/index.html` have the two headers above. Do not use `file://` or Python's plain `http.server` for this threaded build. If a build fails, check that the standard editor and templates match the pinned version. Keep all generated filenames together and unchanged.
