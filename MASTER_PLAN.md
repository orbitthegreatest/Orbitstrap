# 🚀 ORBITSTRAP IMPROVEMENT MASTER PLAN
## Conversation Breakdown — Pass this to every new conversation

---

## ⚠️ HOW TO USE THIS DOCUMENT
This project is too large for one conversation. Each new conversation should:
1. Read this MASTER_PLAN.md (delivered inside the master zip, not just as a loose file).
2. Look at which tasks are ✅ DONE, 🔄 IN PROGRESS, or ❌ TODO.
3. Pick up where the last conversation left off.
4. Update status markers at the end.
5. Re-deliver the **entire updated package** (see below) — not just a diff — so nothing is ever lost between conversations.

## 📦 WORKFLOW — ONE TASK AT A TIME, FULL PACKAGE ZIP EACH TIME
Starting this conversation, the delivery format changed: instead of a tiny per-task diff zip,
**every delivery is a single zip containing the full current state of the project**:
```
Orbitstrap_Project.zip
├── MASTER_PLAN.md              ← this file, always the latest version
├── Orbitstrap-source/          ← FULL cloned repo, with every fix so far already applied
├── Orbitstrap-things/           ← scaffold for the external assets repo (Task 3A/3B, see below)
└── website/                     ← added once the website exists (not yet)
```
Steps for each new task:
1. Pick the next ❌ TODO task from the checklist below (top to bottom, by phase).
2. Unzip the delivered `Orbitstrap-source/` (or `git clone` fresh if it's not attached) and make the actual code change in the real files.
3. Re-zip the **whole package** (source + black_cursor + website-if-it-exists + updated MASTER_PLAN.md) as one file.
4. Deliver that single zip to the user.
5. Update this MASTER_PLAN.md inside the zip: flip the task's status to ✅ and add an entry to the Conversation Log.
6. Move to the next task — do not batch multiple tasks into one conversation, but always re-deliver the full package, not a partial diff.

**⚠️ NON-NEGOTIABLE RULE: a brand-new full-package .zip must be delivered after EVERY single
step/task is completed — never wait until the end of the conversation, never batch several
completed tasks into one zip.** As soon as one ❌ TODO task turns ✅, stop, zip the whole current
project state, and deliver it before starting the next task. This way, even if the conversation
gets cut off mid-task, every task that was actually finished is already safe in the user's hands
as a complete, working zip — nothing is ever left only half-delivered again.

This guarantees that even if a conversation is cut off, the single most-recently-delivered zip is a complete, self-contained snapshot of the whole project — nothing needs to be reconstructed from memory or old diff-zips ever again.

**GitHub Repo:** https://github.com/orbitthegreatest/Orbitstrap
**Bootstrapper source codes:** https://github.com/orbitthegreatest/Bootstrappers-code
**Emote wheels:** https://github.com/orbitthegreatest/Roblox-emote-weels
**Skyboxes:** https://github.com/orbitthegreatest/nice-skyboxes-roblox
**Reference website:** https://leitostrap.netlify.app/

---

## 📁 PROJECT CONTEXT

**What is Orbitstrap?**
A C# WPF (.NET 8+) Roblox bootstrapper — replaces the default Roblox launcher.
Built on: Bloxstrap + Froststrap + Voidstrap + Fishstrap + Velostrap.
UI library: WPFUI (WPF-UI NuGet package).

**✅ REPO STRUCTURE DUPLICATION — RESOLVED:**
The repo used to contain **two parallel copies of the source code** (`Orbitstrap/` and
`orbitstrap_modified/`). This is now fixed:
- Deleted the stale `Orbitstrap/` folder (it wasn't referenced by `Orbitstrap.sln` at all, and
  `orbitstrap_modified/` was a strict superset — every file that differed was more developed in
  `orbitstrap_modified/`, and nothing except a machine-local `.csproj.user` file existed only in
  the stale copy).
- Renamed `orbitstrap_modified/` → `Orbitstrap/`.
- Updated every path reference across the repo to match: `Orbitstrap.sln`, `build.bat`,
  `.github/workflows/release.yml`, and `PUBLISHING_GUIDE.md`.
- There is now exactly **one** source folder. Future fixes only need to touch it once.

**Repo structure:**
```
Orbitstrap/
├── .github/workflows/       ← GitHub Actions CI/CD (release.yml)
├── Images/                  ← Logo, icons
├── Orbitstrap/              ← THE source folder — only one now
│   ├── App.xaml / App.xaml.cs
│   ├── MainWindow.xaml / .cs
│   ├── UI/Elements/...
│   ├── Models/
│   ├── Services/
│   └── Resources/
├── Scripts/Translations/
├── wpfui/                   ← WPF-UI library source (submodule-like, referenced by .sln)
└── Orbitstrap.sln
```

---

## 🗺️ ALL TASKS — MASTER CHECKLIST

- ✅ **Task 1A:** Roblox account name + profile photo in sidebar UI — implemented in both `Orbitstrap/` and `orbitstrap_modified/`
- ✅ **Task 1B:** New logo (combine Leitostrap + Tazstrap icons, red/black style) — applied to both `Orbitstrap/` and `orbitstrap_modified/`
- ✅ **Task 1C:** Fix purple window outline/border — applied to both `Orbitstrap/` and `orbitstrap_modified/`
- ✅ **Task 1D:** Removed `ModInjector` (offset-based process memory read/write injector) and
  `LuaScriptManager` (arbitrary Lua execution + native DLL loading against the Roblox process) —
  fully deleted from both `Orbitstrap/` and `orbitstrap_modified/`, including settings flags,
  view-model bindings, the FastFlags page toggle, the Bootstrapper launch pipeline, and the
  now-unused `NLua` package reference. See spec below for why.
- ✅ **Task 1E:** New logo v2 (orbit-themed red/black flame mark) — replaced `Orbitstrap.png`,
  `Orbitstrap.ico`, and `Resources/IconOrbitstrap.ico` in both `Orbitstrap/` and
  `orbitstrap_modified/`, plus all four branding PNGs in the top-level `Images/` folder. Also
  removed the outdated app showcase screenshots (`Images/showcase.png`,
  `Images/1748248817498.png` — both stale Voidstrap UI captures, not Orbitstrap).

### PHASE 2 — New Mod Features
- ✅ **Task 2A:** Black cursor mod — fully wired into both `Orbitstrap/` and `orbitstrap_modified/` (PNGs embedded as WPF resources, `Utility/BlackCursorMod.cs` apply/remove logic, `UseBlackCursorMod` setting, and a toggle card on the Mods page). Full details in the Task 2A spec below.
- ✅ **Task 2B:** Custom emote wheel selector (dropdown) — fully wired into both `Orbitstrap/` and `orbitstrap_modified/` (`Utility/EmoteWheelMod.cs` download/apply/remove logic, `SelectedEmoteWheel` setting, `EmoteWheelOptions`/`SelectedEmoteWheelId` on `ModsViewModel`, and a dropdown card on the Mods page). This naturally folds in the emote-wheel half of Task 3B. Full details in the Task 2B spec below.
- ✅ **Task 2C:** Skybox selector — achieved by rewiring the app's **pre-existing** skybox-pack
  feature (which was already a dropdown + apply-at-launch pipeline, just pointed at a different,
  unrelated repo) to pull from the `Orbitstrap-things` manifest instead, rather than building a
  second, competing skybox picker from scratch. Full details in the Task 2C spec below.
- ✅ **Task 2D:** Korblox Right Leg + Headless mod toggles — two `ui:CardExpander` toggle cards
  added to the Mods page in both `Orbitstrap/` and `orbitstrap_modified/`, backed by
  `Utility/KorbloxHeadlessMod.cs` (downloads meshes from `orbitthegreatest/Headless-Korblox-in-R6`
  on first use, caches at `Paths.Cache\KorbloxHeadless\`), `UseKorbloxRightLeg` and `UseHeadlessMod`
  settings in `AppSettings.cs`, matching properties on `ModsViewModel`, and hooks in
  `Bootstrapper.ApplyModifications()` that apply/revert both mods each launch. Mesh files bypass
  the Mods folder pipeline (which skips `.mesh`) and are written directly to `_latestVersionDirectory`.
  Full details in Task 2D spec below.

### PHASE 3 — External Resources Repo
- ✅ **Task 3A:** "Orbitstrap-things" repo — **confirmed live by the user** at
  https://github.com/orbitthegreatest/Orbitstrap-things, populated with `emote-wheels/` and
  `skyboxes/` folders + manifests. The manifest `url` fields (pointed at
  `raw.githubusercontent.com/orbitthegreatest/Orbitstrap-things/...`) were already correct once
  the repo went live under that exact name — no manifest edits were needed.
- ✅ **Task 3B:** Update app to download assets from that repo on-demand — both halves done now:
  emote wheels (Task 2B) and skyboxes (Task 2C) both fetch their manifest.json from
  `Orbitstrap-things` and download only the selected zip on demand.

### PHASE 4 — Website
- ✅ **Task 4A:** Website HTML/CSS/JS — built as a single-page, dark/edgy, gamer-styled site at
  `website/index.html` (+ `website/assets/orbitstrap-mark.png`). Hero, merge-diagram ("built from
  five, shipped as one"), features grid, FAQ, final CTA, footer. See spec below.
- 🔄 **Task 4B:** SEO & Google Search Console setup — `website/sitemap.xml` and
  `website/robots.txt` now exist (pointed at `orbitstrap.vercel.app`, matching the meta/OG/JSON-LD
  tags already added in 4A). Still needs the user to actually deploy to Vercel and submit to
  Search Console — both are real-world account/DNS steps this sandbox can't perform. Full
  step-by-step instructions for both are in `PUBLISHING_GUIDE.md`.

### PHASE 5 — Publishing Guide
- ✅ **Task 5A:** Step-by-step publishing guide — written to `PUBLISHING_GUIDE.md` at the repo
  root. Covers versioning, building the release exe, creating a GitHub release (CLI + web UI,
  with an explicit warning about not renaming the `Orbitstrap.exe` asset since the website's
  download buttons depend on that exact filename), an optional GitHub Actions workflow for
  automated builds, deploying the website to Vercel, submitting to Google Search Console, and a
  copy-paste release checklist.

### PHASE 6 — Build Tooling
- ✅ **Task 6A:** Removed `BUILD_ME_FIRST.bat` (redundant debug-build script), renamed `BUILD_AND_PUBLISH.bat` → `build.bat`, verified it's self-contained and correctly targets `orbitstrap_modified\Orbitstrap.csproj`

---

## 📋 DETAILED TASK SPECIFICATIONS

### Task 1A — Roblox Account Sidebar ✅ DONE
**Where (applied to BOTH `Orbitstrap/` and `orbitstrap_modified/`, since they're currently kept in sync):**
`UI/Elements/Settings/MainWindow.xaml` (+ `.xaml.cs`).
**What was actually used (turned out to already exist in the repo, no new service needed):**
- Reused the existing, already-wired `Orbitstrap.Integrations.AccountManager.Shared` singleton
  (`AltAccount` record: `SecurityToken`, `UserId`, `Username`, `DisplayName`) and its
  `ActiveAccountChanged` event — this is the same object the existing Account Manager window
  already subscribes to, so the sidebar header and the Account Manager window now always agree
  on who's signed in.
- Avatar image fetched directly via `GET https://thumbnails.roblox.com/v1/users/avatar-headshot?userIds={id}&size=48x48&format=Png` using the existing `App.HttpClient` static client, decoded into a frozen `BitmapImage` on the UI thread.
- No separate `RobloxAccountService.cs` was needed — the existing `AccountManager` class already
  covers login state; only the sidebar header UI + a small avatar-fetch helper were added directly
  in `MainWindow.xaml.cs`.
**XAML added:** a clickable `Border` at `Grid.Row="0" Grid.Column="0"` (confirmed empty) above
`RootNavigation`, containing a 32×32 circular avatar (fallback `Person24` `SymbolIcon` until the
real avatar loads) + username/display-name text, styled with the universal WPF-UI brush keys
(`TextFillColorPrimaryBrush`, `TextFillColorSecondaryBrush`, `ControlFillColorDefaultBrush`,
`CardBackgroundFillColorDefaultBrush`, `CardStrokeColorDefaultBrush`) so it renders correctly on
every theme, not just the custom Orbitstrap theme.
**Code-behind added:** `AccountHeader_Click` (opens the existing Account Manager window, reusing
`AccountManagerNavItem_Click`), `OnActiveAccountChanged`/`RefreshAccountHeader` (keep the header in
sync whenever the active account changes), `LoadAccountAvatarAsync` (fetches + decodes the avatar
image, fails silently to the fallback icon if offline/rate-limited). Subscribed in the constructor,
unsubscribed on `Closed` to avoid a memory leak.
**Not covered by this task (left for later):** this shows whichever account is currently active in
the existing Account Manager (used for launching), not a separate "logged into the app" concept —
that's consistent with how Leitostrap's sidebar behaves per the original reference image.

### Task 1B — New Logo ✅ DONE
**Design:** Combined Leitostrap's rounded pinwheel-quadrant silhouette with Tazstrap's detail
language (diagonal accent line + corner rivets + beveled border), recolored to Orbitstrap's
red/black palette, with the same soft red glow the current icon already has.
**Files replaced (in BOTH `Orbitstrap/` and `orbitstrap_modified/`):**
- `Orbitstrap.ico` (main app icon, referenced by `<ApplicationIcon>` in the .csproj and via `pack://application:,,,/Orbitstrap.ico`)
- `Resources/IconOrbitstrap.ico` (embedded theme-picker icon, `<EmbeddedResource Include="Resources\IconOrbitstrap.ico" />` in the .csproj)
Both are multi-res `.ico` (16–256px). Editable SVG source is included in the delivered zip under `Orbitstrap-source/source_and_previews/`.
**To apply:** already applied in the `Orbitstrap-source/` folder inside this zip — just rebuild.

### Task 1C — Fix Purple Outline ✅ DONE
**Root cause:** `UI/Elements/Base/WpfUiWindow.cs` → `ApplyTheme()` called `_themeService.SetSystemAccent()`
unconditionally on every theme apply. That WPF-UI method overwrites `SystemAccentColor` with the
**Windows OS accent color**, stomping on whatever accent the selected Orbitstrap theme (Orbitstrap
red, Purple, etc.) had defined. Since many Windows installs default to a purple OS accent, this made
the window glow + hyperlinks purple regardless of the in-app theme.
**Fix applied (in BOTH `Orbitstrap/` and `orbitstrap_modified/`):** Only call `SetSystemAccent()`
when `isCustom` (i.e. `finalThemeEnum == Enums.Theme.Custom` — the user explicitly wants to follow
the OS accent). Every built-in theme now keeps its own defined accent color from its `UI/Style/<Theme>.xaml` file.
**To apply:** already applied in the `Orbitstrap-source/` folder inside this zip — just rebuild.

### Task 1D — Remove ModInjector & LuaScriptManager ✅ DONE
**Why:** `ModInjector.cs` opened the Roblox process with `PROCESS_VM_READ`/`PROCESS_VM_WRITE`
and patched it using remotely-fetched memory offsets — a process-injection pattern, not a
cosmetic feature. `LuaScriptManager.cs` executed an arbitrary `autoexecute.lua` script against
the running game with a `load`/`loadfunc` API that could load native DLLs into the process. That
combination (memory patching + arbitrary script/DLL injection into a live game client) is the
architecture of a game cheat/exploit engine, not a bootstrapper feature, so both were removed
outright rather than fixed or hidden behind a flag.
**Removed (in BOTH `Orbitstrap/` and `orbitstrap_modified/`):**
- `ModInjector.cs` and `LuaScriptManager.cs` source files
- `UseModInjector`, `EnableLuaScripting`, `ModInjectorEnabled` from `Models/Persistable/AppSettings.cs`
- `UseModInjector` binding from `UI/ViewModels/Settings/FastFlagsViewModel.cs`
- The "Use Orbitstrap FFlag Injector" `OptionControl` card from `UI/Elements/Settings/Pages/FastFlagsPage.xaml` (only existed in `Orbitstrap/`)
- The `LaunchModInjectorIfEnabled` method + its call in `StartRoblox()` + the related comment, from `Bootstrapper.cs`
- The now-unused `<PackageReference Include="NLua" ... />` from both `.csproj` files
**Verified:** repo-wide grep for `ModInjector`, `LuaScriptManager`, `EnableLuaScripting` returns nothing.
**To apply:** already applied in the `Orbitstrap-source/` folder inside this zip — just rebuild.

### Task 1E — New Logo v2 + Remove Old Showcases ✅ DONE
**Design:** A distinct second mark in the same red/black flame family as the existing logo, built
around "Orbitstrap" more literally: a black planet-core disc with a jagged red→orange flame
corona, a thin tilted orbit ring, and a small white ring-and-dot satellite on that ring (echoing
the ring/dot motif from the original mark). Legible down to 32px.
**Files replaced (in BOTH `Orbitstrap/` and `orbitstrap_modified/`):**
- `Orbitstrap.png` (256×256), `Orbitstrap.ico` (multi-res 16–256px), `Resources/IconOrbitstrap.ico`
**Files replaced in top-level `Images/`:** `Orbitstrap.png`, `Orbitstrap-full-dark.png`,
`Orbitstrap-full-light.png`, `Orbitstrap-red.png` (all four now the new mark).
**Removed:** `Images/showcase.png` and `Images/1748248817498.png` — both were stale Voidstrap UI
screenshots (purple theme, wrong app name), not Orbitstrap, and not referenced anywhere in the
README or code, so nothing else needed updating.
**Left untouched intentionally:** `Resources/OldIconOrbitstrap.ico` — this is a deliberate
"classic icon" option in the app's icon picker, not the active default logo.
**To apply:** already applied in the `Orbitstrap-source/` folder inside this zip — just rebuild.

### Task 2A — Black Cursor Mod 🔄 ASSETS READY, LOGIC ❌ TODO
**Cursor PNGs delivered in this zip under `black_cursor/content/textures/...`:**
```
black_cursor/content/textures/Cursors/CrossMouseIcon.png
black_cursor/content/textures/Cursors/KeyboardMouse/ArrowCursor.png
black_cursor/content/textures/Cursors/KeyboardMouse/ArrowFarCursor.png
black_cursor/content/textures/Cursors/KeyboardMouse/IBeamCursor.png
black_cursor/content/textures/MouseLockedCursor.png
```
**How it should work:** When a checkbox is enabled, copy these files to:
`%LocalAppData%\Roblox\Versions\[latest version]\content\textures\Cursors\`
**Still needed (not yet in the real repo):**
- `orbitstrap_modified/Mods/BlackCursorMod.cs` — apply/restore logic
- `orbitstrap_modified/Pages/ModsPage.xaml` (or wherever the real Mods UI lives — verify actual path first, don't assume) — add checkbox
- `orbitstrap_modified/Models/AppSettings.cs` (or `Models/Persistable/AppSettings.cs` — confirm exact file) — add `BlackCursorEnabled` bool
- Decide: embed cursor PNGs as resources, OR download from the future "Orbitstrap-things" repo (Task 3A/3B) — leaning toward the latter to keep the .exe small, per the user's original goal

**✅ RESOLVED (Task 2A, a later conversation):** went with embedding the PNGs as WPF `<Resource>`
items rather than downloading from Orbitstrap-things — simpler, avoids a network dependency for a
handful of tiny PNGs, and the app already embeds larger assets (skybox `.tex` files, fonts) the
same way. Full implementation details are in the Task 2A spec above.
**CLEANUP DONE:** the top-level `black_cursor/` reference folder has been removed from the
package now that those PNGs are embedded resources inside `Orbitstrap-source/` — keeping the loose
copy around was dead weight. The same cleanup rule still applies to any future reference-asset
folder staged loose in the package before its task is fully wired in.

### Task 2B — Emote Wheel Selector ✅ DONE
**Available emote wheels (from Roblox-emote-weels repo, via the `Orbitstrap-things` manifest):**
- Cute bears, Deathnote, Emo2, Itachi, Killua, Miguel, Pink anime girl, Pink theme, Purple, Rick and Morty
**Where (applied to BOTH `Orbitstrap/` and `orbitstrap_modified/`):**
- `Utility/EmoteWheelMod.cs` (new) — `GetManifestAsync()` fetches
  `emote-wheels/manifest.json` from the `Orbitstrap-things` repo via the existing
  `Orbitstrap.Utility.Http.GetJson<T>` helper; `ApplyAsync(url)` downloads the selected wheel's
  zip with `App.HttpClient`, extracts it (handling zips with or without a single wrapping top
  folder) into `Paths.Mods\content\gui\EmotesMenu\`, and writes a small tracker file
  (`.orbitstrap_emotewheel_files.json` in the Mods root) recording exactly which relative paths
  it wrote; `Remove()` deletes only those tracked files then deletes the tracker — so switching
  wheels, or picking "None", never touches anything else that might live in that folder. Follows
  the same "own exactly what I wrote" pattern Task 2A's `BlackCursorMod` already established.
- `Models/Persistable/AppSettings.cs` — added `SelectedEmoteWheel` string (default `"None"`).
- `UI/ViewModels/Settings/ModsViewModel.cs` — added `EmoteWheelOptions`
  (`ObservableCollection<EmoteWheelMod.ManifestEntry>`, seeded with a synthetic "None (Default)"
  entry then populated from the manifest), `LoadEmoteWheelOptionsAsync()` (kicked off from the
  constructor alongside the existing `LoadSkyboxPacksFromGithub()` call), and
  `SelectedEmoteWheelId` (get/set property; setter persists to `AppSettings`, then on a
  background `Task.Run` either calls `EmoteWheelMod.Remove()` for "None" or looks up the matching
  manifest entry and calls `EmoteWheelMod.ApplyAsync(entry.Url)` — mirroring how
  `UseBlackCursorMod`'s setter is structured, including surfacing failures via `App.Logger` and a
  message box).
- `UI/Elements/Settings/Pages/ModsPage.xaml` — new "Emote Wheel Selector" `CardExpander` (placed
  directly under the Black Cursor Mod card), with a `ComboBox` bound to `EmoteWheelOptions`
  (`DisplayMemberPath="Name"`, `SelectedValuePath="Id"`) two-way bound to `SelectedEmoteWheelId`.
**Deviation from the original spec above:** file placed at `Utility/EmoteWheelMod.cs` rather than
the originally-sketched `Mods/EmoteWheelMod.cs`, to match where `BlackCursorMod.cs` and
`DarkTexturesMod.cs` actually live in this repo (there is no `Mods/` services folder) — same kind
of path correction Task 2A already made.
**Folds in part of Task 3B:** the manifest-fetch-then-download flow described in Task 3B is
exactly what `EmoteWheelMod` implements, so the emote-wheel half of Task 3B is done; only the
skybox half (Task 2C) remains.
**Not verified:** could not compile/run this in the sandbox (no .NET SDK, WPF needs Windows —
same limitation as every other C# task so far). Static review only: verified namespaces, the
`Http.GetJson<T>` / `App.HttpClient` / `Paths.Mods` usages, and XAML binding names all match
existing conventions in the file. Flagging as a TODO for the user to build-verify on Windows, and
to actually create/populate the `Orbitstrap-things` GitHub repo (Task 3A) so the manifest URL
resolves — without that repo live, the dropdown will populate with "None (Default)" only and any
other selection will fail with a download error, which is expected until Task 3A is finished.

### Task 2C — Skybox Selector ✅ DONE
**Discovery this conversation:** the app already had a skybox picker — a `ComboBox` bound to
`AvailableSkyboxPacks`/`SelectedSkyboxPack` on the Miscellaneous section of the Mods page,
backed by `App.Settings.Prop.SkyboxName`, applied at launch time inside
`Bootstrapper.ApplyModifications()`. It just pointed at a completely different, unrelated repo
(`KloBraticc/SkyboxPackV2`, downloaded as one big whole-repo zip via the GitHub commits API,
cached by commit SHA). Rather than bolt on a second, competing skybox dropdown per the original
task sketch, this conversation **rewired the existing one** to source from `Orbitstrap-things`
instead — same UI, same settings key, new backend.
**Where (applied to BOTH `Orbitstrap/` and `orbitstrap_modified/`):**
- `UI/ViewModels/Settings/ModsViewModel.cs` — `SkyboxPack` now carries an `Id` (manifest id)
  alongside `Name` (dropped the old `DownloadUri` field, which encoded a GitHub-archive-zip +
  folder-name fragment that no longer applies). `LoadSkyboxPacksFromGithub()` (name kept as-is to
  avoid an unnecessary rename) now fetches `skyboxes/manifest.json` from `Orbitstrap-things` via
  `Orbitstrap.Utility.Http.GetJson<T>` — the same helper `EmoteWheelMod` uses — instead of hitting
  the GitHub contents API. `SelectedSkyboxPack`'s setter now persists `.Id` (a lowercase manifest
  id like `"default"`, `"planet-cyan"`) to `AppSettings.SkyboxName`, not the display `.Name`.
- `Models/Persistable/AppSettings.cs` — `SkyboxName` default changed from `"Default"` to
  `"default"` to match the manifest's lowercase ids.
- `Bootstrapper.cs` — replaced the whole-repo, commit-SHA-cached download
  (`GetLatestCommitShaAsync`/`GetLocalCommit`/`SaveLocalCommit`/`EnsureSkyboxPackDownloadedAsync`,
  all pointed at `SkyboxZipUrl`/`SkyboxCommitApiUrl` for `KloBraticc/SkyboxPackV2`) with
  `EnsureSkyboxDownloadedAsync(skyboxId)`: fetches `skyboxes/manifest.json` from
  `Orbitstrap-things` (reusing `EmoteWheelMod.ManifestEntry` as the id/name/url shape — same data
  shape as the emote-wheel manifest, no need for a second record type), looks up the selected id,
  downloads only that one skybox's zip, and caches it under `%LocalAppData%\...\SkyboxPack\<id>\`
  — skipping the network entirely if that id is already cached. `ApplySkyboxAsync(skyboxId,
  modsFolder)` now calls `EnsureSkyboxDownloadedAsync` internally (so `ApplyModifications()` only
  needs the one call, not two) and copies from that cache folder into
  `PlatformContent\pc\textures\sky\`, same as before.
**Left untouched (out of scope for what was asked):** `ApplySkyboxPatchToRobloxStorageAsync()`,
which patches specific content-hash files into Roblox's local CAS storage from a *different*
repo (`KloBraticc/SkyboxPatch`) — this is a separate mechanism (raw storage-hash patching, not a
"skybox pack") that the user didn't ask to move, and its manifest shape (hash → folder map) 
doesn't fit the `Orbitstrap-things` id/name/url format anyway. Flagging this as a thing to ask
the user about explicitly if it ever needs to move too.
**Not verified:** same caveat as every C# task so far — no .NET SDK/Windows in this sandbox, so
this is a static review only (namespaces, `Http.GetJson<T>` usage, `Paths`/`PackFolder` usage,
and XAML binding names all checked against existing conventions, but not build-verified).

### Task 3A — External Resources Repo ✅ CONFIRMED LIVE
The user confirmed the repo exists and is populated at
https://github.com/orbitthegreatest/Orbitstrap-things, with `emote-wheels/` and `skyboxes/`
subfolders. Fetched the repo page to confirm the folder layout matches what this project already
assumed. The manifest `url` fields already pointed at
`raw.githubusercontent.com/orbitthegreatest/Orbitstrap-things/main/...`, which is the repo's real
path now that it's live — no manifest edits were needed.

### Task 3A — External Resources Repo Scaffold 🔄 IN PROGRESS
**Origin:** the user's own original idea (from the very first request) — create this GitHub repo
so the app can download skyboxes/emote-wheels on demand instead of bundling them all into the
.exe, keeping the download size small. (Cursors ended up embedded directly in the .exe instead —
see Task 2A — so this repo no longer needs a `cursors/` folder.)

**What this conversation actually did:** the user uploaded the real `Roblox-emote-weels` and
`nice-skyboxes-roblox` source zips directly, so this is now populated with real assets instead of
a bare scaffold:
```
Orbitstrap-things/
├── README.md                ← setup instructions + full inventory for the repo owner
├── emote-wheels/
│   ├── manifest.json        ← 10 wheels, real filenames, url fields still PLACEHOLDER domain
│   ├── cute-bears.zip
│   ├── deathnote.zip
│   ├── emo2.zip
│   ├── itachi.zip
│   ├── killua.zip
│   ├── miguel.zip
│   ├── pink-anime-girl.zip
│   ├── pink-theme.zip
│   ├── purple.zip
│   └── rick-and-morty.zip
└── skyboxes/
    ├── manifest.json        ← 24 skyboxes, real filenames, url fields still PLACEHOLDER domain
    ├── northern-lights.zip  ← from nice-skyboxes-roblox
    ├── sky-nibiru-bl.zip    ← from nice-skyboxes-roblox
    ├── troll-face.zip       ← from nice-skyboxes-roblox
    ├── xen-skybox.zip       ← from nice-skyboxes-roblox
    ├── beautiful.zip        ← "old" skybox, re-zipped from Orbitstrap/Resources/Skyboxes/Beautfil
    ├── blue.zip             ← "old", from .../Skyboxes/Blue
    ├── chill-gray.zip       ← "old", from .../Skyboxes/Chill gray
    ├── chromakey.zip        ← "old", from .../Skyboxes/ChromaKey
    ├── default.zip          ← "old", from .../Skyboxes/Default
    ├── grimnight.zip        ← "old", from .../Skyboxes/grimnight
    ├── homer-uchiha.zip     ← "old", from .../Skyboxes/homer uchiha skybox
    ├── jungle-csgo.zip      ← "old", from .../Skyboxes/jungle csgo
    ├── light-blue.zip       ← "old", from .../Skyboxes/Light Blue
    ├── light-pink.zip       ← "old", from .../Skyboxes/Light pink
    ├── minesky.zip          ← "old", from .../Skyboxes/Minesky
    ├── neon-sky.zip         ← "old", from .../Skyboxes/NeonSky
    ├── neon-sky-2.zip       ← "old", from .../Skyboxes/NeonSky2
    ├── pandora.zip          ← "old", from .../Skyboxes/Pandora
    ├── planet-cyan.zip      ← "old", from .../Skyboxes/PlanetCyan
    ├── purple-void.zip      ← "old", from .../Skyboxes/PurpleVoid
    ├── remram.zip           ← "old", from .../Skyboxes/RemRam
    ├── sky-purple.zip       ← "old", from .../Skyboxes/sky purple
    ├── sky2006.zip          ← "old", from .../Skyboxes/sky2006
    └── yumeko.zip           ← "old", from .../Skyboxes/Yumeko
```
The 20 "old" skyboxes are the ones already bundled as embedded `.tex` resources inside the app
itself (`Orbitstrap/Resources/Skyboxes/` and `orbitstrap_modified/Resources/Skyboxes/` — those
embedded copies are untouched; these are just re-zipped duplicates for the external repo so
they're also available via the on-demand download path once Task 2C is built, without forcing
every user to download all 24 skyboxes inside the .exe itself).

**manifest.json format (same for both categories):**
```json
[
  { "id": "cute-bears", "name": "Cute Bears", "url": "https://raw.githubusercontent.com/orbitthegreatest/Orbitstrap-things/main/emote-wheels/cute-bears.zip" }
]
```
**TODO for the user (blocks Task 3A from flipping to ✅):**
1. Create an empty GitHub repo named `Orbitstrap-things` under the `orbitthegreatest` account.
2. Push the entire `Orbitstrap-things/` folder from this zip to it — **including the 34 zip
   files**, not just the two `manifest.json` files.
3. If the actual repo path differs from `orbitthegreatest/Orbitstrap-things`, update the `url`
   field in both manifests to match.
**TODO for a future conversation:** once the user confirms the repo exists and is populated,
flip Task 3A to ✅ and move on to Task 3B (wiring `EmoteWheelMod.cs`/`SkyboxMod.cs` to fetch
`manifest.json` and download the selected asset — this naturally folds into Task 2B/2C rather than
being separate download-plumbing work).

### Task 2D — Korblox Right Leg + Headless Mod Toggles ✅ DONE
**User request (Conversation 13):** "Add 2 checkboxes in the mod tab, one for Korblox right leg
and one for Headless. All the things you need are on this .bat and the assets are in
https://github.com/orbitthegreatest/Headless-Korblox-in-R6."

**Key discovery:** The existing `Bootstrapper.ApplyModifications()` copy pipeline explicitly
`continue`s on `.mesh` files (`if (rel.EndsWith(".mesh")) continue;`) — so the Mods folder
staging approach cannot deliver mesh files at all. Both mods therefore bypass the pipeline and
write directly to `_latestVersionDirectory` from within `ApplyModifications()`.

**GitHub asset source:** `https://github.com/orbitthegreatest/Headless-Korblox-in-R6`
- Korblox right leg: `Korblox Meshes/rightleg.mesh`
- Default right leg: `default avatar meshes/default meshes/rightleg.mesh`
- Default heads: `default avatar meshes/default head meshes/{head,headA..headP}.mesh`

**Cache location:** `Paths.Cache\KorbloxHeadless\{korblox,default_meshes,default_heads}\`
First run downloads; subsequent runs are instant local copies. Call `ClearCache()` to force
re-download.

**Files created/modified (both `Orbitstrap/` and `orbitstrap_modified/`):**
- `Utility/KorbloxHeadlessMod.cs` (new) — `ApplyKorbloxRightLegAsync(versionDir)`,
  `RevertKorbloxRightLegAsync(versionDir)`, `ApplyHeadless(versionDir)`,
  `RevertHeadlessAsync(versionDir)`, `ClearCache()`; all using `App.HttpClient` and `Paths.Cache`.
- `Models/Persistable/AppSettings.cs` — added `UseKorbloxRightLeg` bool (default `false`) and
  `UseHeadlessMod` bool (default `false`).
- `UI/ViewModels/Settings/ModsViewModel.cs` — added `UseKorbloxRightLeg` and `UseHeadlessMod`
  properties (get/set persist to `AppSettings`, `OnPropertyChanged`, `App.Settings.Save()`).
- `UI/Elements/Settings/Pages/ModsPage.xaml` — two new `ui:CardExpander` cards with
  `ui:ToggleSwitch` in their headers (placed between the Black Cursor Mod card and the Emote
  Wheel Selector card), bound to `UseKorbloxRightLeg` and `UseHeadlessMod` respectively.
- `Bootstrapper.cs` (ApplyModifications) — after `App.State.Save()`, added try/catch blocks for
  both mods: Korblox calls `ApplyKorbloxRightLegAsync`/`RevertKorbloxRightLegAsync` based on the
  setting (skips revert if the korblox cache file doesn't exist yet, to avoid a pointless network
  call on first install); Headless calls `ApplyHeadless`/`RevertHeadlessAsync` (skips revert if
  the heads cache folder doesn't exist yet).

**Not build-verified:** same caveat as all C# tasks — no .NET SDK / Windows in this sandbox.
Static review only. Please rebuild on Windows and test both toggles.

### Task 3B — Wire App to Download From the Repo ❌ TODO
**App behavior (once Task 3A's repo is live):** On dropdown open, download that category's
`manifest.json` → populate dropdown from it → on selection, download only that entry's zip →
extract to the right Roblox content folder (see Task 2B/2C specs above for exact destination
paths). This is really the same piece of work as Task 2B/2C's mod services, so it's expected to
be implemented together with those rather than as a separate standalone step.

### Task 4A — Website ✅ DONE
**User decisions (this conversation):** logo = new logo v2 (orbit/flame mark), tone = dark/edgy/
gamer-focused (like leitostrap.netlify.app), hosting = `orbitstrap.vercel.app` (free Vercel
subdomain, no custom domain yet).
**Built:** `website/index.html` (single self-contained file, inline `<style>`, no build step) +
`website/assets/orbitstrap-mark.png` (copied from `Images/Orbitstrap.png`, the Task 1E logo).
**Design tokens used:**
- Colors: near-black warm background (`#0b0707`) with a flame-red/orange accent
  (`#ff4130`→`#ff8a3d`) pulled directly from the existing app logo's own palette, rather than a
  new invented brand color — keeps the site and the app looking like the same product.
- Type: `Chakra Petch` (display/headlines — geometric, technical, matches the "orbit" name
  without reaching for the more overused Orbitron), `Manrope` (body copy), `JetBrains Mono`
  (eyebrows, nav CTA, stat labels, badges — gives it a HUD/tool feel appropriate for a bootstrapper
  aimed at people who already read FastFlag JSON for fun).
- Signature element: an animated orbit ring (two concentric rings, opposite rotation speeds) around
  the logo in the hero, with a small glowing dot "satellite" — a literal, moving version of the
  ring+dot motif already in the Orbitstrap mark itself, built in pure CSS (`@keyframes spin`),
  respecting `prefers-reduced-motion`.
- Structural device: a "merge diagram" section (5 source-bootstrapper nodes converging via SVG
  lines into one central "ORBITSTRAP" node) instead of a generic numbered feature list — this
  encodes the product's actual differentiator (5 tools merged into 1), which is a more accurate
  "hero thesis" for the content than a plain feature grid.
**Sections:** sticky nav → hero (headline + dual CTA to GitHub releases/source) → trust strip →
merge diagram ("Built from five, shipped as one") → 9-feature grid (pulled from README, copy
rewritten in second person / active voice per the writing guidance, not copy-pasted) → FAQ (the
two questions from the README's FAQ: "Can it get me banned?" / "Is it a virus?", as native
`<details>` accordions) → final CTA → footer (GitHub/Releases/Issues links + credit line).
**SEO already included (overlaps with the start of Task 4B):** `<title>`, meta description,
keywords, canonical URL, Open Graph + Twitter Card tags, and a JSON-LD `SoftwareApplication`
schema block — all pointed at `https://orbitstrap.vercel.app/` per the chosen hosting.
**Not yet done (still Task 4B):** actually deploying to Vercel, `sitemap.xml`, `robots.txt`,
Search Console verification/submission.
**Not visually verified:** this sandbox has no headless browser, so the page was reviewed by
reading the markup/CSS and checking tag balance + the merge-diagram SVG line coordinates against
the 5-column grid math, not by rendering it. Please open `website/index.html` in a browser (or
push to Vercel) and confirm it renders as intended, especially the orbit-ring animation and the
merge-diagram connector lines on desktop width.
**Auto-updating download links (Conversation 16):** per the user's request, all three Download
buttons (nav, hero, final CTA) now point at
`https://github.com/orbitthegreatest/Orbitstrap/releases/latest/download/Orbitstrap.exe` instead
of the releases *page*. GitHub resolves that exact URL pattern to whichever asset named
`Orbitstrap.exe` is attached to the newest release, so every future `gh release create ... 
"Orbitstrap.exe"` (per `build.bat`'s own instructions, which already upload the asset with that
exact filename) is picked up automatically — no website edit needed on new releases, no JS
required for the download itself to work. A small optional JS snippet at the bottom of the page
also calls the GitHub API (`/repos/.../releases/latest`) to display the live version tag + file
size in the fine print under the hero button; it fails silently (leaving the static fine-print
text in place) if the API is unreachable or rate-limited, so the actual download links never
depend on it succeeding.

### Task 4B — SEO & Google Search Console 🔄 IN PROGRESS
**Goal:** Website appears when someone Googles "Orbitstrap"
**Done as part of Task 4A already:** `<title>`, meta description + keywords, canonical URL, Open
Graph + Twitter Card tags, JSON-LD `SoftwareApplication` schema — all in `website/index.html`,
pointed at `https://orbitstrap.vercel.app/`.
**Done this conversation:** `website/sitemap.xml` (single-URL sitemap for the homepage) and
`website/robots.txt` (`Allow: /` + sitemap pointer) added, both pointed at the same
`orbitstrap.vercel.app` URL.
**Still TODO (real-world account steps, can't be done from this sandbox):**
1. Deploy `website/` to Vercel → confirm the live URL matches `orbitstrap.vercel.app` (or update
   the URLs in `index.html`/`sitemap.xml`/`robots.txt` if it doesn't).
2. Go to https://search.google.com/search-console/ → Add property → verify via the HTML-file
   method → submit `sitemap.xml` → Request Indexing on the homepage.
Full step-by-step for all of this is now in `PUBLISHING_GUIDE.md`, section 5–6.
**Timeline:** Google usually indexes a new site within 1–4 days after submitting to Search Console.

### Task 5A — Publishing Guide ✅ DONE
**Delivered:** `PUBLISHING_GUIDE.md` at the repo root. Covers: semantic versioning convention,
building the release exe with `build.bat`, creating a GitHub release both via `gh` CLI and the
web UI (with an explicit callout that the asset must stay named exactly `Orbitstrap.exe`, since
the website's Download buttons hardcode that filename in GitHub's `/releases/latest/download/...`
redirect — renaming it on upload would silently break the site's download buttons), a
ready-to-add (not yet installed) GitHub Actions workflow for automated tag-triggered builds,
step-by-step Vercel deployment for `website/`, Google Search Console submission, a note on the
README's existing shields.io badges (already auto-updating, no action needed), and a copy-paste
release checklist.

### Task 2A — Black Cursor Mod ✅ DONE
**Where (applied to BOTH `Orbitstrap/` and `orbitstrap_modified/`):**
- `Resources/BlackCursor/*.png` (new, 5 files copied from `black_cursor/`)
- `Orbitstrap.csproj` (new `<Resource Include>` entries)
- `Utility/BlackCursorMod.cs` (new)
- `Models/Persistable/AppSettings.cs` (`UseBlackCursorMod` bool added)
- `UI/ViewModels/Settings/ModsViewModel.cs` (`UseBlackCursorMod` property added)
- `UI/Elements/Settings/Pages/ModsPage.xaml` (new toggle card added)

**Destination paths written by `Apply()` / cleaned by `Remove()`:**
```
Paths.Mods\content\textures\MouseLockedCursor.png
Paths.Mods\content\textures\Cursors\KeyboardMouse\ArrowCursor.png
Paths.Mods\content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png
Paths.Mods\content\textures\Cursors\KeyboardMouse\IBeamCursor.png
Paths.Mods\content\textures\Cursors\CrossMouseIcon.png
```
These are the exact same paths the existing "Custom Cursor Set" feature in `ModsViewModel.cs`
writes to (see `ApplyCursorSetCommand` around line ~1440-1530) — so this mod and a saved custom
cursor set will overwrite each other. That's a known, intentional trade-off for a simple built-in
preset, not a bug — the UI copy says so.

**TODO for next conversation:** confirm on a real Windows build that `Application.GetResourceStream`
resolves the `pack://application:,,,/Resources/BlackCursor/...` URIs correctly (this pattern is
already used elsewhere in the repo for icons/skyboxes, but wasn't build-verified here).

### Task 6A — Build Script Cleanup ✅ DONE
**What existed before:** two build scripts at the repo root doing overlapping things —
`BUILD_ME_FIRST.bat` (restore + `dotnet build Orbitstrap.sln -c Debug`, produced a debug DLL/exe
under `orbitstrap_modified\bin\Debug\net10.0-windows\`, meant as a one-time dev setup script) and
`BUILD_AND_PUBLISH.bat` (restore + `dotnet publish` a self-contained single-file Release exe to
`publish_output\Orbitstrap.exe`, plus printed instructions for creating a GitHub release with the
`gh` CLI).
**What changed:**
- Deleted `BUILD_ME_FIRST.bat` — confirmed via full-repo grep that nothing else (README, `.github/workflows/*`, the other bat file) referenced it by name, so removing it broke nothing.
- Renamed `BUILD_AND_PUBLISH.bat` → `build.bat`. Its contents are unchanged and were already fully self-contained (no dependency on the deleted script).
**Verification performed (static, not a live build):**
- Confirmed `build.bat` targets `orbitstrap_modified\Orbitstrap.csproj` — the same project the
  `.sln` actually builds (see the repo-structure note above), and that path exists on disk.
- Confirmed the `.sln`'s two project references (`orbitstrap_modified\Orbitstrap.csproj` and
  `wpfui\src\Wpf.Ui\Wpf.Ui.csproj`) both resolve to real files.
- Confirmed the target framework (`net10.0-windows8.0`, `OutputType=WinExe`, `UseWPF=true`) is
  compatible with the `dotnet publish -r win-x64 --self-contained true -p:PublishSingleFile=true`
  command used in the script.
- **Could not run an actual `dotnet build`/`dotnet publish`** in this conversation's sandbox — it's
  Linux with no .NET SDK installed, and the network policy here doesn't allow installing one
  (dotnet's install domains aren't in the allowed list), and WPF apps need the Windows desktop
  runtime to build/run properly anyway. So this was verified as thoroughly as possible without
  actually executing it.
**TODO for next conversation (or the user, on their own Windows machine):** actually run
`build.bat` end-to-end once, on Windows with the .NET SDK installed, to confirm it produces a
working `publish_output\Orbitstrap.exe` — flip this line to a confirmed ✅ once that's done. If it
fails, the error will point at exactly which step (`[1/3]` restore vs `[2/3]` publish) broke.

---

## 📁 FOLDER LAYOUT (updated by the user, Conversation 18 — do not restructure)

The project root is now `Orbitstrap_Project/`, containing:
- `Orbitstrap-source/` — the actual git repo (has `.git/`, pushed to
  `github.com/orbitthegreatest/Orbitstrap`). `.github/workflows/` lives inside
  here, since that's the repo root GitHub Actions needs.
- `website/` — sibling of `Orbitstrap-source/`, NOT inside it, and NOT currently
  under git version control (no `.git` of its own, and outside the
  `Orbitstrap-source` git root). Deploy this folder to Vercel directly.
- `MASTER_PLAN.md` and `PUBLISHING_GUIDE.md` — also top-level siblings now,
  outside `Orbitstrap-source`.

Any future work: source/C#/XAML changes go in `Orbitstrap-source/`, website
changes go in the top-level `website/`, docs stay top-level. Don't move things
back to the old nested layout.

## 📝 CONVERSATION LOG

### Conversation 18 (New logo v3, folder reorg respected, GitHub Actions workflow)
**Date:** 2026-08-10
**Completed:**
- ✅ **Logo swap.** User supplied a new vector logo (`orbitstrap-logo-v2.svg` — a
  four-panel red pinwheel mark with a small white/red orbit-ring center glyph).
  To guarantee it's never blurry, every raster size was rendered directly from
  the vector (via a headless-Chrome screenshot at 1024×1024 with a real alpha
  channel, confirmed by compositing over a dark background) rather than
  upscaling a small bitmap, then downsampled with Lanczos filtering for the
  smaller sizes — so every size (16 through 512px) traces back to the same
  crisp vector source. Replaced in every actual brand-logo location (left
  the unrelated legacy "icon style" options — Icon2008.ico, IconEarly2015.ico,
  etc. — alone, since those are separate selectable bootstrapper icon styles,
  not the app's own brand mark):
  - `Orbitstrap-source/Images/Orbitstrap.png`, `Orbitstrap-full-dark.png`,
    `Orbitstrap-full-light.png`, `Orbitstrap-red.png`
  - `Orbitstrap-source/{Orbitstrap,orbitstrap_modified}/Orbitstrap.png` and
    `Orbitstrap.ico` (the actual `<ApplicationIcon>` for the .exe)
  - `Orbitstrap-source/{Orbitstrap,orbitstrap_modified}/Resources/IconOrbitstrap.ico`
    (the "Orbitstrap" entry in the in-app icon-style picker)
  - `website/assets/orbitstrap-mark.png` (bumped to 512px, up from 256px)
  - Also stashed the raw vector source itself, for future re-exports without
    needing another upload: `Orbitstrap-source/Images/orbitstrap-mark.svg` and
    `website/assets/orbitstrap-mark.svg`.
  Checked the .csproj/.xaml references first (`pack://application:,,,/Orbitstrap.ico`
  etc.) — all reference these exact filenames, so no code/XAML changes were
  needed, just the files themselves.
- ✅ **Folder reorg respected.** The user restructured the project locally
  (`website/`, `MASTER_PLAN.md`, `PUBLISHING_GUIDE.md` are now top-level
  siblings of `Orbitstrap-source/`, not nested inside it). Kept that layout
  as-is — see the new "FOLDER LAYOUT" section above — and made no changes back
  toward the old nested structure.
- ✅ **GitHub Actions workflow added:** `Orbitstrap-source/.github/workflows/release.yml`.
  Triggers on any `v*` tag push (plus manual `workflow_dispatch` for test
  runs). Mirrors `build.bat` exactly — `dotnet restore` then `dotnet publish`
  of `orbitstrap_modified/Orbitstrap.csproj` as a self-contained single-file
  win-x64 exe — then creates the GitHub release automatically and attaches
  the exe. The asset is uploaded straight from `publish_output/Orbitstrap.exe`
  so the filename always matches what the website's download buttons expect.
  Every run (including manual/non-tag runs) also uploads the exe as a
  downloadable build artifact, so test builds don't require a tag.
**Not verified (still true for every C# task in this project):** this sandbox
has no Windows/.NET SDK, so the workflow YAML was written carefully against
`build.bat`'s known-working steps but has not actually been run. First real
tag push will be the first real test — check the Actions tab after pushing
`v1.x.x` to confirm it goes green and the release gets created with the exe
attached.
**Next up:**
- User to push this commit + `.github/workflows/release.yml` to GitHub, then
  cut a real tag (e.g. `git tag v1.2.0 && git push --tags`) to test the
  workflow end-to-end.
- Still open from prior conversations: deploy `website/` to Vercel, finish
  Search Console submission, and get an actual Windows build/test pass done
  at some point.

### Conversation 17 (Task 4B sitemap/robots + Task 5A publishing guide)
**Date:** 2026-08-09
**Completed:**
- 🔄 Task 4B (partial — the parts this sandbox can actually produce): added
  `website/sitemap.xml` and `website/robots.txt`, both pointed at `orbitstrap.vercel.app` to match
  the meta/OG/JSON-LD tags already added in Task 4A. Deploying to Vercel and submitting to Google
  Search Console are real account/DNS steps that need the user — instructions for both are now in
  `PUBLISHING_GUIDE.md`.
- ✅ Task 5A: wrote `PUBLISHING_GUIDE.md` — versioning convention, build steps, GitHub release
  creation (CLI + web UI, with a callout on why the `Orbitstrap.exe` asset filename must not
  change), an optional GitHub Actions workflow to automate future builds, Vercel deployment steps
  for the website, Search Console submission steps, and a release checklist.
- Re-packaged the full project into one zip, per the "new zip after every completed step" rule.
**Next up (in order):**
- 🔄 User to actually deploy `website/` to Vercel and submit to Search Console (only remaining
  piece of Task 4B — the guide for both is in `PUBLISHING_GUIDE.md`).
- ❌ (Optional, not on the original checklist) add `.github/workflows/release.yml` for real,
  using the workflow already drafted in `PUBLISHING_GUIDE.md`, if the user wants automated builds.
- 🔄 Still open, carried over from every prior conversation: confirm `build.bat` produces a
  working exe on a real Windows machine, and confirm all C#/XAML changes compile and work at
  runtime (all unverified — no Windows/.NET SDK here). Also still never rendered
  `website/index.html` in an actual browser.
- All of Phase 1–5's *buildable-from-this-sandbox* work is now done. What's left across the whole
  project is real-world verification (Windows build/test pass) and account-level steps (Vercel,
  Search Console) that only the user can do.

### Conversation 16 (Auto-updating download links)
**Date:** 2026-08-09
**Completed:**
- ✅ Per the user's request ("make it auto download the latest .exe... like an auto updater"):
  changed all three Download buttons in `website/index.html` (nav bar, hero, final CTA) from
  linking to the GitHub releases *page* to linking directly at
  `.../releases/latest/download/Orbitstrap.exe` — GitHub's built-in stable redirect that always
  resolves to the `Orbitstrap.exe` asset on whichever release is currently newest. This needs no
  JS and nothing to update on the website when a new version ships, as long as `build.bat`'s
  existing `gh release create` step keeps uploading the asset as `Orbitstrap.exe` (confirmed it
  already does).
- Added a small optional JS snippet that calls the GitHub API to show the live version tag and
  `.exe` file size in the fine print under the hero button, with a silent fallback (static text
  stays) if the API call fails — purely cosmetic, the download links don't depend on it.
- Re-packaged the full project into one zip, per the "new zip after every completed step" rule.
**Next up (in order):**
- ❌ Task 4B: deploy to Vercel, add `sitemap.xml` + `robots.txt`, submit to Google Search Console.
- ❌ Task 5A: Step-by-step publishing guide
- 🔄 Still open: confirm `build.bat` produces a working exe, confirm all C#/XAML changes compile,
  and view `website/index.html` in an actual browser at least once (never rendered in this
  sandbox).

### Conversation 15 (Task 4A — Website)
**Date:** 2026-08-09
**Completed:**
- ✅ Task 4A: Built the Orbitstrap website — `website/index.html` + `website/assets/`. User chose
  the new v2 logo, a dark/edgy/gamer tone, and `orbitstrap.vercel.app` hosting via the elicitation
  prompt in the previous turn. Full design breakdown in the Task 4A spec above.
- Re-packaged the full project (source + website + this master plan) into one zip, per the "new
  zip after every completed step" rule.
**Next up (in order):**
- ❌ Task 4B: finish SEO — deploy to Vercel, add `sitemap.xml` + `robots.txt`, submit to Google
  Search Console (basic meta/OG/JSON-LD tags are already done as part of 4A).
- ❌ Task 5A: Step-by-step publishing guide
- 🔄 Still open: confirm `build.bat` produces a working exe on a real Windows machine, and confirm
  all prior C#/XAML changes compile and work at runtime (all unverified — no Windows/.NET SDK
  here). Also: please actually view `website/index.html` in a browser — it was never rendered in
  this sandbox, only statically reviewed.

### Conversation 14 (Bug fixes — Force Roblox Reinstallation + removed "Find Mods" card)
**Date:** 2026-08-09
**Completed (applied to BOTH `Orbitstrap/` and `orbitstrap_modified/`):**
- 🐛 **"Force Roblox reinstallation" toggle did nothing, real root cause found:**
  `UI/Elements/Settings/Pages/ChannelPage.xaml` had a `ui:ToggleSwitch` bound to
  `ForceRobloxReinstallation`, but **that property never existed anywhere in the C# code** — not
  in `AppSettings.cs`, not in `ChannelViewModel.cs`, and nothing in `Bootstrapper.cs` ever read it.
  It was a pure orphaned XAML binding: the toggle would flip visually in the UI but silently did
  nothing on disk and wasn't persisted anywhere, so activating it and then launching Roblox never
  triggered a reinstall.
  **Fix:**
  - `Models/Persistable/AppSettings.cs` — added `public bool ForceRobloxReinstallation { get; set; } = false;`.
  - `UI/ViewModels/Settings/ChannelViewModel.cs` — added a matching `ForceRobloxReinstallation`
    property (get/set from `App.Settings.Prop`), explicitly calling `App.Settings.Save()` in the
    setter so the flag survives even if the app is closed before the next Roblox launch.
  - `Bootstrapper.cs` (`Run()`) — right after `GetLatestVersionInfo()`, if the flag is set: clears
    `AppData.State.VersionGuid` (this is the same mechanism `Installer.cs` already uses elsewhere
    in this codebase to force a reinstall after an app update), which makes the existing
    `_mustUpgrade` check true so the normal `UpgradeRoblox()` path runs and deletes+redownloads
    the current version directory from scratch — even if no new Roblox version is available. The
    flag is then immediately reset to `false` and saved, so it's a one-shot "reinstall on next
    launch" action (matching its own description string,
    `Menu.Behaviour.ForceRobloxReinstall.Description` = "Roblox will be installed fresh on next
    launch."), not a permanent always-reinstall mode.
  **Not build-verified:** same caveat as every C# task in this project — no .NET SDK/Windows in
  this sandbox. Please rebuild, enable the toggle, launch Roblox once, and confirm the Versions
  folder actually gets wiped and redownloaded, and that the toggle switches itself back off
  afterward.
- 🗑️ **Removed the "Find Mods" card from the Mods page** (per the user's explicit request — not a
  bug, a UI removal): deleted the `ui:CardAction` block (icon + "Find Mods" / "Search for Mods on
  the Orbitstrap Website." text, linking to `https://orbitstrapp.netlify.app/mods/mods`) from
  `UI/Elements/Settings/Pages/ModsPage.xaml`, directly above the "Preset Mod" tab's UI Overlays
  section. Verified `GlobalViewModel.OpenWebpageCommand` is still used elsewhere in the same file
  (other mod cards link out the same way), so nothing else needed cleanup.
- Re-packaged the full project into one zip, per the "new zip after every completed step" rule.
**Next up (in order), unchanged from Conversation 13's list — did not start Task 4A this
conversation since it was explicitly asked to fix these two things first:**
- ❌ Task 4A: Website HTML/CSS/JS — still needs user input first (domain? tone? which branding
  asset/logo to use?)
- ❌ Task 4B: SEO & Google Search Console setup
- ❌ Task 5A: Step-by-step publishing guide
- 🔄 Still open: confirm `build.bat` produces a working exe on a real Windows machine, and confirm
  Tasks 2A, 2B, 2C, 2D, and this conversation's two fixes all compile and work at runtime (all
  unverified — no Windows/.NET SDK here).

### Conversation 13 (Task 2D — Korblox Right Leg + Headless mod)
**Date:** 2026-08-09
**Completed:**
- ✅ Task 2D: Added two toggle-switch cards to the Mods page: "Korblox Right Leg" and "Headless".
  The user provided the reference `.bat` file and pointed at the GitHub repo
  `orbitthegreatest/Headless-Korblox-in-R6` for the assets. Applied to both `Orbitstrap/` and
  `orbitstrap_modified/`. Full implementation breakdown in the Task 2D spec above.
- Re-packaged the full project into one zip, per the "new zip after every completed step" rule.
**Next up (in order):**
- ❌ Task 4A: Website HTML/CSS/JS — needs user input first (domain? tone? branding asset to use?)
- ❌ Task 4B: SEO & Google Search Console setup
- ❌ Task 5A: Step-by-step publishing guide
- 🔄 Still open: confirm `build.bat` produces a working exe on a real Windows machine, and confirm
  Tasks 2A, 2B, 2C, 2D all compile and work at runtime (all unverified — no Windows/.NET SDK here).

### Conversation 12 (Real fix for the emote wheel — wrong destination path, found by the user)
**Date:** 2026-08-09
**Context:** Conversation 11's UI restructure (moving the ComboBox out of the CardExpander header)
fixed the visual/interaction bug, but the user reported the wheel still always applied "Cute Bears"
regardless of selection. **The user found the actual root cause themselves:** `EmoteWheelMod` was
extracting each wheel's zip into `Paths.Mods\content\gui\EmotesMenu\`, but the zips already contain
their own internal `content\textures\ui\Emotes\Large\...` structure (the actual path Roblox reads
emote-wheel button textures from). So every wheel was actually landing at
`Paths.Mods\content\gui\EmotesMenu\content\textures\ui\Emotes\Large\...` — a path Roblox never
reads — meaning **no wheel selection was ever taking effect at all**; Roblox was just always
showing whatever was already baked into the game's own default/cached files (which happened to look
like "Cute Bears" to the user, but the app itself was never successfully applying anything).
**Fix (applied to both `Orbitstrap/` and `orbitstrap_modified/`):**
- `Utility/EmoteWheelMod.cs` — changed `DestRelativeParts` from `{ "content", "gui", "EmotesMenu" }`
  to `Array.Empty<string>()`, so `ApplyAsync`/`Remove` now extract/track relative to `Paths.Mods`
  root directly, letting each wheel zip's own `content\textures\ui\Emotes\Large\...` structure land
  exactly where Roblox expects it.
- `Bootstrapper.cs` — `ApplyModifications()`'s mod-copy loop now explicitly skips
  `.orbitstrap_emotewheel_files.json` (EmoteWheelMod's own bookkeeping/tracker file), so that
  file doesn't get copied into the live Roblox install directory as clutter now that it lives
  directly in the Mods root instead of a nested subfolder.
**Not verified with a real build/run:** same caveat as always — no Windows/.NET SDK in this
sandbox. Please rebuild, select a wheel (e.g. Miguel), relaunch Roblox, and confirm the emote
wheel actually changes now.

**Same conversation, second bug — the "Starting Roblox" launch dialog logo is also blurry (a
third, distinct spot from the two Conversation 11 already fixed):** root cause is different again.
This dialog's icon doesn't go through WPF's `BitmapImage`/pack-URI path at all — it goes through
`System.Drawing.Icon` (GDI+): `App.Settings.Prop.BootstrapperIcon.GetIcon()` returns a
`Properties.Resources.IconOrbitstrap` icon loaded from the embedded multi-size `.ico` via the
resx-generated property, then `.GetImageSource()` converts it to a WPF `ImageSource`. A
`System.Drawing.Icon` loaded this way resolves to the **system's small icon size (commonly
32×32)**, not the largest frame in the file — even though 48/64/128/256px frames exist in the
same `.ico`. The dialogs then display that at 48–80px (`FluentDialog.xaml` 80×80,
`ClassicFluentDialog.xaml` 48×48, etc.), upscaling a 32×32 bitmap → blurry. The codebase already
has the fix for this pattern (`IconEx.GetSized(width, height)`, an extension that requests a
specific frame from the icon) and one older dialog (`ProgressDialog.cs`) already used it — the
newer Fluent/Byfron/Classic/Custom dialogs just never called it.
**Fix (applied to both `Orbitstrap/` and `orbitstrap_modified/`):** inserted `.GetSized(256, 256)`
before `.GetImageSource()` at all 5 call sites building the dialog `Icon`:
`UI/ViewModels/Bootstrapper/BootstrapperDialogViewModel.cs`,
`UI/Elements/Bootstrapper/ByfronDialog.xaml.cs`,
`UI/Elements/Bootstrapper/ClassicFluentDialog.xaml.cs`,
`UI/Elements/Bootstrapper/CustomDialog.xaml.cs`, `UI/Elements/Bootstrapper/FluentDialog.xaml.cs`.
Also documented the root cause directly on `IconEx.GetSized`'s definition so this doesn't get
missed again if a new dialog is added later. Left `WinFormsDialogBase.cs`'s native
`Form.Icon = ...GetIcon()` and `VistaDialog.cs`'s `TaskDialogIcon` alone — those are OS-managed
small title-bar/task-dialog icon slots, a different display mechanism, and weren't reported as
blurry.
**Not verified with a real build/run:** same caveat as above.
**Next up, unchanged from Conversation 11's list.**

### Conversation 11 (Real root-cause fixes for the emote wheel + blurry logo bugs, reported again by user)
**Date:** 2026-08-09
**Context:** Conversation 7's fixes for these same two bugs did not actually resolve them — the
user reported both still happening (screenshot of the Emote Wheel Selector showing a blank combo
box with two chevrons stacked next to each other, and the splash-screen logo still blurry/pixelated).
This conversation found and fixed the real root causes of both, applied to **both**
`Orbitstrap/` and `orbitstrap_modified/`:

- 🐛 **Emote wheel always applying "Cute Bears" + the doubled-chevron/blank ComboBox, real root
  cause found:** the `ComboBox` lived inside `ui:CardExpander.Header`. Per WPF-UI's own
  `CardExpander.xaml` control template, the entire `Header` content is hosted inside the
  expander's own `ToggleButton` (`ContentPresenter` inside `DefaultUiCardExpanderToggleButtonStyle`).
  That means the emote-wheel ComboBox was an interactive popup control nested inside a second,
  outer interactive toggle control — this is what produced both symptoms at once: the ComboBox's
  own dropdown arrow rendering right next to the CardExpander's separate expand chevron (the
  "doubled chevron" / blank-looking box in the screenshot), and mouse-capture/click-bubbling
  between the popup and the outer ToggleButton interfering with the SelectedValue binding ever
  committing a new value — so the wheel that actually got downloaded and applied to disk kept
  landing on whichever entry was first in the list (Cute Bears) instead of whatever was clicked.
  Conversation 7's fix (a concurrency lock in `ModsViewModel`) addressed a real but secondary
  issue and didn't touch this actual cause, which is why the bug persisted.
  **Fix:** rebuilt the Emote Wheel Selector card using `controls:OptionControl` (Header +
  Description properties, ComboBox in the plain content area) — the exact same pattern the
  Skybox Manager card directly above it already uses successfully, which keeps the ComboBox out
  of any ToggleButton's content. No ViewModel/binding changes needed; `SelectedEmoteWheelId`'s
  existing `SelectedValue="{Binding ..., Mode=TwoWay}"` binding is unchanged, just re-hosted.
  Applied to `UI/Elements/Settings/Pages/ModsPage.xaml` in both source copies.
- 🐛 **Blurry logo, real root cause found:** Conversation 7 diagnosed this as a too-small `.ico`
  file and regenerated it with all 6 standard sizes (confirmed present: 16/32/48/64/128/256) — but
  the blur persisted because the actual problem is that `BitmapImage` frame-selection for
  multi-frame `.ico` files loaded via `pack://` URI is not guaranteed to pick the largest/best
  frame just because `DecodePixelWidth`/`DecodePixelHeight` are set, even when that frame exists
  in the file. The three places rendering the logo large on-screen (splash screen, About page,
  launch-menu dialog) were all pulling from `Orbitstrap.ico` and getting a lower-quality frame
  upscaled to 72–120px.
  **Fix:** switched those three large on-screen `Image` sources from `Orbitstrap.ico` to
  `Orbitstrap.png` (a single unambiguous 256×256 RGBA source, already registered as a `<Resource>`
  in both `.csproj` files) — `UI/Elements/Settings/MainWindow.xaml` (`AppIconImage` resource, used
  for the splash logo), `UI/Elements/About/Pages/AboutPage.xaml` (`Image1`), and
  `UI/Elements/Dialogs/LaunchMenuDialog.xaml`. Left every small titlebar `Icon="...Orbitstrap.ico"`
  usage (rendered at ~16–20px, where `.ico`'s small frames are exactly right) and the
  `<ApplicationIcon>`/taskbar `.ico` references untouched — those aren't blurry and don't need to
  change.
- **Not verified with a real build/run:** same caveat as every prior C# task in this project — no
  .NET SDK or Windows in this sandbox, so this is a static, root-cause-traced fix, not a
  compiled-and-confirmed one. Please rebuild and confirm both: (1) switching emote wheels a few
  times in a row actually applies the one you last picked and the ComboBox now shows its text with
  a single chevron, and (2) the splash-screen and About-page logos render sharp at their full size.
- Re-packaged the full project into one zip, per the "new zip after every completed step" rule.
**Next up, per the master checklist (unchanged otherwise):**
- ❌ Task 4A: Website HTML/CSS/JS — needs user input first (domain name? hosting preference beyond
  the "Vercel" mention in Task 4B? tone/copy? which existing logo/branding asset to use?)
- ❌ Task 4B: SEO & Google Search Console setup
- ❌ Task 5A: Step-by-step publishing guide
- 🔄 Still open: confirm `build.bat` actually produces a working exe on a real Windows machine —
  this is now the single highest-priority open item, since a growing number of fixes (Task 1D/1E,
  Conversation 10's build-error fixes, this conversation's two bug fixes) have never been
  build-verified in this project. Strongly recommend doing a real Windows build/test pass before
  adding more features on top.

**⚠️ IMPORTANT CORRECTIONS (carried forward — read this first):**
- The original "Conversation 1" (RobloxAccountService.cs, EmoteWheelMod.cs, SkyboxMod.cs, website,
  publishing guide, etc.) ran out of tokens before anything was ever delivered as a file — none of
  that code exists anywhere except as text in an old chat transcript. **Never happened.**
- A later conversation started real work on Task 1A (Roblox account sidebar) directly against a
  cloned repo, got quite far (XAML header added, code-behind wired up, brush-key bugs found and
  fixed), but was **interrupted before any zip was delivered**. That sandbox no longer exists in
  this conversation, so **all of that Task 1A work is also lost and must be redone from scratch.**
  Its notes are preserved above in the Task 1A spec so the next conversation doesn't have to
  re-discover the same things (file paths, brush keys, symbol names, click handler to reuse).

### Conversation 9 (Task 2C + Task 3A confirmed live)
**Date:** 2026-08-09
**Completed:**
- ✅ Task 3A confirmed: user reported the `Orbitstrap-things` repo is pushed live at
  https://github.com/orbitthegreatest/Orbitstrap-things. Fetched the repo page to verify
  `emote-wheels/` and `skyboxes/` folders both exist as expected — no manifest URL changes were
  needed since they already pointed at the real `orbitthegreatest/Orbitstrap-things` path.
- ✅ Task 2C: per the user's explicit request ("for the skyboxes, change the repo to
  Orbitstrap-things"), rewired the app's pre-existing skybox-pack feature (previously pointed at
  the unrelated `KloBraticc/SkyboxPackV2` repo) to source from the `Orbitstrap-things`
  `skyboxes/manifest.json` instead — same dropdown, same `SkyboxName` setting, new manifest-based
  backend (`Bootstrapper.EnsureSkyboxDownloadedAsync`/`ApplySkyboxAsync`,
  `ModsViewModel.LoadSkyboxPacksFromGithub`). This satisfies Task 2C without creating a second,
  competing skybox picker — see the Task 2C spec above for the full breakdown. Also completes the
  skybox half of Task 3B, so Task 3B is now fully ✅.
- Left `ApplySkyboxPatchToRobloxStorageAsync()` (a separate CAS-storage-hash patch mechanism,
  pointed at `KloBraticc/SkyboxPatch`) untouched — different data shape, wasn't part of what was
  asked. Flagged for the user in the Task 2C spec in case it should move too.
- Could not compile/run in the sandbox (no .NET SDK, WPF needs Windows) — static review only,
  same caveat as every C# task so far.
- Re-packaged the full project (source + Orbitstrap-things + this master plan) into one zip, per
  the "new zip after every completed step" rule.
**Next up (in order):**
- ❌ GitHub Actions workflow for automated .exe releases
- ❌ Website (Task 4A) + SEO/Search Console setup (Task 4B)
- ❌ Publishing guide (Task 5A)
- 🔄 Still open: confirm `build.bat` produces a working exe, and confirm the Task 2A Black Cursor
  mod, Task 2B Emote Wheel mod, and Task 2C reworked Skybox mod all compile and work at runtime
  (all carried over, unconfirmed — no Windows/.NET SDK here). This is now the biggest pile of
  unverified-at-runtime work in the project — worth prioritizing an actual Windows build/test pass
  before adding more features.
- All of Phase 1, 2, and 3 are now ✅ — Phase 4 (Website) is the natural next phase per the
  checklist's top-to-bottom ordering.

### Conversation 2 (real file output began here)
**Completed:**
- ✅ Task 1C: Found & fixed the purple-outline bug (`SetSystemAccent()` in `WpfUiWindow.cs`).
- ✅ Task 1B: New combined Leitostrap+Tazstrap logo, red/black.
- Delivered as two separate small diff-zips: `orbitstrap_step1_fix_purple_outline.zip`, `orbitstrap_step2_new_logo.zip`.

### Conversation 3 (this conversation — switched to full-package workflow)
**Date:** 2026-08-09
**What changed:** User asked to stop doing tiny per-task diff zips and instead always deliver ONE
zip containing the full current project state (whole source tree + black cursor assets + master
plan, website once it exists), so a new conversation never has to reconstruct anything from
fragments again.
**Completed:**
- Re-cloned the real repo fresh (`git clone https://github.com/orbitthegreatest/Orbitstrap.git`).
- **Discovered the repo has two parallel, non-identical source copies** (`Orbitstrap/` and
  `orbitstrap_modified/`), and only `orbitstrap_modified/` is actually referenced by `Orbitstrap.sln`.
  This wasn't caught in Conversation 2 — the step1/step2 diff-zips only patched `Orbitstrap/`,
  which means **those earlier diff-zips likely didn't actually affect the real built .exe.**
- Re-applied the Task 1C purple-outline fix to BOTH `Orbitstrap/` and `orbitstrap_modified/`.
- Re-applied the Task 1B new logo (.ico files) to BOTH `Orbitstrap/` and `orbitstrap_modified/`.
- Packaged everything (full source + black cursor PNGs + this master plan) into one zip.
- Stripped `.git/`, `bin/`, `obj/`, `.vs/` from the delivered source to keep the zip as small as
  reasonably possible (the repo's `Resources/` folders are still large — mostly bundled theme
  images/icons — this is inherent to the project, not something introduced by these changes).
**Next up (in order):**
- ❌ Task 2A logic: wire the black cursor checkbox + apply/restore code into the real Mods page
- ❌ Task 3A: Create "Orbitstrap-things" repo structure + manifest.json files
- ❌ Task 3B: Wire app to download from Orbitstrap-things repo on demand
- ❌ Task 2B: Emote wheel dropdown mod
- ❌ Task 2C: Skybox dropdown mod
- ❌ GitHub Actions workflow for automated .exe releases
- ❌ Website (Task 4A) + SEO/Search Console setup (Task 4B)
- ❌ Publishing guide (Task 5A)
- 🔄 Ongoing: user was asked whether `Orbitstrap/` or `orbitstrap_modified/` is the real copy to
  keep — they chose **"not sure, keep patching both for now"**, so every fix continues to be
  applied to both folders until they decide otherwise. Don't ask again unless they bring it up.

**Files delivered so far (already in user's hands):**
1. `orbitstrap_step1_fix_purple_outline.zip` (Conversation 2 — likely patched the wrong copy, superseded)
2. `orbitstrap_step2_new_logo.zip` (Conversation 2 — likely patched the wrong copy, superseded)
3. Conversation 3's full package zip (Task 1B + 1C applied to both source copies) — superseded by #4 below
4. This conversation's full package zip — supersedes all of the above, adds Task 1A on top

### Conversation 4 (this conversation — Task 1A)
**Completed:**
- ✅ Task 1A: Roblox account name + avatar in the sidebar, applied to BOTH `Orbitstrap/` and
  `orbitstrap_modified/` (user chose to keep patching both for now rather than pick one).
  Implementation reused the repo's existing `AccountManager.Shared` singleton and
  `ActiveAccountChanged` event (no new service class needed) plus a direct thumbnails-API call for
  the avatar image. Full details in the Task 1A spec above.
- Re-packaged the full project (source + black_cursor + this master plan) into one zip, per the
  "new zip after every completed step" rule.
**Next up (in order), unchanged from before except Task 1A is now done:**
- ❌ Task 2A logic: wire the black cursor checkbox + apply/restore code into the real Mods page
- ❌ Task 3A: Create "Orbitstrap-things" repo structure + manifest.json files
- ❌ Task 3B: Wire app to download from Orbitstrap-things repo on demand
- ❌ Task 2B: Emote wheel dropdown mod
- ❌ Task 2C: Skybox dropdown mod
- ❌ GitHub Actions workflow for automated .exe releases
- ❌ Website (Task 4A) + SEO/Search Console setup (Task 4B)
- ❌ Publishing guide (Task 5A)

**Files delivered so far (already in user's hands):**
1. `orbitstrap_step1_fix_purple_outline.zip` (Conversation 2 — likely patched the wrong copy, superseded)
2. `orbitstrap_step2_new_logo.zip` (Conversation 2 — likely patched the wrong copy, superseded)
3. Conversation 3's full package zip (Task 1B + 1C on both copies) — superseded by #4
4. Conversation 4's full package zip (added Task 1A) — superseded by #5
5. This conversation's full package zip — current, supersedes everything above (Task 1A + 1B + 1C + Task 6A build script cleanup)

### Conversation 10 (Build error fixes)
**Date:** 2026-08-09
**Completed:**
- User ran `build.bat` on their own Windows machine (first real build/publish attempt against
  this repo) and it failed with 4 compile errors, all pre-existing and unrelated to Task 1D/1E:
  - **3× `CS0234`** in `UI/Elements/Settings/MainWindow.xaml.cs` (both copies): `AccountManager.Shared`
    was being resolved against the empty `Orbitstrap.UI.Elements.AccountManager` namespace instead
    of the actual `Orbitstrap.Integrations.AccountManager` class, because C#'s unqualified-name
    lookup checks nested namespaces of the enclosing namespace (`Orbitstrap.UI.Elements`) before
    reaching for a class in a sibling namespace, and no `using` disambiguated it. This dates back to
    Task 1A's account-header work. **Fix:** added
    `using AccountManager = Orbitstrap.Integrations.AccountManager;` near the file's other
    `AccountManagerWindow` alias, in both `Orbitstrap/` and `orbitstrap_modified/`.
  - **1× `CS0120`** in `Bootstrapper.cs`: the `static` method `EnsureSkyboxDownloadedAsync` called
    the instance method `SetStatus(...)` (which needs a live `Dialog` instance tied to an in-progress
    Roblox launch). This dates back to Task 2C's skybox selector, and the call didn't make sense
    there anyway since this path is invoked from Settings, not from an active launch dialog.
    **Fix:** replaced `SetStatus("Downloading Skybox...")` with an `App.Logger.WriteLine(...)` call
    in both `Orbitstrap/` and `orbitstrap_modified/`.
- These two fixes account for exactly the 4 errors reported (3 + 1), so no further build errors are
  expected from this pass — but this still hasn't been build-verified end-to-end in this sandbox
  (still no Windows/.NET SDK here), so the user's next build attempt is the real test.
- Re-packaged the full project into one zip, per the "new zip after every completed step" rule.
**Next up (in order):**
- 🔄 User to re-run `build.bat` and confirm it now succeeds (or share any new errors).
- 🔄 User to create the `Orbitstrap-things` GitHub repo and push the populated folder — including
  all 34 zip files, not just the manifests (blocks Task 3A ✅).
- ❌ Task 2B/2C: now real risk they carry more of these latent static/namespace bugs since they were
  never build-verified before this — worth a closer read-through once the build is green.
- ❌ GitHub Actions workflow for automated .exe releases
- ❌ Website (Task 4A) + SEO/Search Console setup (Task 4B)
- ❌ Publishing guide (Task 5A)

### Conversation 9 (Task 1D + Task 1E)
**Date:** 2026-08-09
**Completed:**
- ✅ Task 1D: Removed `ModInjector.cs` (process-memory injector using remote offsets) and
  `LuaScriptManager.cs` (arbitrary Lua execution + native DLL loading against the Roblox process)
  entirely from both `Orbitstrap/` and `orbitstrap_modified/`, along with every setting,
  view-model binding, XAML toggle, and Bootstrapper call site tied to them, plus the now-unused
  `NLua` package reference. Verified with a repo-wide grep that nothing references them anymore.
  Full details in the Task 1D spec above.
- ✅ Task 1E: Designed a second logo (orbit-themed black planet + red/orange flame corona + orbit
  ring + white satellite dot), same red/black flame family as the existing mark but a distinct
  composition. Replaced `Orbitstrap.png`/`Orbitstrap.ico`/`Resources/IconOrbitstrap.ico` in both
  source copies and all four branding PNGs in `Images/`. Removed the two outdated app-showcase
  screenshots (`Images/showcase.png`, `Images/1748248817498.png`) — confirmed both were stale
  Voidstrap captures with no code/README references, so nothing else needed touching.
- Re-packaged the full project (source + Orbitstrap-things + this master plan) into one zip, per
  the "new zip after every completed step" rule.
**Next up (in order), unchanged from before except Tasks 1D/1E are now done:**
- 🔄 User to create the `Orbitstrap-things` GitHub repo and push the populated folder — including
  all 34 zip files, not just the manifests (blocks Task 3A ✅).
- ❌ Task 2B: Emote wheel dropdown mod (now has real assets to point at once the repo is live)
- ❌ Task 2C: Skybox dropdown mod (same — 24 real skyboxes ready once the repo is live)
- ❌ GitHub Actions workflow for automated .exe releases
- ❌ Website (Task 4A) + SEO/Search Console setup (Task 4B)
- ❌ Publishing guide (Task 5A)
- 🔄 Still open: confirm `build.bat` produces a working exe on a real Windows machine, and confirm
  the Task 2A Black Cursor mod compiles and works at runtime (both carried over, unconfirmed — no
  Windows/.NET SDK in this sandbox). Recommend a full `dotnet build` right after pulling this zip
  to make sure the Task 1D removals compiled cleanly, since that couldn't be verified here either.

### Conversation 8 (this conversation — Task 3A real assets)
**Date:** 2026-08-09
**Completed:**
- 🔄 Task 3A (still not ✅ — GitHub push is still on the user, see spec above): the user uploaded
  the real `Roblox-emote-weels-main.zip` and `nice-skyboxes-roblox-main.zip` source archives
  directly. Unpacked both, re-zipped each item with a clean slugified filename, and populated
  `Orbitstrap-things/emote-wheels/` (10 real zips) and `Orbitstrap-things/skyboxes/` (4 real zips)
  with them.
- Per the user's follow-up request ("add the old skyboxes too"): also re-zipped all 20 skyboxes
  already bundled inside the app itself (`Orbitstrap/Resources/Skyboxes/*`) and added them to
  `Orbitstrap-things/skyboxes/` alongside the 4 new ones — 24 skyboxes total now. The app's own
  embedded `.tex` copies were left untouched; these are separate re-zipped duplicates for the
  external repo/on-demand path.
- Rewrote both `manifest.json` files with the real filenames (still placeholder URL domain —
  can't resolve for real until the repo is actually pushed to GitHub) and rewrote
  `Orbitstrap-things/README.md` with the full real inventory.
- Re-packaged the full project (source + Orbitstrap-things with real assets + this master plan)
  into one zip, per the "new zip after every completed step" rule.
**Next up (in order):**
- 🔄 User to create the `Orbitstrap-things` GitHub repo and push the populated folder — including
  all 34 zip files, not just the manifests (blocks Task 3A ✅).
- ❌ Task 2B: Emote wheel dropdown mod (now has real assets to point at once the repo is live)
- ❌ Task 2C: Skybox dropdown mod (same — 24 real skyboxes ready once the repo is live)
- ❌ GitHub Actions workflow for automated .exe releases
- ❌ Website (Task 4A) + SEO/Search Console setup (Task 4B)
- ❌ Publishing guide (Task 5A)
- 🔄 Still open: confirm `build.bat` produces a working exe, and confirm the Task 2A Black Cursor
  mod compiles and works at runtime (both carried over, unconfirmed — no Windows/.NET SDK here).

### Conversation 8 (Task 2B)
**Date:** 2026-08-09
**Completed:**
- ✅ Task 2B: Emote wheel selector — wired into both `Orbitstrap/` and `orbitstrap_modified/`.
  See the Task 2B spec above for the full breakdown (`Utility/EmoteWheelMod.cs`,
  `SelectedEmoteWheel` setting, `ModsViewModel` dropdown wiring, `ModsPage.xaml` card). This also
  completes the emote-wheel half of Task 3B, since `EmoteWheelMod` already implements the
  manifest-fetch-then-download-on-select flow that Task 3B describes.
- Could not compile/run in the sandbox (no .NET SDK, WPF needs Windows) — static review only,
  same caveat as every C# task so far. Also can't functionally test the download path since the
  `Orbitstrap-things` repo (Task 3A) isn't actually live yet — the manifest URL will 404 until the
  user pushes that repo, so right now the dropdown will only show "None (Default)".
- Re-packaged the full project (source + Orbitstrap-things + this master plan) into one zip, per
  the "new zip after every completed step" rule.
**Next up (in order):**
- 🔄 User to create the `Orbitstrap-things` GitHub repo and push the populated folder (blocks
  Task 3A ✅, and blocks actually testing Task 2B's download path).
- ❌ Task 2C: Skybox dropdown mod (same pattern as 2B, completes Task 3B fully once done — note
  there's already a *different*, pre-existing skybox-pack feature in `ModsViewModel`
  (`LoadSkyboxPacksFromGithub`, pointed at a separate `KloBraticc/SkyboxPackV2` repo) worth
  checking against before building 2C, so the two don't collide or confuse users with two
  overlapping skybox pickers).
- ❌ GitHub Actions workflow for automated .exe releases
- ❌ Website (Task 4A) + SEO/Search Console setup (Task 4B)
- ❌ Publishing guide (Task 5A)
- 🔄 Still open: confirm `build.bat` produces a working exe, and confirm the Task 2A Black Cursor
  mod and Task 2B Emote Wheel mod both compile and work at runtime (all carried over,
  unconfirmed — no Windows/.NET SDK here).

### Conversation 7 (Task 3A)
**Date:** 2026-08-09
**Completed:**
- 🔄 Task 3A (partial — see spec above for exactly what's left): scaffolded the
  `Orbitstrap-things/` folder (README + `emote-wheels/manifest.json` +
  `skyboxes/manifest.json`, populated with the id/name of every wheel and skybox named in the
  Task 2B/2C specs) and added it to the delivered zip. **Could not create or push to an actual
  GitHub repo** — no network/credentials in this sandbox — so the manifest `url` fields are
  clearly-labeled placeholders and the task stays 🔄, not ✅, until the user does the GitHub side.
- Cleanup: removed the now-redundant top-level `black_cursor/` folder from the package, per the
  master plan's own cleanup rule — those PNGs are embedded resources inside `Orbitstrap-source/`
  now (Task 2A), so the loose staging copy was dead weight.
- Re-packaged the full project (source + Orbitstrap-things scaffold + this master plan) into one
  zip, per the "new zip after every completed step" rule.
**Next up (in order):**
- 🔄 User to create the `Orbitstrap-things` GitHub repo and push the scaffold (blocks Task 3A ✅).
- ❌ Task 2B: Emote wheel dropdown mod (naturally includes the Task 3B download-wiring for this category)
- ❌ Task 2C: Skybox dropdown mod (same — includes Task 3B wiring for this category)
- ❌ GitHub Actions workflow for automated .exe releases
- ❌ Website (Task 4A) + SEO/Search Console setup (Task 4B)
- ❌ Publishing guide (Task 5A)
- 🔄 Still open: confirm `build.bat` produces a working exe, and confirm the Task 2A Black Cursor
  mod compiles and works at runtime (both carried over, unconfirmed — no Windows/.NET SDK here).

### Conversation 6 (Task 2A)
**Date:** 2026-08-09
**Completed:**
- ✅ Task 2A: Wired the Black Cursor mod into both `Orbitstrap/` and `orbitstrap_modified/`:
  - Copied the 5 PNGs from `black_cursor/content/textures/...` into each project's
    `Resources/BlackCursor/` folder and added them to the `.csproj` as WPF `<Resource>` items
    (embedded pack resources, same pattern already used for skyboxes/icons in this repo).
  - Added `Utility/BlackCursorMod.cs` (`public static class BlackCursorMod`) with
    `Apply()`/`Remove()`: `Apply()` reads each embedded PNG via
    `Application.GetResourceStream(pack://application:,,,/Resources/BlackCursor/...)` and writes
    it to `Paths.Mods\content\textures\...`; `Remove()` deletes just those specific files (not
    the whole folder) since the "Custom Cursor Set" feature already writes to the exact same
    paths (`content\textures\MouseLockedCursor.png` and
    `content\textures\Cursors\KeyboardMouse\{ArrowCursor,ArrowFarCursor,IBeamCursor}.png`) — the
    two features are mutually exclusive by nature (last one applied wins), which is called out in
    the UI description rather than hidden.
  - Added `UseBlackCursorMod` bool to `Models/Persistable/AppSettings.cs` (persisted setting,
    defaults to `false`).
  - Added a `UseBlackCursorMod` property to `ModsViewModel.cs` (both copies) following the same
    pattern as `UseDarkTextureMod` — setter calls `BlackCursorMod.Apply()`/`Remove()` and
    surfaces failures via `App.Logger` (+ a message box in `orbitstrap_modified`, matching how
    that copy already handles the Dark Texture Mod's errors).
  - Added a new "Black Cursor Mod" `CardExpander` with a toggle switch to `ModsPage.xaml` (both
    copies), placed directly under the existing Dark Texture Mod card.
- Could not compile/run this in the sandbox (no .NET SDK, WPF needs Windows — same limitation
  noted for Task 6A). Static review only: verified the resource URIs, `Paths.Mods` usage, and
  binding names all match existing conventions in the file. Flagging as a TODO for the user to
  build-verify on Windows, same as the still-open Task 6A item below.
- Re-packaged the full project into one zip, per the "new zip after every completed step" rule.
**Next up (in order), unchanged from before except Task 2A is now done:**
- ❌ Task 3A: Create "Orbitstrap-things" GitHub repo structure + manifest.json files
- ❌ Task 3B: Wire app to download from Orbitstrap-things repo on demand
- ❌ Task 2B: Emote wheel dropdown mod
- ❌ Task 2C: Skybox dropdown mod
- ❌ GitHub Actions workflow for automated .exe releases
- ❌ Website (Task 4A) + SEO/Search Console setup (Task 4B)
- ❌ Publishing guide (Task 5A)
- 🔄 Still open: confirm `build.bat` actually produces a working exe on a real Windows machine
  (carried over from Task 6A), and now also confirm the new Black Cursor mod toggle compiles and
  actually applies/removes files correctly at runtime.

### Conversation 5 (Task 6A)
**Completed:**
- ✅ Task 6A: Deleted `BUILD_ME_FIRST.bat`, renamed `BUILD_AND_PUBLISH.bat` → `build.bat`.
  Verified statically (grep across the repo, `.sln`/`.csproj` path checks, target-framework
  compatibility check) that nothing referenced the deleted file and that `build.bat` correctly
  targets the real buildable project. Could not run a live `dotnet build`/`publish` in this
  conversation's sandbox (no .NET SDK here, WPF needs Windows) — flagged as a TODO for the user to
  confirm on their own machine. Full details in the Task 6A spec above.
- Re-packaged the full project into one zip, per the "new zip after every completed step" rule.

### Conversation 6 (Build error fix)
**Completed:**
- 🐛 **Build fix:** `orbitstrap_modified\Orbitstrap.csproj` (the one `Orbitstrap.sln` actually
  builds) failed with `CS0234: The type or namespace name 'Shared' does not exist in the
  namespace 'Orbitstrap.UI.Elements.AccountManager'` at `UI/Elements/Settings/MainWindow.xaml.cs`
  lines 202/204/205.
  **Root cause:** the file declares `using AccountManager = Orbitstrap.Integrations.AccountManager;`
  (an alias to the real account-state singleton) *and* separately `using AccountManagerWindow =
  Orbitstrap.UI.Elements.AccountManager.MainWindow;`. Because the file's own namespace
  (`Orbitstrap.UI.Elements.Settings`) is nested inside `Orbitstrap.UI.Elements`, and
  `Orbitstrap.UI.Elements.AccountManager` is a *sibling* child namespace of that same parent, C#'s
  name-resolution rules let that sibling namespace win over the using-alias when both are named
  `AccountManager` — so `AccountManager.Shared` silently resolved to the namespace instead of the
  intended class, which has no `Shared` member.
  **Fix:** renamed the alias to `AccountManagerService` (no longer collides with anything) and
  updated the three call sites (`AccountManagerService.Shared.ActiveAccountChanged` ×2,
  `AccountManagerService.Shared.ActiveAccount`). Applied identically to **both**
  `Orbitstrap/UI/Elements/Settings/MainWindow.xaml.cs` and
  `orbitstrap_modified/UI/Elements/Settings/MainWindow.xaml.cs`, per the "fix both copies" rule.
  Grepped the whole repo for any other file importing `AccountManager` as an alias — only these
  two files did, so no other file is affected.
  **Not verified with a real compile:** this sandbox has no .NET SDK and can't build a Windows WPF
  target, so this was fixed by reading the C# name-resolution rules and tracing the exact
  identifiers, not by re-running `dotnet build`. **Please rebuild on your Windows machine and
  confirm CS0234 is gone before moving on.**
- ⚠️ **Found a documentation inconsistency, not yet resolved:** the previous "Conversation 5" log
  entry above lists Task 2A logic, 3A, 3B, 2B, and 2C as still `❌` "next up", but the master
  checklist earlier in this file marks all five as `✅ DONE`. This conversation did **not** re-verify
  which is actually true in the real code — flagging this for the next conversation (or the user)
  to confirm before trusting either list blindly.
- Re-packaged the full project into one zip, per the "new zip after every completed step" rule.

### Conversation 7 (Emote wheel bug + blurry logo, reported by user)
**Completed (applied to BOTH `Orbitstrap/` and `orbitstrap_modified/`):**
- 🐛 **Emote wheel always applying "Cute Bears" regardless of selection:** `ModsViewModel.
  SelectedEmoteWheelId`'s setter fires `_ = Task.Run(...)` on every change, and `EmoteWheelMod.
  ApplyAsync` downloaded/extracted through *fixed* temp file names
  (`orbitstrap-emotewheel.zip` / `orbitstrap-emotewheel-extract`) with no locking. If the
  dropdown fires more than one selection change in quick succession — including a possible
  spurious first change while `EmoteWheelOptions` is still being populated at app start — two
  `ApplyAsync` calls could run concurrently and stomp on each other's temp files, so the wheel
  that actually landed in the Mods folder wasn't reliably the last one you picked.
  **Fix:** added a generation counter + `SemaphoreSlim` in `ModsViewModel` so apply calls are
  serialized and a stale in-flight request (superseded by a newer selection before it got to run)
  is skipped entirely instead of writing anything to disk. Also switched `EmoteWheelMod.
  ApplyAsync`'s temp zip/extract paths to a per-call GUID as defense-in-depth, so even a future
  concurrency bug elsewhere can't cross-contaminate two downloads.
  **Not fully confirmed:** couldn't run the app in this sandbox (no .NET/Windows here) to watch
  the race actually happen — this is the most concrete, plausible mechanism found by reading the
  code, but please confirm on your machine that switching wheels a few times quickly always
  applies the one you last picked.
- 🐛 **Blurry sidebar logo, root cause found and fixed:** `Orbitstrap.ico` and
  `Resources/IconOrbitstrap.ico` (in both source copies) turned out to contain **only a single
  16×16 frame** (761 bytes each), even though Task 1E's notes claimed a "multi-res 16–256px" icon
  had been produced — it hadn't; only a placeholder-sized frame ever got written into the `.ico`
  container. `MainWindow.xaml`'s `AppIconImage` resource decodes that icon at
  `DecodePixelWidth="256" DecodePixelHeight="256"`, so the sidebar was upscaling a 16×16 bitmap to
  256×256, which is exactly what blurry logos look like. Confirmed by comparing against
  `Resources/OldIconOrbitstrap.ico`, which correctly contains all six sizes (16/32/48/64/128/256)
  and is 85KB.
  **Fix:** regenerated both `.ico` files from the existing 256×256 `Orbitstrap.png` source with
  all six standard sizes embedded, replacing the broken 16×16-only files. No code/XAML changes
  needed — the XAML was already asking for 256px, the icon just never actually had it.
- ⚠️ **Did not yet investigate:** the screenshot you sent also shows the Emote Wheel Selector
  ComboBox rendering with no visible item text and what looks like a doubled chevron/arrow. WPF-UI's
  bundled `ComboBox.xaml` style (in `wpfui/src/Wpf.Ui/Styles/Controls/ComboBox.xaml`) has a
  `<!-- TODO: Refactor editable and fix borders -->` comment at the top, suggesting this is a
  known-incomplete control upstream — but this wasn't confirmed live (this sandbox can't render
  WPF), so treat this as an unverified lead, not a fix. **Next conversation: reproduce this on
  Windows, try swapping the plain `<ComboBox>` in `ModsPage.xaml` for a version with explicit
  `Height`/`Padding` overrides (or check for a newer `Wpf.Ui` release upstream that patches this
  control), and confirm whether it's actually the same root cause as the "always applies the first
  item" bug or a fully separate rendering issue.**
- Re-packaged the full project into one zip, per the "new zip after every completed step" rule.

### Conversation 8 (Duplicate source folder cleanup — RESOLVED)
**Completed:**
- ✅ Confirmed via `Orbitstrap.sln` (which folder is actually referenced) and a full `diff -rq`
  between `Orbitstrap/` and `orbitstrap_modified/` that `orbitstrap_modified` was the live,
  more-developed copy and `Orbitstrap` was a stale orphan.
- Deleted `Orbitstrap/`, renamed `orbitstrap_modified/` → `Orbitstrap/`.
- Fixed every path reference repo-wide: `Orbitstrap.sln`, `build.bat`,
  `.github/workflows/release.yml`, `PUBLISHING_GUIDE.md`.
- Re-verified with `grep -r orbitstrap_modified` across all build/config/doc files — clean.
- **Not yet done:** a real `dotnet build`/`dotnet restore` on a Windows machine to confirm the
  rename didn't break anything (this sandbox has no .NET SDK). Please build once on your machine
  before publishing a release off this.
- Re-packaged the full project into one zip, per the "new zip after every completed step" rule.

**Next up, per the master checklist (assuming it's accurate — see inconsistency note above):**
- ❌ Task 4A: Website HTML/CSS/JS — needs user input first (domain name? hosting preference beyond
  the "Vercel" mention in Task 4B? tone/copy? which existing logo/branding asset to use?)
- ✅ Task 4A: Website HTML/CSS/JS — Mods section, scroll-reveal, particle field, cursor glow added.
- ⚠️ Task 4B: SEO & Google Search Console setup — sitemap/robots/Google verification file already
  existed pre-Conversation-9; not re-verified this round.
- ✅ Task 5A: Step-by-step publishing guide — v3.2.0 walkthrough section added.
- 🔄 Still open: confirm `build.bat` actually produces a working exe on a real Windows machine.
- 🔄 Still open: confirm a real `dotnet build` succeeds after the folder rename (Conversation 8).

### Conversation 9 (Three bug fixes + website Mods/animation + publishing docs)
**Reported by user:**
1. Linking a Roblox account, closing, and reopening the app loses the linked account.
2. Duplicate `SkyboxPack`/`Skyboxes` folders (previously claimed fixed — it wasn't).
3. A stray `.orbitstrap_emotewheel_files.json` file showing up in the Mods folder.
4. Update the website with more info/features, refresh the logo, add smooth animated UI.
5. Add the website link to the GitHub README; guide publishing the source + a v3.2.0 release.

**Completed:**
- 🐛 **Bug 1 fixed — Roblox account not persisting:** `Paths.Cache` was defined but never
  actually created in `Paths.Initialize()`, so `AccountManager.SaveAccounts()` writing into
  `Paths.Cache\AccountManager.json` threw a `DirectoryNotFoundException` that was silently
  swallowed — nothing ever reached disk. Fixed by adding `EnsureDirectoryExists(Cache)` to
  `Paths.Initialize()`, plus a defensive directory-create directly in `SaveAccounts()`.
- 🐛 **Bug 2 fixed — duplicate SkyboxPack/Skyboxes folders:** Removed the dead, always-empty
  `Paths.Skyboxes` property and its creation call (real skybox downloads go to the separate
  `SkyboxPack` folder in `Bootstrapper.cs`, confirmed via repo-wide grep). Added a one-time
  migration that deletes the stale empty `Skyboxes` folder from existing installs.
- 🐛 **Bug 3 fixed — stray tracker file:** `.orbitstrap_emotewheel_files.json` was being written
  into the user-visible `OrbitstrapMods` folder. Moved it to the internal `Paths.Cache` folder
  (same place other internal caches live), with automatic migration of any existing tracker file.
- Website: added a scroll-reveal system (`.reveal` + `IntersectionObserver`), an ambient canvas
  particle field in the hero, and a cursor-glow effect — all disabled under
  `prefers-reduced-motion`. Added a new "Mods" showcase section (Black Cursor, Emote Wheel
  Selector, Skybox Selector, Korblox/Headless, Multi-Account Manager, Themes) with a matching nav
  link. Moved `website/` into the git repo (it previously lived outside it as a sibling folder,
  which didn't match `PUBLISHING_GUIDE.md`'s assumption that it's at `website/`).
- README: added a Website link next to the Download link.
- PUBLISHING_GUIDE.md: added a concrete "Walkthrough: shipping v3.2.0" section referencing this
  conversation's specific fixes, and renumbered the trailing checklist section to §9.
- Committed everything to the git repo (bug fixes + docs + website-now-in-repo) in one commit.
- **Not yet done — logo refresh:** the user asked for an updated/refined logo; this conversation
  did not regenerate `Orbitstrap.ico`/`Orbitstrap.png`/`orbitstrap-mark.svg`. Flagging this for
  the next conversation rather than rushing a redesign.
- **Not yet done — Google Search Console / faster-indexing walkthrough:** the site's
  `sitemap.xml`, `robots.txt`, and Google verification HTML file already existed from before this
  conversation; the actual step-by-step "submit to Search Console" guidance the user asked for
  was not written this round.
- **Not verified with a real compile:** this sandbox has no .NET SDK and can't build a Windows
  WPF target — the three C# fixes were made by reading the code and tracing exact call paths, not
  by re-running `dotnet build`. **Please rebuild on your Windows machine and confirm all three
  bugs are actually resolved (relink an account and restart the app; check the Mods folder for
  the tracker file's new location; confirm no `Skyboxes` folder gets recreated) before tagging
  v3.2.0.**

**Next up:**
- ❌ Logo refresh (deferred from this conversation — needs a real design pass, not a rush job).
- ❌ Step-by-step Google Search Console walkthrough (sitemap/robots/verification file already
  exist; the guided submission steps still need writing).
- 🔄 Still open: confirm `build.bat` produces a working exe and `dotnet build` succeeds, on a real
  Windows machine — same open item as Conversation 8, still not verified from this sandbox.

---

## 💡 TIPS FOR NEXT AI IN NEXT CONVERSATION

1. **Always start** by re-reading this MASTER_PLAN.md (it's inside the zip you were given).
2. Orbitstrap is **C# WPF .NET 8** — use modern C# syntax.
3. UI library is **WPF-UI (Wpf.Ui NuGet)** — use `Wpf.Ui.Controls.*` components.
4. Settings are stored as **JSON** in `%LocalAppData%\Orbitstrap\Settings.json`.
5. The app follows **Bloxstrap's** architecture pattern.
6. Mods are applied to **`%LocalAppData%\Roblox\Versions\[version]\`**.
7. **The repo has two parallel source folders — always check `Orbitstrap.sln` to see which
   `.csproj` is actually referenced before assuming which folder to edit** (see the discovery
   note above). Apply fixes to both until the user says it's safe to delete one.
8. Deliver **one full-package zip per conversation** (source + black_cursor + website-if-built +
   this file), not tiny diffs — see the Workflow section at the top. **Deliver a new zip after
   EVERY completed step/task, not just once at the end of the conversation.**
9. Ask the user to provide any specific XAML or CS file contents if code won't compile.
10. **Always update the status markers** (❌→✅) in this document after completing tasks, inside
    the zip you deliver.
