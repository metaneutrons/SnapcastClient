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
/// Parameters for adding a new stream to the server
/// </summary>
public struct StreamAddStream
{
    /// <summary>
    /// Gets or sets the URI of the stream to add
    /// </summary>
    [JsonProperty("streamUri")]
    public string StreamUri;
}

/// <summary>
/// Parameters for removing a stream from the server
/// </summary>
public struct StreamRemoveStream
{
    /// <summary>
    /// Gets or sets the unique identifier of the stream to remove
    /// </summary>
    [JsonProperty("id")]
    public string Id;
}

/// <summary>
/// Parameters for stream update notification events
/// </summary>
public struct StreamOnUpdate
{
    /// <summary>
    /// Gets or sets the unique identifier of the stream
    /// </summary>
    [JsonProperty("id")]
    public string Id;

    /// <summary>
    /// Gets or sets the updated stream information
    /// </summary>
    [JsonProperty("stream")]
    public Models.Stream Stream;
}

/// <summary>
/// Parameters for controlling stream playback
/// </summary>
public struct StreamControl
{
    /// <summary>
    /// Gets or sets the unique identifier of the stream
    /// </summary>
    [JsonProperty("id")]
    public string Id;

    /// <summary>
    /// Gets or sets the control command to execute (e.g., play, pause, stop)
    /// </summary>
    [JsonProperty("command")]
    public string Command;

    /// <summary>
    /// Gets or sets additional parameters for the control command
    /// </summary>
    [JsonProperty("params")]
    public Dictionary<string, object>? Params;
}

/// <summary>
/// Parameters for setting a property of a stream
/// </summary>
public struct StreamSetProperty
{
    /// <summary>
    /// Gets or sets the unique identifier of the stream
    /// </summary>
    [JsonProperty("id")]
    public string Id;

    /// <summary>
    /// Gets or sets the name of the property to set
    /// </summary>
    [JsonProperty("property")]
    public string Property;

    /// <summary>
    /// Gets or sets the value to assign to the property
    /// </summary>
    [JsonProperty("value")]
    public object Value;
}

/// <summary>
/// Parameters for stream properties notification events
/// </summary>
public struct StreamOnProperties
{
    /// <summary>
    /// Gets or sets the unique identifier of the stream
    /// </summary>
    [JsonProperty("id")]
    public string Id;

    /// <summary>
    /// Gets or sets the updated stream properties
    /// </summary>
    [JsonProperty("properties")]
    public Models.StreamProperties Properties;
}
