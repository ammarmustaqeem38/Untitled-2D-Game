# The Good, The Bad, The Vago Website

This folder is a static, minimal game-portal website. It can be published directly with GitHub Pages.

## Godot Web Build

The active project scripts are now GDScript, so the project is no longer blocked by Godot 4 C# web export support.

Export the game to:

```text
docs/game/index.html
```

Recommended export flow:

1. Open the project in the standard Godot 4 editor.
2. Go to `Editor` -> `Manage Export Templates...` and install templates for your Godot version.
3. Go to `Project` -> `Export`.
4. Add a `Web` preset.
5. Set the export path to `docs/game/index.html`.
6. Export.
7. Test locally with:

```powershell
python -m http.server 8000 -d docs
```

Then open:

```text
http://localhost:8000
```

## GitHub Pages Setup

1. Push this repository to GitHub.
2. Open the repo on GitHub.
3. Go to `Settings` -> `Pages`.
4. Under `Build and deployment`, choose `Deploy from a branch`.
5. Pick your default branch and set the folder to `/docs`.
6. Save.
7. GitHub will publish the site at a `github.io` URL.

## If You Later Have a Web Build

Copy the exported web files into:

```text
docs/game/
```

The game's HTML file must be:

```text
docs/game/index.html
```

Then commit and push again. The Play button on the website will load it in the game frame.
