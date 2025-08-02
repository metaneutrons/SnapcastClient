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

public interface IClient
{
    public Task<Models.SnapClient> ClientGetStatusAsync(string id);

    public Task ClientSetVolumeAsync(string id, int volume);

    public Task ClientSetLatencyAsync(string id, int latency);

    public Task ClientSetNameAsync(string id, string name);

    public Task<Models.Group> GroupGetStatusAsync(string id);

    public Task GroupSetMuteAsync(string id, bool mute);

    public Task GroupSetStreamAsync(string id, string streamId);

    public Task GroupSetClientsAsync(string id, List<string> clients);

    public Task GroupSetNameAsync(string id, string name);

    public Task<Models.RpcVersion> ServerGetRpcVersionAsync();

    public Task<Models.Server> ServerGetStatusAsync();

    public Task ServerDeleteClientAsync(string id);

    public Task<string> StreamAddStreamAsync(string streamUri);

    public Task<string> StreamRemoveStreamAsync(string id);

    public Task StreamControlAsync(string id, string command, Dictionary<string, object>? parameters = null);

    public Task StreamSetPropertyAsync(string id, string property, object value);

    // Convenience methods for common Stream.Control commands
    public Task StreamPlayAsync(string streamId);
    public Task StreamPauseAsync(string streamId);
    public Task StreamNextAsync(string streamId);
    public Task StreamPreviousAsync(string streamId);
    public Task StreamSeekAsync(string streamId, double position);
    public Task StreamSeekByOffsetAsync(string streamId, double offset);

    // Convenience methods for common Stream.SetProperty commands
    public Task StreamSetVolumeAsync(string streamId, int volume);
    public Task StreamSetMuteAsync(string streamId, bool mute);
    public Task StreamSetShuffleAsync(string streamId, bool shuffle);
    public Task StreamSetLoopStatusAsync(string streamId, string loopStatus);
    public Task StreamSetRateAsync(string streamId, double rate);

    public Action<Models.SnapClient>? OnClientConnect { set; }

    public Action<Models.SnapClient>? OnClientDisconnect { set; }

    public Action<Params.ClientSetVolume>? OnClientVolumeChanged { set; }

    public Action<Params.ClientSetLatency>? OnClientLatencyChanged { set; }

    public Action<Params.ClientSetName>? OnClientNameChanged { set; }

    public Action<Params.GroupOnMute>? OnGroupMute { set; }

    public Action<Params.GroupOnStreamChanged>? OnGroupStreamChanged { set; }

    public Action<Params.GroupOnNameChanged>? OnGroupNameChanged { set; }

    public Action<Params.StreamOnProperties>? OnStreamProperties { set; }

    public Func<Models.Stream, Task>? OnStreamUpdate { set; }

    public Action<Models.Server>? OnServerUpdate { set; }
}
