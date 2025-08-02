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
/// Response containing server status information
/// </summary>
public struct ServerStatus
{
    /// <summary>
    /// Gets or sets the server information including groups and streams
    /// </summary>
    [JsonProperty("server")]
    public Models.Server Server;
}

/// <summary>
/// Response for a delete client operation
/// </summary>
public struct DeleteClient
{
    /// <summary>
    /// Gets or sets the updated server information after client deletion
    /// </summary>
    [JsonProperty("server")]
    public Models.Server Server;
}
