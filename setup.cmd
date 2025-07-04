@echo off
echo ========================================
echo  AWS WorkSpace Manager - Quick Setup
echo ========================================
echo.

REM Check if .NET 8.0 is installed
echo [1/4] Checking .NET 8.0 installation...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET 8.0 SDK is not installed or not in PATH
    echo Please download and install from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

for /f "tokens=1" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo ✓ .NET version %DOTNET_VERSION% found

REM Check if we're in the right directory
if not exist "WorkSpaceManager.sln" (
    echo ERROR: WorkSpaceManager.sln not found in current directory
    echo Please run this script from the solution root directory
    pause
    exit /b 1
)

echo.
echo [2/4] Restoring NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore NuGet packages
    pause
    exit /b 1
)
echo ✓ NuGet packages restored successfully

echo.
echo [3/4] Building the solution...
dotnet build --no-restore
if %errorlevel% neq 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo ✓ Solution built successfully

echo.
echo [4/4] Creating database...
dotnet ef database update --no-build
if %errorlevel% neq 0 (
    echo WARNING: Database update failed. This is normal if Entity Framework tools are not installed.
    echo The database will be created automatically when you first run the application.
)

echo.
echo ========================================
echo  Setup Complete!
echo ========================================
echo.
echo Next steps:
echo 1. Update your AWS settings in appsettings.json
echo 2. Run the application with: dotnet run
echo 3. Open your browser to: https://localhost:5001
echo.
echo For detailed instructions, see: GETTING_STARTED_GUIDE.md
echo.
pause