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
/// Factory class for creating Snapcast command objects
/// </summary>
public class CommandFactory
{
    private uint Id = 0;
    private Mutex IdMutex = new Mutex();

    /// <summary>
    /// Initializes a new instance of the CommandFactory class
    /// </summary>
    public CommandFactory() { }

    /// <summary>
    /// Generates a new unique command ID
    /// </summary>
    /// <returns>A unique command identifier</returns>
    public uint NewId()
    {
        IdMutex.WaitOne();
        var id = Id++;
        IdMutex.ReleaseMutex();
        return id;
    }

    /// <summary>
    /// Creates a command instance based on the specified command type and parameters
    /// </summary>
    /// <typeparam name="T">The type of command parameters</typeparam>
    /// <param name="commandType">The type of command to create</param>
    /// <param name="commandParams">The parameters for the command</param>
    /// <returns>A command instance, or null if the command type is not supported</returns>
    public ICommand? createCommand<T>(CommandType commandType, T commandParams)
    {
        switch (commandType)
        {
            case CommandType.CLIENT_GET_STATUS:
                if (commandParams == null)
                    throw new System.ArgumentNullException(nameof(commandParams));
                return new ClientGetStatus(NewId(), commandParams);

            case CommandType.CLIENT_SET_VOLUME:
                if (commandParams == null)
                    throw new System.ArgumentNullException(nameof(commandParams));
                return new ClientSetVolume(NewId(), commandParams);

            case CommandType.CLIENT_SET_LATENCY:
                if (commandParams == null)
                    throw new System.ArgumentNullException(nameof(commandParams));
                return new ClientSetLatency(NewId(), commandParams);

            case CommandType.CLIENT_SET_NAME:
                if (commandParams == null)
                    throw new System.ArgumentNullException(nameof(commandParams));
                return new ClientSetName(NewId(), commandParams);

            case CommandType.GROUP_GET_STATUS:
                if (commandParams == null)
                    throw new System.ArgumentNullException(nameof(commandParams));
                return new GroupGetStatus(NewId(), commandParams);

            case CommandType.GROUP_SET_MUTE:
                if (commandParams == null)
                    throw new System.ArgumentNullException(nameof(commandParams));
                return new GroupSetMute(NewId(), commandParams);

            case CommandType.GROUP_SET_STREAM:
                if (commandParams == null)
                    throw new System.ArgumentNullException(nameof(commandParams));
                return new GroupSetStream(NewId(), commandParams);

            case CommandType.GROUP_SET_CLIENTS:
                if (commandParams == null)
                    throw new System.ArgumentNullException(nameof(commandParams));
                return new GroupSetClients(NewId(), commandParams);

            case CommandType.GROUP_SET_NAME:
                if (commandParams == null)
                    throw new System.ArgumentNullException(nameof(commandParams));
                return new GroupSetName(NewId(), commandParams);

            case CommandType.SERVER_GET_RPC_VERSION:
                return new ServerGetRpcVersion(NewId());

            case CommandType.SERVER_GET_STATUS:
                return new ServerGetStatus(NewId());

            case CommandType.SERVER_DELETE_CLIENT:
                if (commandParams == null)
                    throw new System.ArgumentNullException(nameof(commandParams));
                return new ServerDeleteClient(NewId(), commandParams);

            case CommandType.STREAM_ADD_STREAM:
                if (commandParams == null)
                    throw new System.ArgumentNullException(nameof(commandParams));
                return new StreamAddStream(NewId(), commandParams);

            case CommandType.STREAM_REMOVE_STREAM:
                if (commandParams == null)
                    throw new System.ArgumentNullException(nameof(commandParams));
                return new StreamRemoveStream(NewId(), commandParams);

            case CommandType.STREAM_CONTROL:
                if (commandParams == null)
                    throw new System.ArgumentNullException(nameof(commandParams));
                return new StreamControl(NewId(), commandParams);

            case CommandType.STREAM_SET_PROPERTY:
                if (commandParams == null)
                    throw new System.ArgumentNullException(nameof(commandParams));
                return new StreamSetProperty(NewId(), commandParams);
        }

        return null;
    }
}
