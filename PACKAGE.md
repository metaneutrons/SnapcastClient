# SnapcastClient Package Installation

This document explains how to install and use the SnapcastClient package from GitHub Packages.

## Installation

### Step 1: Add GitHub Packages Source

```bash
dotnet nuget add source https://nuget.pkg.github.com/metaneutrons/index.json --name github-snapcast-client
```

### Step 2: Install the Package

```bash
dotnet add package SnapcastClient --source github-snapcast-client
```

### Alternative: Using PackageReference

Add this to your `.csproj` file:

```xml
<PackageReference Include="SnapcastClient" Version="1.0.0" />
```

And add this `nuget.config` to your project root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github-snapcast-client" value="https://nuget.pkg.github.com/metaneutrons/index.json" />
  </packageSources>
</configuration>
```

## Authentication (if needed)

If you need to authenticate with GitHub Packages, you'll need a GitHub Personal Access Token with `read:packages` scope.

### Option 1: Environment Variable
```bash
export NUGET_AUTH_TOKEN=your_github_token
```

### Option 2: NuGet Config
Add to your `nuget.config`:

```xml
<packageSourceCredentials>
  <github-snapcast-client>
    <add key="Username" value="your_github_username" />
    <add key="ClearTextPassword" value="your_github_token" />
  </github-snapcast-client>
</packageSourceCredentials>
```

## Publishing (for maintainers)

To publish a new version of the package:

### Option 1: Using Environment Variable (Recommended)
```bash
export GITHUB_TOKEN=your_github_token
./publish-to-github.sh
```

### Option 2: Using Command Line Parameter
```bash
./publish-to-github.sh your_github_token
```

The script will automatically:
1. Build the project in Release configuration
2. Run all tests
3. Create the NuGet package
4. Publish to GitHub Packages

## Usage

```csharp
using SnapcastClient;

var connection = new TcpConnection("127.0.0.1", 1705);
var client = new Client(connection);

// Get server version
var version = await client.ServerGetRpcVersionAsync();
Console.WriteLine($"Server version: {version.Major}.{version.Minor}.{version.Patch}");

// Control streams
await client.StreamPlayAsync("Spotify");
await client.StreamSetVolumeAsync("Spotify", 80);
```

## Package Information

- **Package ID**: SnapcastClient
- **Version**: 1.0.0
- **Target Framework**: .NET 8.0
- **Repository**: https://github.com/metaneutrons/SnapcastClient
- **License**: GPL-3.0

## Features

- Complete Snapcast JSON-RPC API support
- All 16 commands implemented
- All 11 notifications supported
- Convenience methods for common operations
- Strongly typed parameters and responses
- Comprehensive test coverage (69 tests)
