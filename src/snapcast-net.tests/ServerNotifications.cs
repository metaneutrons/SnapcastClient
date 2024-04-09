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

namespace SnapCastNet.tests;

internal class ServerNotifications
{
	public static string ClientConnectNotification()
	{
		const string notification = @"{
            ""jsonrpc"": ""2.0"",
            ""method"": ""Client.OnConnect"",
            ""params"": {
                ""client"": {
                    ""config"": {
                        ""instance"": 1,
                        ""latency"": 0,
                        ""name"": ""Kitchen"",
                        ""volume"": {
                            ""muted"": false,
                            ""percent"": 74
                        }
                    },
                    ""connected"": true,
                    ""host"": {
                        ""arch"": ""x86_64"",
                        ""ip"": ""192.168.0.54"",
                        ""mac"": ""00:21:6a:7d:74:fc"",
                        ""name"": ""Kitchen"",
                        ""os"": ""Raspbian GNU/Linux 10 (buster)""
                    },
                    ""id"": ""00:21:6a:7d:74:fc"",
                    ""lastSeen"": {
                        ""sec"": 1488025696,
                        ""usec"": 611255
                    },
                    ""snapclient"": {
                        ""name"": ""Snapclient"",
                        ""protocolVersion"": 2,
                        ""version"": ""0.10.0""
                    }
                },
                ""id"": ""00:21:6a:7d:74:fc""
            }
        }";

		return notification.ReplaceLineEndings("");
	}

	public static string ClientDisconnectNotification()
	{
		const string notification = @"{
            ""jsonrpc"": ""2.0"",
            ""method"": ""Client.OnDisconnect"",
            ""params"": {
                ""client"": {
                    ""config"": {
                        ""instance"": 1,
                        ""latency"": 0,
                        ""name"": ""Kitchen"",
                        ""volume"": {
                            ""muted"": false,
                            ""percent"": 74
                        }
                    },
                    ""connected"": true,
                    ""host"": {
                        ""arch"": ""x86_64"",
                        ""ip"": ""192.168.0.54"",
                        ""mac"": ""00:21:6a:7d:74:fc"",
                        ""name"": ""Kitchen"",
                        ""os"": ""Raspbian GNU/Linux 10 (buster)""
                    },
                    ""id"": ""00:21:6a:7d:74:fc"",
                    ""lastSeen"": {
                        ""sec"": 1488025696,
                        ""usec"": 611255
                    },
                    ""snapclient"": {
                        ""name"": ""Snapclient"",
                        ""protocolVersion"": 2,
                        ""version"": ""0.10.0""
                    }
                },
                ""id"": ""00:21:6a:7d:74:fc""
            }
        }";

		return notification.ReplaceLineEndings("");
	}
}
