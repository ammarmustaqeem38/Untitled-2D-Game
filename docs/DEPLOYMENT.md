# Hosting on Netlify

## Deploy from Git

1. Push this repository, including `netlify.toml`, `scripts/`, and `docs/_headers`.
2. Import the repository into Netlify. Leave the base directory at the repository root.
3. Use build command `bash scripts/netlify-build.sh` and publish directory `docs` (both are configured in `netlify.toml`).
4. Deploy, then open the HTTPS site, click **Play**, and press **Enter**.

Netlify downloads the standard Godot editor and web export template for the version pinned in `scripts/godot-version.txt`, imports every game asset, exports the `Web` preset, and checks the generated files before publishing. It also runs `tests/web-smoke.gd` against the exported game pack to check all rooms, movement, phone calls, objectives, dialogue choices, and music continuity. It needs no API keys, backend, npm packages, or .NET SDK. The initial build downloads the official export-template archive, which is approximately 1.3 GB.

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

## Audio, input, and browser requirements

The Web preset uses threads and streamed audio to preserve the generated phone ringtone, positional audio, and audio playback positions across scenes. Godot's default sample playback does not support all of these features. Both the parent website and embedded game must be served with:

```text
Cross-Origin-Opener-Policy: same-origin
Cross-Origin-Embedder-Policy: require-corp
```

`docs/_headers` supplies these headers for Netlify, including manual uploads. It also sets the WebAssembly MIME type and revalidates filenames shared between releases so old JavaScript cannot be combined with a new game pack. Keep the iframe on the same origin as the site. The preview server supplies the same headers locally.

Use an up-to-date browser supporting WebGL 2, WebAssembly, and SharedArrayBuffer over HTTPS (localhost also works). The game retains its keyboard controls; a keyboard is needed. Press Enter or click inside the game if the browser waits for input before playing audio. A full-window game link is provided below the controls.

These settings follow the [Godot web export documentation](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_web.html) and [Netlify custom header documentation](https://docs.netlify.com/manage/routing/headers/).

## Before sharing a deploy

- Play from the title screen and confirm title music and the start sound.
- Move with the arrow keys, interact with E, and advance dialogue with X.
- Answer with F and decline with H; verify the ringtone and retry behavior.
- Visit the balcony, living room, Machi exterior/interior, and Amun's room.
- Collect Zyn, enter Mangi Baithak, try the X/C/V choices, and return; confirm music continues.
- Open the full-window link and confirm that controls and audio also work there.

If you see `SharedArrayBuffer` or cross-origin isolation errors, verify that `_headers` was published and that both `/` and `/game/index.html` have the two headers above. Do not use `file://` or Python's plain `http.server` for this threaded build. If a build fails, check that the standard editor and templates match the pinned version. Keep all generated filenames together and unchanged.
