@echo off
echo ==========================================
echo   Orbitstrap -- One-Time Setup
echo ==========================================
echo.
echo Restoring NuGet packages (needed once)...
dotnet restore Orbitstrap.sln
if %errorlevel% neq 0 (
    echo.
    echo ERROR: Restore failed. Make sure .NET 10 SDK is installed.
    pause
    exit /b 1
)
echo.
echo Building...
dotnet build Orbitstrap.sln -c Debug
if %errorlevel% neq 0 (
    echo.
    echo Build failed - check errors above.
    pause
    exit /b 1
)
echo.
echo ==========================================
echo   Done! Exe is at:
echo   orbitstrap_modified\bin\Debug\net10.0-windows\Orbitstrap.exe
echo ==========================================
pause
