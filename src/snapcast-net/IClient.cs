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

namespace SnapCastNet;

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

	public Action<Models.SnapClient>? OnClientConnect { set; }
}
