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
/// Interface for network connections to the Snapcast server.
/// </summary>
public interface IConnection
{
    /// <summary>
    /// Sends data to the server.
    /// </summary>
    /// <param name="data">The data to send.</param>
    public void Send(string data);

    /// <summary>
    /// Reads data from the server.
    /// </summary>
    /// <returns>The data received from the server, or null if no data is available.</returns>
    public string? Read();
}
