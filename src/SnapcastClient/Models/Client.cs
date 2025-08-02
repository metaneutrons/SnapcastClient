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

public struct ClientConfig
{
    [JsonProperty("instance")]
    public required int Instance { get; init; }

    [JsonProperty("latency")]
    public required int Latency { get; init; }

    [JsonProperty("name")]
    public required string Name { get; init; }

    [JsonProperty("volume")]
    public required ClientVolume Volume { get; init; }
}

public struct ClientHost
{
    [JsonProperty("arch")]
    public required string Arch { get; init; }

    [JsonProperty("ip")]
    public required string Ip { get; init; }

    [JsonProperty("mac")]
    public required string Mac { get; init; }

    [JsonProperty("name")]
    public required string Name { get; init; }

    [JsonProperty("os")]
    public required string Os { get; init; }
}

public struct ClientVolume
{
    [JsonProperty("muted")]
    public required bool Muted { get; init; }

    [JsonProperty("percent")]
    public required int Percent { get; init; }
}

public struct LastSeen
{
    [JsonProperty("sec")]
    public required long Sec { get; init; }

    [JsonProperty("usec")]
    public required int Usec { get; init; }
}

public struct SnapClientInfo
{
    [JsonProperty("name")]
    public required string Name { get; init; }

    [JsonProperty("protocolVersion")]
    public required int ProtocolVersion { get; init; }

    [JsonProperty("version")]
    public required string Version { get; init; }
}

public struct SnapClient
{
    [JsonProperty("config")]
    public required ClientConfig Config { get; init; }

    [JsonProperty("connected")]
    public required bool Connected { get; init; }

    [JsonProperty("host")]
    public required ClientHost Host { get; init; }

    [JsonProperty("id")]
    public required string Id { get; init; }

    [JsonProperty("lastSeen")]
    public required LastSeen LastSeen { get; init; }

    [JsonProperty("snapclient")]
    public required SnapClientInfo ClientInfo { get; init; }
}
