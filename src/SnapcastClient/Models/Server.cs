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

using Newtonsoft.Json;

namespace SnapcastClient.Models;

public struct SnapServerInfo
{
    [JsonProperty("protocolVersion")]
    public int ProtocolVersion;

    [JsonProperty("controlProtocolVersion")]
    public int ControlProtocolVersion;

    [JsonProperty("name")]
    public string Name;

    [JsonProperty("version")]
    public string Version;
}

public struct ServerInfo
{
    [JsonProperty("host")]
    public Host Host;

    [JsonProperty("snapserver")]
    public SnapServerInfo SnapServer;
}

public struct Server
{
    [JsonProperty("server")]
    public ServerInfo ServerInfo;

    [JsonProperty("groups")]
    public List<Group> Groups;

    [JsonProperty("streams")]
    public List<Stream> Streams;
}

public struct RpcVersion
{
    [JsonProperty("major")]
    public int Major;

    [JsonProperty("minor")]
    public int Minor;

    [JsonProperty("patch")]
    public int Patch;
}
