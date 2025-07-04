#!/bin/bash

echo "========================================"
echo " AWS WorkSpace Manager - Quick Setup"
echo "========================================"
echo

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if .NET 8.0 is installed
echo "[1/4] Checking .NET 8.0 installation..."
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}ERROR: .NET 8.0 SDK is not installed or not in PATH${NC}"
    echo "Please download and install from: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✓ .NET version $DOTNET_VERSION found${NC}"

# Check if we're in the right directory
if [ ! -f "WorkSpaceManager.sln" ]; then
    echo -e "${RED}ERROR: WorkSpaceManager.sln not found in current directory${NC}"
    echo "Please run this script from the solution root directory"
    exit 1
fi

echo
echo "[2/4] Restoring NuGet packages..."
if ! dotnet restore; then
    echo -e "${RED}ERROR: Failed to restore NuGet packages${NC}"
    exit 1
fi
echo -e "${GREEN}✓ NuGet packages restored successfully${NC}"

echo
echo "[3/4] Building the solution..."
if ! dotnet build --no-restore; then
    echo -e "${RED}ERROR: Build failed${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Solution built successfully${NC}"

echo
echo "[4/4] Creating database..."
if ! dotnet ef database update --no-build 2>/dev/null; then
    echo -e "${YELLOW}WARNING: Database update failed. This is normal if Entity Framework tools are not installed.${NC}"
    echo -e "${YELLOW}The database will be created automatically when you first run the application.${NC}"
fi

echo
echo "========================================"
echo " Setup Complete!"
echo "========================================"
echo
echo "Next steps:"
echo "1. Update your AWS settings in appsettings.json"
echo "2. Run the application with: dotnet run"
echo "3. Open your browser to: https://localhost:5001"
echo
echo "For detailed instructions, see: GETTING_STARTED_GUIDE.md"
echo

# Make the script executable
chmod +x setup.sh