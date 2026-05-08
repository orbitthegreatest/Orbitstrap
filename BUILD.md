# Building Orbitstrap

## Prerequisites
- Windows 10/11
- Visual Studio 2022 (Community or higher) **with:**
  - .NET Desktop Development workload
  - .NET 6.0 SDK
- OR: .NET 6 SDK + command line

## Steps

### Option A — Visual Studio
1. Open `Orbitstrap.csproj` in Visual Studio 2022
2. Right-click the project → **Restore NuGet Packages**
3. Add your `Orbitstrap.ico` to `Orbitstrap/Resources/`
4. Press **F5** to build and run

### Option B — Command Line
```
dotnet restore
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```
The output exe will be in `bin\Release\net6.0-windows\win-x64\publish\Orbitstrap.exe`

## What's in this build
- **Base**: Velostrap (full decompiled & renamed to Orbitstrap)
- **Added from Voidstrap**: Extensions page (Fleasion + AniWatch), Skybox Manager, cool FluentDialog loading screen
- **Theme**: Red & Black (Orbitstrap.xaml — auto-loaded on startup)
- **Branding**: All "Velostrap" text replaced with "Orbitstrap"
- **Cleaned**: `niggainject` renamed to `ModInjector`

## Folder Structure
```
Orbitstrap/
├── Orbitstrap.csproj          ← Project file
├── BUILD.md                   ← This file
└── Orbitstrap/
    ├── App.cs / App.xaml      ← Entry point + branding
    ├── Bootstrapper.cs        ← Core launch logic
    ├── Models/Persistable/
    │   └── Settings.cs        ← All settings (+ FleasionEnabled, SkyboxName etc.)
    ├── UI/
    │   ├── Style/
    │   │   ├── Orbitstrap.xaml  ← Red/black theme ★
    │   │   ├── dark.xaml
    │   │   └── default.xaml
    │   └── Elements/Settings/Pages/
    │       ├── ExtensionPage.xaml/.cs  ← Fleasion + AniWatch ★
    │       ├── ModsPage.xaml           ← + Skybox Manager ★
    │       └── ... (all Velostrap pages)
    └── Resources/
        └── Orbitstrap.ico     ← Add your icon here
```
