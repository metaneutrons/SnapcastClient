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

using System.Net.Sockets;
using System.Text;

namespace SnapcastClient;

/// <summary>
/// TCP connection implementation for communicating with the Snapcast server.
/// </summary>
/// <summary>
/// TCP connection implementation for communicating with the Snapcast server.
/// </summary>
public class TcpConnection : IConnection, IDisposable
{
    private readonly TcpClient Client;
    private readonly NetworkStream Stream;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the TcpConnection class.
    /// </summary>
    /// <param name="host">The hostname or IP address of the server.</param>
    /// <param name="port">The port number to connect to.</param>
    public TcpConnection(string host, int port)
    {
        Client = new TcpClient(host, port);
        Stream = Client.GetStream();
    }

    /// <summary>
    /// Sends data to the server over the TCP connection.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the connection has been disposed.</exception>
    public void Send(string data)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TcpConnection));

        byte[] bytes = Encoding.UTF8.GetBytes(data + '\n');
        Stream.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// Reads data from the server over the TCP connection.
    /// </summary>
    /// <returns>The data received from the server, or null if no data is available or connection is disposed.</returns>
    /// <summary>
    /// Reads data from the server over the TCP connection.
    /// </summary>
    /// <returns>The data received from the server, or null if no data is available or connection is disposed.</returns>
    public string? Read()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TcpConnection));

        if (!Stream.DataAvailable)
            return null;

        string responseData = "";
        int chunkSize = 1024;
        byte[] responseBytes = new byte[chunkSize];

        int bytesRead;
        while ((bytesRead = Stream.Read(responseBytes, 0, responseBytes.Length)) > 0)
        {
            responseData += Encoding.UTF8.GetString(responseBytes, 0, bytesRead);

            if (responseData.Contains('\n'))
                break;
        }
        if (responseData.EndsWith('\n'))
            responseData = responseData.Substring(0, responseData.Length - 1);

        return responseData;
    }

    /// <summary>
    /// Releases all resources used by the TcpConnection.
    /// </summary>
    /// <summary>
    /// Releases all resources used by the TcpConnection.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Stream?.Dispose();
        Client?.Dispose();
    }
}
