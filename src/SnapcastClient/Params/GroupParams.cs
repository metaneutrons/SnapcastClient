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

namespace SnapcastClient.Params;

public struct GroupGetStatus
{
    [JsonProperty("id")]
    public string Id { get; set; }
}

public struct GroupSetMute
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("mute")]
    public bool Mute { get; set; }
}

public struct GroupSetStream
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("stream_id")]
    public string StreamId { get; set; }
}

public struct GroupSetClients
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("clients")]
    public List<string> Clients { get; set; }
}

public struct GroupSetName
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}

public struct GroupOnMute
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("mute")]
    public bool Mute { get; set; }
}

public struct GroupOnStreamChanged
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("stream_id")]
    public string StreamId { get; set; }
}

public struct GroupOnNameChanged
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}
