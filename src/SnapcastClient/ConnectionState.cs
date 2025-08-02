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

namespace SnapcastClient;

/// <summary>
/// Represents the current state of the connection to the Snapcast server
/// </summary>
public enum ConnectionState
{
    /// <summary>
    /// Connection is not established
    /// </summary>
    Disconnected,

    /// <summary>
    /// Attempting to establish connection
    /// </summary>
    Connecting,

    /// <summary>
    /// Connection is established and healthy
    /// </summary>
    Connected,

    /// <summary>
    /// Connection is established but experiencing issues
    /// </summary>
    Degraded,

    /// <summary>
    /// Connection was lost and attempting to reconnect
    /// </summary>
    Reconnecting,

    /// <summary>
    /// Connection failed and will not attempt to reconnect
    /// </summary>
    Failed,
}
