# New Game Project

A Godot 4.7 .NET 2D narrative exploration prototype. The player controls Vago, moves between rooms, triggers dialogue, receives phone calls, follows objectives, and visits scenes around the apartment and Machi.

Content note: this project contains strong language and mature dialogue.

## Requirements

- Godot 4.7 .NET edition
- .NET 8 SDK
- Git, if you want to clone or version the project

The project targets `net8.0` for desktop builds through `Godot.NET.Sdk/4.7.0`.

## Running the Project

1. Open Godot.
2. Import or open this folder by selecting `project.godot`.
3. Let Godot import the assets and build the C# project.
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
| `New Game Project.csproj` | C# project file for Godot .NET |
| `TitleScreen.tscn` / `TitleScreen.cs` | Opening title screen and start input |
| `Player.tscn` / `Player.cs` | Persistent player autoload, movement, dialogue UI, phone calls, objectives, camera, and audio listener |
| `main.tscn` / `Main.cs` | Vago's room and the first objective interaction |
| `MachiInt.tscn`, `MachiExt.tscn`, `Livingroom.tscn`, `AmunsRoom.tscn`, `BalconyIdle.tscn`, `MangiBaithak.tscn` | Explorable rooms and special scenes |
| `Spritos/` | Sprite, texture, and icon assets |
| `Audio/` | Music and ambience assets |

## Development Notes

- `Player.tscn` is registered as an autoload named `Player`, so room scripts access it through `/root/Player`.
- Room scenes generally move the persistent player to a local `Spawn` marker in `_Ready()`.
- Scene transitions are handled by `Area2D` scripts such as `Entrance.cs`, `ExitDoor2.cs`, `GoToMachi.cs`, `GoToBalcony.cs`, and `GoToAmun.cs`.
- Dialogue, phone call behavior, and objective prompts are currently implemented in `Player.cs`.
- The project uses Godot's built-in `ui_up`, `ui_down`, `ui_left`, and `ui_right` actions for movement.

## Build Notes

For editor playtesting, use Godot's **Run Project** button. For exported builds, configure export presets in Godot and verify that all files in `Spritos/` and `Audio/` are included.

No license file is currently included. Add one before distributing the project publicly.
