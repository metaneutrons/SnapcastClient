#!/bin/bash

# Build and pack SnapcastClient for local development
# This script builds the project and automatically copies the package to ~/.nuget/local

set -e

echo "🔨 Building SnapcastClient for local development..."

# Navigate to the source directory
cd "$(dirname "$0")/src"

# Clean previous builds
echo "🧹 Cleaning previous builds..."
dotnet clean --configuration Release

# Restore packages
echo "📦 Restoring packages..."
dotnet restore

# Build the project
echo "🏗️  Building project..."
dotnet build --configuration Release --no-restore

# Pack the project (this will automatically copy to local NuGet due to our MSBuild target)
echo "📦 Creating NuGet package..."
dotnet pack --configuration Release --no-build

echo "✅ Build complete! Package has been copied to ~/.nuget/local"
echo ""
echo "To use in other projects:"
echo "  dotnet add package SnapcastClient --source local"
echo ""
echo "To list local packages:"
echo "  ls -la ~/.nuget/local"
