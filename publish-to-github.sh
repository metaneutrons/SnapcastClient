#!/bin/bash

# Script to manually publish SnapCastNet package to GitHub Packages
# Usage: ./publish-to-github.sh [GITHUB_TOKEN]

set -e

# Check if GitHub token is provided
if [ -z "$1" ]; then
    echo "Usage: $0 <GITHUB_TOKEN>"
    echo "You can get a GitHub token from: https://github.com/settings/tokens"
    echo "Required scopes: write:packages, read:packages"
    exit 1
fi

GITHUB_TOKEN=$1
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
dotnet nuget push packages/*.nupkg --source "$PACKAGE_SOURCE" --api-key "$GITHUB_TOKEN"

echo "Package published successfully!"
echo ""
echo "To consume this package, users should run:"
echo "dotnet nuget add source https://nuget.pkg.github.com/metaneutrons/index.json --name github-snapcast-net"
echo "dotnet add package SnapCastNet --source github-snapcast-net"
