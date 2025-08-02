# Snapcast .NET

Another .NET API client for [Snapcast](https://github.com/badaix/snapcast).  The current implementation uses a raw TCP connection to communicate with the Snapcast server.  All serialisation and deserialisation of data is handled within the client.

**Note (2025/08/02): This is a fork of https://gitlab.com/sturd/snapcast-net implementing the missing functionality. I eventually create a PR for this, but this needs testing first. **

## Installation

### From GitHub Packages

```bash
# Add GitHub Packages source
dotnet nuget add source https://nuget.pkg.github.com/metaneutrons/index.json --name github-snapcast-net

# Install the package
dotnet add package SnapCastNet --source github-snapcast-net
```

See [PACKAGE.md](PACKAGE.md) for detailed installation instructions and authentication setup.

## Usage

### Initialise

``` c#
using SnapCastNet;

var connection = new TcpConnection("127.0.0.1", 1705);
var client = new Client(connection);

var result = await client.ServerGetRpcVersionAsync();

// result = {
//   Major: 2,
//   Minor: 0,
//   Patch: 0
// }
```

## Implemented Commands

- [x] Client.GetStatus
- [x] Client.SetVolume
- [x] Client.SetLatency
- [x] Client.SetName
- [x] Group.GetStatus
- [x] Group.SetMute
- [x] Group.SetStream
- [x] Group.SetClients
- [x] Group.SetName
- [x] Server.GetRPCVersion
- [x] Server.GetStatus
- [x] Server.DeleteClient
- [x] Stream.AddStream
- [x] Stream.RemoveStream
- [x] Stream.Control
- [x] Stream.SetProperty

## Implemented Notifications

- [x] Client.OnConnect
- [x] Client.OnDisconnect
- [x] Client.OnVolumeChanged
- [x] Client.OnLatencyChanged
- [x] Client.OnNameChanged
- [x] Group.OnMute
- [x] Group.OnStreamChanged
- [x] Group.OnNameChanged
- [x] Stream.OnProperties
- [x] Stream.OnUpdate
- [x] Server.OnUpdate

## Stream Control Convenience Methods

The library provides convenient methods for common stream control operations:

### Playback Control

- `StreamPlayAsync(streamId)` - Play a stream
- `StreamPauseAsync(streamId)` - Pause a stream
- `StreamNextAsync(streamId)` - Skip to next track
- `StreamPreviousAsync(streamId)` - Skip to previous track
- `StreamSeekAsync(streamId, position)` - Seek to specific position
- `StreamSeekByOffsetAsync(streamId, offset)` - Seek by offset

### Stream Properties

- `StreamSetVolumeAsync(streamId, volume)` - Set stream volume (0-100)
- `StreamSetMuteAsync(streamId, mute)` - Mute/unmute stream
- `StreamSetShuffleAsync(streamId, shuffle)` - Enable/disable shuffle
- `StreamSetLoopStatusAsync(streamId, loopStatus)` - Set loop mode ("none", "track", "playlist")
- `StreamSetRateAsync(streamId, rate)` - Set playback rate (1.0 = normal speed)

## Event Handling

Subscribe to notifications to receive real-time updates:

```c#
// Client events
client.OnClientConnect = (client) => Console.WriteLine($"Client connected: {client.Id}");
client.OnClientDisconnect = (client) => Console.WriteLine($"Client disconnected: {client.Id}");
client.OnClientVolumeChanged = (volumeChange) => Console.WriteLine($"Volume changed: {volumeChange.Volume.Percent}%");

// Group events
client.OnGroupMute = (muteChange) => Console.WriteLine($"Group {muteChange.Id} muted: {muteChange.Mute}");
client.OnGroupStreamChanged = (streamChange) => Console.WriteLine($"Group {streamChange.Id} stream: {streamChange.StreamId}");

// Stream events
client.OnStreamUpdate = async (stream) => Console.WriteLine($"Stream updated: {stream.Id}");
client.OnStreamProperties = (properties) => Console.WriteLine($"Stream properties: {properties.Id}");

// Server events
client.OnServerUpdate = (server) => Console.WriteLine("Server updated");
```

## Advanced Stream Control

### Direct Stream Control

For advanced control, you can use the low-level `StreamControlAsync` and `StreamSetPropertyAsync` methods:

```c#
// Direct stream control with custom parameters
await client.StreamControlAsync("Spotify", "seek", new Dictionary<string, object> { { "offset", 30 } });

// Set custom stream properties
await client.StreamSetPropertyAsync("Spotify", "volume", 75);
await client.StreamSetPropertyAsync("Spotify", "loopStatus", "playlist");
```

### Stream Control Examples

```c#
// Basic playback control
await client.StreamPlayAsync("Spotify");
await client.StreamPauseAsync("Spotify");
await client.StreamNextAsync("Spotify");
await client.StreamPreviousAsync("Spotify");

// Seeking
await client.StreamSeekAsync("Spotify", 120.5); // Seek to 2 minutes 30 seconds
await client.StreamSeekByOffsetAsync("Spotify", 30); // Skip forward 30 seconds

// Stream properties
await client.StreamSetVolumeAsync("Spotify", 80); // Set volume to 80%
await client.StreamSetMuteAsync("Spotify", true); // Mute the stream
await client.StreamSetShuffleAsync("Spotify", true); // Enable shuffle
await client.StreamSetLoopStatusAsync("Spotify", "track"); // Loop current track
await client.StreamSetRateAsync("Spotify", 1.5); // Play at 1.5x speed
```
