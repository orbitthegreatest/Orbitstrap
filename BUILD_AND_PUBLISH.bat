@echo off
setlocal

echo ==========================================
echo   Orbitstrap -- Build and Publish
echo ==========================================
echo.

:: Change to the folder containing this script
cd /d "%~dp0"

:: Check dotnet is available
where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK not found. Download from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [1/3] Restoring NuGet packages...
dotnet restore Orbitstrap.sln
if errorlevel 1 (
    echo Restore failed. Check the errors above.
    pause
    exit /b 1
)

echo.
echo [2/3] Publishing single-file exe...
dotnet publish orbitstrap_modified\Orbitstrap.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeAllContentForSelfExtract=true ^
    -o publish_output
if errorlevel 1 (
    echo Publish failed. Check errors above.
    pause
    exit /b 1
)

echo.
echo [3/3] Done!
echo.
echo Your exe is at:
echo   %~dp0publish_output\Orbitstrap.exe
echo.
echo ==========================================
echo   To publish to GitHub:
echo ==========================================
echo.
echo 1. Install GitHub CLI from: https://cli.github.com
echo 2. Run these commands:
echo.
echo    gh auth login
echo    gh release create v1.1.0 "publish_output\Orbitstrap.exe" ^
echo        --repo orbitthegreatest/Orbitstrap ^
echo        --title "Orbitstrap v1.1.0" ^
echo        --notes "First public release"
echo.
echo Or manually: go to github.com/orbitthegreatest/Orbitstrap
echo   Releases - Create release - drag the exe in - Publish
echo.
pause
