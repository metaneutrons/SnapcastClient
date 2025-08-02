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

namespace SnapcastClient.tests;

internal class ServerErrors
{
	public static string ClientNotFound()
	{
		var response = @"{
			""error"": {
				""code"": -32603,
				""data"": ""Client not found"",
				""message"": ""Internal error""
			},
			""id"": 0,
			""jsonrpc"": ""2.0""
		}";

		return response.ReplaceLineEndings("");
	}

	public static string GroupNotFound()
	{
		var response = @"{
			""error"": {
				""code"": -32603,
				""data"": ""Group not found"",
				""message"": ""Internal error""
			},
			""id"": 0,
			""jsonrpc"": ""2.0""
		}";

		return response.ReplaceLineEndings("");
	}

	public static string StreamNotFound()
	{
		var response = @"{
			""error"": {
				""code"": -32603,
				""data"": ""Stream not found"",
				""message"": ""Internal error""
			},
			""id"": 0,
			""jsonrpc"": ""2.0""
		}";

		return response.ReplaceLineEndings("");
	}
}
