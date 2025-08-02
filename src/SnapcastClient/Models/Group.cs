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

/// <summary>
/// Represents a Snapcast group containing multiple clients
/// </summary>
public struct Group
{
    /// <summary>
    /// Gets the unique identifier of the group
    /// </summary>
    [JsonProperty("id")]
    public string Id;

    /// <summary>
    /// Gets the identifier of the stream assigned to this group
    /// </summary>
    [JsonProperty("stream_id")]
    public string StreamId;

    /// <summary>
    /// Gets the list of clients that belong to this group
    /// </summary>
    [JsonProperty("clients")]
    public List<SnapClient> Clients;

    /// <summary>
    /// Gets the display name of the group
    /// </summary>
    [JsonProperty("name")]
    public string Name;

    /// <summary>
    /// Gets a value indicating whether the group is muted
    /// </summary>
    [JsonProperty("muted")]
    public bool Muted;
}
