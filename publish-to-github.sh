#!/bin/bash

# Script to manually publish SnapCastNet package to GitHub Packages
# Usage: ./publish-to-github.sh [GITHUB_TOKEN]
# 
# The script will use GITHUB_TOKEN environment variable by default.
# If not set, you can provide the token as a command line argument.

set -e

# Check for GitHub token - environment variable first, then CLI parameter
if [ -n "$GITHUB_TOKEN" ]; then
    echo "Using GITHUB_TOKEN from environment variable"
    TOKEN="$GITHUB_TOKEN"
elif [ -n "$1" ]; then
    echo "Using GITHUB_TOKEN from command line parameter"
    TOKEN="$1"
else
    echo "Error: No GitHub token provided!"
    echo ""
    echo "Usage: $0 [GITHUB_TOKEN]"
    echo ""
    echo "You can either:"
    echo "  1. Set the GITHUB_TOKEN environment variable:"
    echo "     export GITHUB_TOKEN=your_github_token"
    echo "     $0"
    echo ""
    echo "  2. Or provide the token as a parameter:"
    echo "     $0 your_github_token"
    echo ""
    echo "You can get a GitHub token from: https://github.com/settings/tokens"
    echo "Required scopes: write:packages, read:packages"
    exit 1
fi

PACKAGE_SOURCE="https://nuget.pkg.github.com/metaneutrons/index.json"

echo "Building project in Release configuration..."
cd src
dotnet build --configuration Release

echo "Running tests..."
dotnet test --configuration Release --no-build

echo "Creating package..."
dotnet pack snapcast-net/snapcast-net.csproj --no-build --configuration Release --output ../packages

echo "Publishing to GitHub Packages..."
cd ..
dotnet nuget push packages/*.nupkg --source "$PACKAGE_SOURCE" --api-key "$TOKEN"

echo "Package published successfully!"
echo ""
echo "To consume this package, users should run:"
echo "dotnet nuget add source https://nuget.pkg.github.com/metaneutrons/index.json --name github-snapcast-net"
echo "dotnet add package SnapCastNet --source github-snapcast-net"
