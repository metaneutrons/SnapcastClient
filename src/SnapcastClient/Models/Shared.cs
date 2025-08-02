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
/// Information about a host system
/// </summary>
public struct Host
{
    /// <summary>
    /// Gets the system architecture of the host
    /// </summary>
    [JsonProperty("arch")]
    public string Arch;

    /// <summary>
    /// Gets the IP address of the host
    /// </summary>
    [JsonProperty("ip")]
    public string Ip;

    /// <summary>
    /// Gets the MAC address of the host
    /// </summary>
    [JsonProperty("mac")]
    public string Mac;

    /// <summary>
    /// Gets the hostname
    /// </summary>
    [JsonProperty("name")]
    public string Name;

    /// <summary>
    /// Gets the operating system of the host
    /// </summary>
    [JsonProperty("os")]
    public string Os;
}
