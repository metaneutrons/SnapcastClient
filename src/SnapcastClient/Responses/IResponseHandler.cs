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

namespace SnapcastClient.Responses;

/// <summary>
/// Interface for handling responses and errors from Snapcast server commands
/// </summary>
public interface IResponseHandler
{
    /// <summary>
    /// Handles a successful response from the server
    /// </summary>
    /// <param name="response">The JSON response string from the server</param>
    void HandleResponse(string response);

    /// <summary>
    /// Handles an error response from the server
    /// </summary>
    /// <param name="error">The error details from the server</param>
    void HandleError(Error error);
}
