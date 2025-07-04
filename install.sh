#!/bin/bash
# AWS WorkSpaces Manager Installation Script

set -e

echo "🚀 Installing AWS WorkSpaces Manager..."

# Check if Python 3.8+ is available
if ! command -v python3 &> /dev/null; then
    echo "❌ Python 3 is required but not installed."
    exit 1
fi

PYTHON_VERSION=$(python3 -c 'import sys; print(".".join(map(str, sys.version_info[:2])))')
REQUIRED_VERSION="3.8"

if [ "$(printf '%s\n' "$REQUIRED_VERSION" "$PYTHON_VERSION" | sort -V | head -n1)" != "$REQUIRED_VERSION" ]; then
    echo "❌ Python 3.8+ is required. Found Python $PYTHON_VERSION"
    exit 1
fi

echo "✅ Python $PYTHON_VERSION found"

# Check if pip is available
if ! command -v pip3 &> /dev/null; then
    echo "❌ pip3 is required but not installed."
    exit 1
fi

echo "✅ pip3 found"

# Install dependencies
echo "📦 Installing Python dependencies..."
pip3 install -r requirements.txt

# Install the package in development mode
echo "📦 Installing AWS WorkSpaces Manager..."
pip3 install -e .

# Make the main script executable
chmod +x aws_workspace_manager.py

echo ""
echo "🎉 Installation complete!"
echo ""
echo "Quick start:"
echo "  1. Configure: aws-workspace-manager configure"
echo "  2. Login:     aws-workspace-manager login"
echo "  3. List:      aws-workspace-manager list"
echo ""
echo "For help:      aws-workspace-manager --help"
echo "Short alias:   awsm --help"