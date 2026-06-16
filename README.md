<p align="center">
<img src="Images/Orbitstrap.png" alt="Orbitstrap" width="100px"/>
</p>

<h1 align="center"><b>Orbitstrap Ultimate</b></h1>
<p align="center">The most feature-complete Orbitstrap build — combining fixed8 and fflagsfix2</p>

---

## What's in this Ultimate Build

### From **Orbitstrap-fixed8** (base)
- ✅ GPU Overclocking (GPUOverclocker.cs)
- ✅ Lua Script Manager (LuaScriptManager.cs)
- ✅ SwiftTunnel VPN Integration
- ✅ Play Time Watcher
- ✅ 158+ FFlag Preset Mappings (FastFlagManager)
- ✅ Profile Manager
- ✅ 7 Cursor Sets (Stoofs, Clean, FPS, Classic 2006/2013, WhiteDot, Dot)
- ✅ Bundled Flag Pack Presets (Stoof, Mahorgas, Obsurd/Unterial)
- ✅ Nvidia Profile Settings
- ✅ Motion Blur / Camera Motion Detection
- ✅ RobloxDX12 utility
- ✅ Integration Watcher
- ✅ Multi-language support (25 locales)
- ✅ Mod Generator
- ✅ In-game Resolution Control
- ✅ CPU Core Limiter
- ✅ Activity Watcher with DRP

### From **Orbitstrap-fflagsfix2** (merged in)
- ✅ **Bundled local skyboxes** (20 packs: Blue, NeonSky, PurpleVoid, Pandora, etc.)
- ✅ `Paths.Skyboxes` for local skybox directory management
- ✅ `MemoryCleanerIntervalOption` model
- ✅ `MultiInstanceWatcher` for multi-instance Roblox
- ✅ `ModInjector` (clean rename)
- ✅ GBSPresets (FontSize enum)
- ✅ Additional fflags: `Rendering.ManualFullscreen`, `Rendering.MSAA`
- ✅ Extra settings: `MemoryCleanerIntervalSeconds`, `TrimRobloxMemory`, `SkyboxEnabled`

---

## Building

### Prerequisites
- Windows 10/11
- Visual Studio 2022 + .NET Desktop workload
- .NET 6.0 SDK

### Steps
```
dotnet restore
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

---

## Credits
- Based on [Orbitstrap](https://github.com/orbitstrap/Orbitstrap) by KloBraticc
- fflagsfix2 branch improvements merged
