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

namespace SnapCastNet.Commands;

internal class ClientGetStatus(uint id, object commandParams) :
	CommandWithParams(id, "Client.GetStatus", commandParams);

internal class ClientSetVolume(uint id, object commandParams) :
	CommandWithParams(id, "Client.SetVolume", commandParams);

internal class ClientSetLatency(uint id, object commandParams) :
	CommandWithParams(id, "Client.SetLatency", commandParams);

internal class ClientSetName(uint id, object commandParams) :
	CommandWithParams(id, "Client.SetName", commandParams);
