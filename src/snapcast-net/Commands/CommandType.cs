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

public enum CommandType
{
	CLIENT_GET_STATUS,
	CLIENT_SET_VOLUME,
	CLIENT_SET_LATENCY,
	CLIENT_SET_NAME,

	GROUP_GET_STATUS,
	GROUP_SET_MUTE,
	GROUP_SET_STREAM,
	GROUP_SET_CLIENTS,
	GROUP_SET_NAME,

	SERVER_GET_RPC_VERSION,
	SERVER_GET_STATUS,
	SERVER_DELETE_CLIENT,

	STREAM_ADD_STREAM,
	STREAM_REMOVE_STREAM,
	STREAM_CONTROL,
	STREAM_SET_PROPERTY
}
