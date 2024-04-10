# Snapcast .NET

Another .NET API client for [Snapcast](https://github.com/badaix/snapcast).  The current implementation uses a raw TCP connection to communicate with the Snapcast server.  All serialisation and deserialisation of data is handled within the client.

**Note (10/04/2024): This project is a work in progress, but is actively being developed, when I find the time to push it forward.**


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
- [ ] Stream.AddStream
- [ ] Stream.RemoveStream
- [ ] Stream.Control
- [ ] Stream.SetProperty

## Implemented Notifications

- [x] Client.OnConnect
- [x] Client.OnDisconnect
- [ ] Client.OnVolumeChanged
- [ ] Client.OnLatencyChanged
- [ ] Client.OnNameChanged
- [ ] Group.OnMute
- [ ] Group.OnStreamChanged
- [ ] Group.OnNameChanged
- [ ] Stream.OnProperties
- [ ] Stream.OnUpdate
- [ ] Server.OnUpdate
