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
    public int Instance;

    [JsonProperty("latency")]
    public int Latency;

    [JsonProperty("name")]
    public string Name;

    [JsonProperty("volume")]
    public ClientVolume Volume;
}

public struct ClientHost
{
    [JsonProperty("arch")]
    public string Arch;

    [JsonProperty("ip")]
    public string Ip;

    [JsonProperty("mac")]
    public string Mac;

    [JsonProperty("name")]
    public string Name;

    [JsonProperty("os")]
    public string Os;
}

public struct ClientVolume
{
    [JsonProperty("muted")]
    public bool Muted;

    [JsonProperty("percent")]
    public int Percent;
}

public struct LastSeen
{
    [JsonProperty("sec")]
    public Int64 Sec;

    [JsonProperty("usec")]
    public int Usec;
}

public struct SnapClientInfo
{
    [JsonProperty("name")]
    public string Name;

    [JsonProperty("protocolVersion")]
    public int ProtocolVersion;

    [JsonProperty("version")]
    public string Version;
}

public struct SnapClient
{
    [JsonProperty("config")]
    public ClientConfig Config;

    [JsonProperty("connected")]
    public bool Connected;

    [JsonProperty("host")]
    public ClientHost Host;

    [JsonProperty("id")]
    public string Id;

    [JsonProperty("lastSeen")]
    public LastSeen LastSeen;

    [JsonProperty("snapclient")]
    public SnapClientInfo ClientInfo;
}
