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

namespace SnapcastClient.Commands;

/// <summary>
/// Defines the types of commands supported by the Snapcast API
/// </summary>
public enum CommandType
{
    /// <summary>
    /// Get the status of a specific client
    /// </summary>
    CLIENT_GET_STATUS,
    
    /// <summary>
    /// Set the volume of a specific client
    /// </summary>
    CLIENT_SET_VOLUME,
    
    /// <summary>
    /// Set the latency of a specific client
    /// </summary>
    CLIENT_SET_LATENCY,
    
    /// <summary>
    /// Set the name of a specific client
    /// </summary>
    CLIENT_SET_NAME,

    /// <summary>
    /// Get the status of a specific group
    /// </summary>
    GROUP_GET_STATUS,
    
    /// <summary>
    /// Set the mute state of a specific group
    /// </summary>
    GROUP_SET_MUTE,
    
    /// <summary>
    /// Set the stream for a specific group
    /// </summary>
    GROUP_SET_STREAM,
    
    /// <summary>
    /// Set the clients for a specific group
    /// </summary>
    GROUP_SET_CLIENTS,
    
    /// <summary>
    /// Set the name of a specific group
    /// </summary>
    GROUP_SET_NAME,

    /// <summary>
    /// Get the RPC version of the server
    /// </summary>
    SERVER_GET_RPC_VERSION,
    
    /// <summary>
    /// Get the status of the server
    /// </summary>
    SERVER_GET_STATUS,
    
    /// <summary>
    /// Delete a client from the server
    /// </summary>
    SERVER_DELETE_CLIENT,

    /// <summary>
    /// Add a new stream to the server
    /// </summary>
    STREAM_ADD_STREAM,
    
    /// <summary>
    /// Remove a stream from the server
    /// </summary>
    STREAM_REMOVE_STREAM,
    
    /// <summary>
    /// Control a stream (play, pause, etc.)
    /// </summary>
    STREAM_CONTROL,
    
    /// <summary>
    /// Set a property of a stream
    /// </summary>
    STREAM_SET_PROPERTY,
}
