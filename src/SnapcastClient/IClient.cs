/***
    This file is part of snapcast-net
    Copyright (C) 2024  Craig Sturdy

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
***/

namespace SnapcastClient;

/// <summary>
/// Interface for Snapcast client operations, providing methods to control clients, groups, streams, and server.
/// </summary>
public interface IClient
{
    /// <summary>
    /// Gets the status of a specific client.
    /// </summary>
    /// <param name="id">The client ID.</param>
    /// <returns>The client status information.</returns>
    public Task<Models.SnapClient> ClientGetStatusAsync(string id);

    /// <summary>
    /// Sets the volume for a specific client.
    /// </summary>
    /// <param name="id">The client ID.</param>
    /// <param name="volume">The volume level (0-100).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ClientSetVolumeAsync(string id, int volume);

    /// <summary>
    /// Sets the latency for a specific client.
    /// </summary>
    /// <param name="id">The client ID.</param>
    /// <param name="latency">The latency in milliseconds.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ClientSetLatencyAsync(string id, int latency);

    /// <summary>
    /// Sets the name for a specific client.
    /// </summary>
    /// <param name="id">The client ID.</param>
    /// <param name="name">The new client name.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ClientSetNameAsync(string id, string name);

    /// <summary>
    /// Gets the status of a specific group.
    /// </summary>
    /// <param name="id">The group ID.</param>
    /// <returns>The group status information.</returns>
    public Task<Models.Group> GroupGetStatusAsync(string id);

    /// <summary>
    /// Sets the mute status for a specific group.
    /// </summary>
    /// <param name="id">The group ID.</param>
    /// <param name="mute">True to mute, false to unmute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task GroupSetMuteAsync(string id, bool mute);

    /// <summary>
    /// Sets the stream for a specific group.
    /// </summary>
    /// <param name="id">The group ID.</param>
    /// <param name="streamId">The stream ID to assign to the group.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task GroupSetStreamAsync(string id, string streamId);

    /// <summary>
    /// Sets the clients for a specific group.
    /// </summary>
    /// <param name="id">The group ID.</param>
    /// <param name="clients">List of client IDs to assign to the group.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task GroupSetClientsAsync(string id, List<string> clients);

    /// <summary>
    /// Sets the name for a specific group.
    /// </summary>
    /// <param name="id">The group ID.</param>
    /// <param name="name">The new group name.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task GroupSetNameAsync(string id, string name);

    /// <summary>
    /// Gets the RPC version of the server.
    /// </summary>
    /// <returns>The server's RPC version information.</returns>
    public Task<Models.RpcVersion> ServerGetRpcVersionAsync();

    /// <summary>
    /// Gets the status of the server.
    /// </summary>
    /// <returns>The server status information.</returns>
    public Task<Models.Server> ServerGetStatusAsync();

    /// <summary>
    /// Deletes a client from the server.
    /// </summary>
    /// <param name="id">The client ID to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ServerDeleteClientAsync(string id);

    /// <summary>
    /// Adds a new stream to the server.
    /// </summary>
    /// <param name="streamUri">The URI of the stream to add.</param>
    /// <returns>The ID of the added stream.</returns>
    public Task<string> StreamAddStreamAsync(string streamUri);

    /// <summary>
    /// Removes a stream from the server.
    /// </summary>
    /// <param name="id">The stream ID to remove.</param>
    /// <returns>The ID of the removed stream.</returns>
    public Task<string> StreamRemoveStreamAsync(string id);

    /// <summary>
    /// Sends a control command to a stream.
    /// </summary>
    /// <param name="id">The stream ID.</param>
    /// <param name="command">The control command to send.</param>
    /// <param name="parameters">Optional parameters for the command.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StreamControlAsync(
        string id,
        string command,
        Dictionary<string, object>? parameters = null
    );

    /// <summary>
    /// Sets a property on a stream.
    /// </summary>
    /// <param name="id">The stream ID.</param>
    /// <param name="property">The property name to set.</param>
    /// <param name="value">The value to set for the property.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StreamSetPropertyAsync(string id, string property, object value);

    // Convenience methods for common Stream.Control commands
    /// <summary>
    /// Plays a stream.
    /// </summary>
    /// <param name="streamId">The stream ID to play.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StreamPlayAsync(string streamId);

    /// <summary>
    /// Pauses a stream.
    /// </summary>
    /// <param name="streamId">The stream ID to pause.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StreamPauseAsync(string streamId);

    /// <summary>
    /// Skips to the next track in a stream.
    /// </summary>
    /// <param name="streamId">The stream ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StreamNextAsync(string streamId);

    /// <summary>
    /// Skips to the previous track in a stream.
    /// </summary>
    /// <param name="streamId">The stream ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StreamPreviousAsync(string streamId);

    /// <summary>
    /// Seeks to a specific position in a stream.
    /// </summary>
    /// <param name="streamId">The stream ID.</param>
    /// <param name="position">The position to seek to in seconds.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StreamSeekAsync(string streamId, double position);

    /// <summary>
    /// Seeks by an offset in a stream.
    /// </summary>
    /// <param name="streamId">The stream ID.</param>
    /// <param name="offset">The offset to seek by in seconds.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StreamSeekByOffsetAsync(string streamId, double offset);

    // Convenience methods for common Stream.SetProperty commands
    /// <summary>
    /// Sets the volume for a stream.
    /// </summary>
    /// <param name="streamId">The stream ID.</param>
    /// <param name="volume">The volume level (0-100).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StreamSetVolumeAsync(string streamId, int volume);

    /// <summary>
    /// Sets the mute status for a stream.
    /// </summary>
    /// <param name="streamId">The stream ID.</param>
    /// <param name="mute">True to mute, false to unmute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StreamSetMuteAsync(string streamId, bool mute);

    /// <summary>
    /// Sets the shuffle mode for a stream.
    /// </summary>
    /// <param name="streamId">The stream ID.</param>
    /// <param name="shuffle">True to enable shuffle, false to disable.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StreamSetShuffleAsync(string streamId, bool shuffle);

    /// <summary>
    /// Sets the loop status for a stream.
    /// </summary>
    /// <param name="streamId">The stream ID.</param>
    /// <param name="loopStatus">The loop status ("none", "track", "playlist").</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StreamSetLoopStatusAsync(string streamId, string loopStatus);

    /// <summary>
    /// Sets the playback rate for a stream.
    /// </summary>
    /// <param name="streamId">The stream ID.</param>
    /// <param name="rate">The playback rate (1.0 = normal speed).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StreamSetRateAsync(string streamId, double rate);

    /// <summary>
    /// Event fired when a client connects to the server.
    /// </summary>
    public Action<Models.SnapClient>? OnClientConnect { set; }

    /// <summary>
    /// Event fired when a client disconnects from the server.
    /// </summary>
    public Action<Models.SnapClient>? OnClientDisconnect { set; }

    /// <summary>
    /// Event fired when a client's volume changes.
    /// </summary>
    public Action<Params.ClientSetVolume>? OnClientVolumeChanged { set; }

    /// <summary>
    /// Event fired when a client's latency changes.
    /// </summary>
    public Action<Params.ClientSetLatency>? OnClientLatencyChanged { set; }

    /// <summary>
    /// Event fired when a client's name changes.
    /// </summary>
    public Action<Params.ClientSetName>? OnClientNameChanged { set; }

    /// <summary>
    /// Event fired when a group's mute status changes.
    /// </summary>
    public Action<Params.GroupOnMute>? OnGroupMute { set; }

    /// <summary>
    /// Event fired when a group's stream changes.
    /// </summary>
    public Action<Params.GroupOnStreamChanged>? OnGroupStreamChanged { set; }

    /// <summary>
    /// Event fired when a group's name changes.
    /// </summary>
    public Action<Params.GroupOnNameChanged>? OnGroupNameChanged { set; }

    /// <summary>
    /// Event fired when stream properties change.
    /// </summary>
    public Action<Params.StreamOnProperties>? OnStreamProperties { set; }

    /// <summary>
    /// Event fired when a stream is updated.
    /// </summary>
    public Func<Models.Stream, Task>? OnStreamUpdate { set; }

    /// <summary>
    /// Event fired when the server is updated.
    /// </summary>
    public Action<Models.Server>? OnServerUpdate { set; }
}
