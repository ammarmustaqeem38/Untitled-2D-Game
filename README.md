# New Game Project

A Godot 4.7 2D narrative exploration prototype. The player controls Vago, moves between rooms, triggers dialogue, receives phone calls, follows objectives, and visits scenes around the apartment and Machi. All active scenes use GDScript; the earlier C# source is retained.

Content note: this project contains strong language and mature dialogue.

## Requirements

- Godot 4.7 **standard edition** and its matching export templates for web builds
- Node.js 22 or newer for the web build and preview scripts (no npm dependencies)
- Git, if you want to clone or version the project

The .NET edition can still be used for desktop development with the .NET 8 SDK. Godot's .NET editor cannot export to Web, even when the attached scripts are GDScript. The automated Netlify build installs the standard edition separately.

## Running the Project

1. Open Godot.
2. Import or open this folder by selecting `project.godot`.
3. Let Godot import the assets.
4. Press **Run Project** or `F5`.

The configured main scene is `TitleScreen.tscn`.

## Controls

| Input | Action |
| --- | --- |
| `Enter` | Start from the title screen |
| Arrow keys | Move Vago |
| `E` | Interact when a prompt is visible |
| `X` | Advance dialogue |
| `F` | Pick up an incoming phone call |
| `H` | Hang up an incoming phone call |
| `X`, `C`, or `V` | Return from the Mangi Baithak screen |

## Project Structure

| Path | Purpose |
| --- | --- |
| `project.godot` | Godot project settings, main scene, autoloads, and renderer settings |
| `New Game Project.csproj` / `*.cs` | Retained .NET project and earlier C# scripts |
| `TitleScreen.tscn` / `TitleScreen.gd` | Opening title screen and start input |
| `Player.tscn` / `Player.gd` | Persistent player autoload, movement, dialogue UI, phone calls, objectives, camera, and audio listener |
| `main.tscn` / `Main.gd` | Vago's room and the first objective interaction |
| `MachiInt.tscn`, `MachiExt.tscn`, `Livingroom.tscn`, `AmunsRoom.tscn`, `BalconyIdle.tscn`, `MangiBaithak.tscn` | Explorable rooms and special scenes |
| `Spritos/` | Sprite, texture, and icon assets |
| `Audio/` | Music and ambience assets |
| `docs/` | Browser page and Netlify response headers |
| `scripts/` | Reproducible web export, validation, and local preview |
| `netlify.toml` | Netlify build command and publish directory |

## Development Notes

- `Player.tscn` is registered as an autoload named `Player`, so room scripts access it through `/root/Player`.
- Room scenes generally move the persistent player to a local `Spawn` marker in `_ready()`.
- Scene transitions are handled by `Area2D` scripts such as `Entrance.gd`, `ExitDoor2.gd`, `GoToMachi.gd`, `GoToBalcony.gd`, and `GoToAmun.gd`.
- Dialogue, phone call behavior, and objective prompts are implemented in `Player.gd`.
- The project uses Godot's built-in `ui_up`, `ui_down`, `ui_left`, and `ui_right` actions for movement.

## Build Notes

Connect this repository to Netlify. The root `netlify.toml` sets the build command to `bash scripts/netlify-build.sh` and the publish directory to `docs`. Each deploy imports the assets and exports the complete game with the pinned standard Godot version in `scripts/godot-version.txt`.

For a local export in PowerShell, install standard Godot and its matching export templates, then run:

```powershell
$env:GODOT_BIN = 'C:\path\to\Godot_v4.7-stable_win64_console.exe'
node scripts/build-web.mjs
node scripts/serve-web.mjs
```

Open `http://localhost:8000`. The preview server sends the same cross-origin isolation headers as Netlify. These are required for the threaded export that preserves procedural ringtone generation, positional audio, and music continuity. A plain file URL or a server without these headers cannot run this build.

The exporter builds in an isolated directory under `.cache/`, includes every scene and its dependencies, and only replaces `docs/game/` after export validation and gameplay smoke checks pass. The checks load the exported pack and cover all rooms, movement, phone calls, objectives, dialogue choices, and audio continuity. Unused resources, .NET sources, tests, and website tooling are excluded from the game pack. The original game scripts, assets, and desktop project remain available.

The browser build uses lossless gzip compression for the engine and content-hashed download parts under 900 KB, with immutable caching for repeat visits. The browser loader reconstructs Godot's binaries without changing audio or image quality. The raw `.wasm` and `.pck` are not duplicated in the upload folder. Always deploy the whole generated `docs` folder. Run `node --test tests/web-assets.test.mjs` to check the transport's byte accuracy and failure handling.

See [deployment instructions](docs/DEPLOYMENT.md) for manual uploads and troubleshooting.

No license file is currently included. Add one before distributing the project publicly.
