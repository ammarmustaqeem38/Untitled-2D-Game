import { spawnSync } from "node:child_process";
import { cpSync, mkdirSync, mkdtempSync, readFileSync, readdirSync, rmSync, writeFileSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { validateWebBuild } from "./check-web-build.mjs";
import { packageWebAssets } from "./package-web-assets.mjs";

const root = fileURLToPath(new URL("../", import.meta.url));
const version = readFileSync(path.join(root, "scripts/godot-version.txt"), "utf8").trim();
const godot = process.env.GODOT_BIN || "godot";

function run(args, capture = false) {
  const result = spawnSync(godot, args, {
    cwd: root,
    encoding: "utf8",
    stdio: capture ? "pipe" : "inherit",
    windowsHide: true,
  });
  if (result.error) throw new Error(`Cannot run Godot. Set GODOT_BIN to your Godot ${version} executable. ${result.error.message}`);
  if (result.status !== 0) throw new Error(`Godot failed (${result.status}): ${args.join(" ")}\n${result.stderr || ""}`);
  return result.stdout?.trim();
}

const installedVersion = run(["--version"], true);
if (installedVersion.includes(".mono.")) {
  throw new Error("Godot's .NET editor cannot export to Web. Set GODOT_BIN to the standard (non-.NET) Godot editor.");
}
if (!installedVersion.startsWith(`${version}.stable.`)) {
  throw new Error(`Expected Godot ${version} stable, found ${installedVersion}. Use matching editor and export templates.`);
}

const cache = path.join(root, ".cache");
mkdirSync(cache, { recursive: true });
const work = mkdtempSync(path.join(cache, "web-"));
const source = path.join(work, "project");
const output = path.join(work, "export");
const publishGame = path.join(root, "docs/game");
const excluded = new Set(["docs", "scripts", "tests", "node_modules", "bin", "obj"]);

try {
  // Import from source every time, without depending on an editor's .godot cache.
  // Legacy C# copies remain in the repository; all active scenes use GDScript.
  mkdirSync(source);
  for (const entry of readdirSync(root)) {
    if (entry.startsWith(".") || excluded.has(entry)) continue;
    cpSync(path.join(root, entry), path.join(source, entry), {
      recursive: true,
      filter: (file) => !/\.(cs(?:\.uid)?|csproj|sln)$/i.test(file),
    });
  }
  const projectFile = path.join(source, "project.godot");
  writeFileSync(projectFile, readFileSync(projectFile, "utf8").replace(/, "C#"/g, ""));
  // Scene changes use string paths, so include every scene and its dependencies.
  const scenes = readdirSync(source, { recursive: true })
    .filter((file) => file.endsWith(".tscn"))
    .map((file) => `"res://${file.replaceAll(path.sep, "/")}"`)
    .sort();
  const presetsFile = path.join(source, "export_presets.cfg");
  writeFileSync(presetsFile, readFileSync(presetsFile, "utf8")
    .replace(/^export_files=.*$/m, `export_files=PackedStringArray(${scenes.join(", ")})`));
  mkdirSync(output, { recursive: true });
  run(["--headless", "--path", source, "--editor", "--import"]);
  run(["--headless", "--path", source, "--export-release", "Web", path.join(output, "index.html")]);
  validateWebBuild(output);
  run(["--headless", "--main-pack", path.join(output, "index.pck"), "--log-file", path.join(work, "smoke.log"), "--script", path.join(root, "tests/web-smoke.gd")]);
  packageWebAssets(output);
  validateWebBuild(output);

  // Only replace the generated game after a successful export and validation.
  if (path.relative(root, publishGame) !== path.join("docs", "game")) throw new Error("Invalid publish path");
  rmSync(publishGame, { recursive: true, force: true });
  cpSync(output, publishGame, { recursive: true });
  console.log(`Web game ready in ${publishGame}`);
} finally {
  if (path.dirname(work) === cache && path.basename(work).startsWith("web-")) {
    rmSync(work, { recursive: true, force: true });
  }
}
