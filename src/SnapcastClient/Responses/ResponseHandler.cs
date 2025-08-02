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
/// Generic response handler that processes server responses and invokes appropriate callbacks
/// </summary>
/// <typeparam name="T">The type of the response result</typeparam>
public class ResponseHandler<T> : IResponseHandler
{
    private Action<T>? ResponeCallback;
    private Action<Error>? ErrorCallback;

    /// <summary>
    /// Initializes a new instance of the ResponseHandler class
    /// </summary>
    /// <param name="responseCallback">Callback to invoke when a successful response is received</param>
    /// <param name="errorCallback">Optional callback to invoke when an error response is received</param>
    public ResponseHandler(Action<T> responseCallback, Action<Error>? errorCallback = null)
    {
        ResponeCallback = responseCallback;
        ErrorCallback = errorCallback;
    }

    /// <summary>
    /// Handles a successful response by deserializing it and invoking the response callback
    /// </summary>
    /// <param name="response">The JSON response string from the server</param>
    public void HandleResponse(string response)
    {
        if (ResponeCallback == null)
            throw new Exception("Callback is null");
        var responseObj = JsonConvert.DeserializeObject<RpcResponse<T>>(response);
        ResponeCallback(responseObj.Result);
    }

    /// <summary>
    /// Handles an error response by invoking the error callback if one is set
    /// </summary>
    /// <param name="error">The error details from the server</param>
    public void HandleError(Error error)
    {
        ErrorCallback?.Invoke(error);
    }
}
