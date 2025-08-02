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

namespace SnapcastClient.Responses;

/// <summary>
/// Represents an error response from the Snapcast server
/// </summary>
public struct Error
{
    /// <summary>
    /// Gets or sets the error code
    /// </summary>
    [JsonProperty("code")]
    public int Code { get; set; }

    /// <summary>
    /// Gets or sets additional error data
    /// </summary>
    [JsonProperty("data")]
    public string Data { get; set; }

    /// <summary>
    /// Gets or sets the error message
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; set; }
}
