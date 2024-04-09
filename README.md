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

