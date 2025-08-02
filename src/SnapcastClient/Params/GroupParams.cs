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

/// <summary>
/// Parameters for getting the status of a specific group
/// </summary>
public struct GroupGetStatus
{
    /// <summary>
    /// Gets or sets the unique identifier of the group
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}

/// <summary>
/// Parameters for setting the mute state of a specific group
/// </summary>
public struct GroupSetMute
{
    /// <summary>
    /// Gets or sets the unique identifier of the group
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to mute the group
    /// </summary>
    [JsonProperty("mute")]
    public bool Mute { get; set; }
}

/// <summary>
/// Parameters for setting the stream of a specific group
/// </summary>
public struct GroupSetStream
{
    /// <summary>
    /// Gets or sets the unique identifier of the group
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the stream identifier to assign to the group
    /// </summary>
    [JsonProperty("stream_id")]
    public string StreamId { get; set; }
}

/// <summary>
/// Parameters for setting the clients of a specific group
/// </summary>
public struct GroupSetClients
{
    /// <summary>
    /// Gets or sets the unique identifier of the group
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the list of client identifiers to assign to the group
    /// </summary>
    [JsonProperty("clients")]
    public List<string> Clients { get; set; }
}

/// <summary>
/// Parameters for setting the name of a specific group
/// </summary>
public struct GroupSetName
{
    /// <summary>
    /// Gets or sets the unique identifier of the group
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the new name for the group
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }
}

/// <summary>
/// Parameters for group mute notification events
/// </summary>
public struct GroupOnMute
{
    /// <summary>
    /// Gets or sets the unique identifier of the group
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the group is muted
    /// </summary>
    [JsonProperty("mute")]
    public bool Mute { get; set; }
}

/// <summary>
/// Parameters for group stream change notification events
/// </summary>
public struct GroupOnStreamChanged
{
    /// <summary>
    /// Gets or sets the unique identifier of the group
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the stream identifier assigned to the group
    /// </summary>
    [JsonProperty("stream_id")]
    public string StreamId { get; set; }
}

/// <summary>
/// Parameters for group name change notification events
/// </summary>
public struct GroupOnNameChanged
{
    /// <summary>
    /// Gets or sets the unique identifier of the group
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the new name of the group
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }
}
