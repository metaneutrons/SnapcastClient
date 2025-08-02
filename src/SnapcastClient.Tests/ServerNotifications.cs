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

	public static string ClientVolumeChangedNotification()
	{
		const string notification = @"{
			""jsonrpc"": ""2.0"",
			""method"": ""Client.OnVolumeChanged"",
			""params"": {
				""id"": ""00:21:6a:7d:74:fc"",
				""volume"": {
					""muted"": false,
					""percent"": 36
				}
			}
		}";

		return notification.ReplaceLineEndings("");
	}

	public static string ClientLatencyChangedNotification()
	{
		const string notification = @"{
			""jsonrpc"": ""2.0"",
			""method"": ""Client.OnLatencyChanged"",
			""params"": {
				""id"": ""00:21:6a:7d:74:fc"",
				""latency"": 50
			}
		}";

		return notification.ReplaceLineEndings("");
	}

	public static string ClientNameChangedNotification()
	{
		const string notification = @"{
			""jsonrpc"": ""2.0"",
			""method"": ""Client.OnNameChanged"",
			""params"": {
				""id"": ""00:21:6a:7d:74:fc"",
				""name"": ""Laptop""
			}
		}";

		return notification.ReplaceLineEndings("");
	}

	public static string StreamUpdateNotification()
	{
		const string notification = @"{
			""jsonrpc"": ""2.0"",
			""method"":""Stream.OnUpdate"",
			""params"": {
				""id"":""stream 1"",
				""stream"":{
					""id"":""stream 1"",
					""status"":""idle"",
					""uri"": {
						""fragment"":""test"",
						""host"":""localhost"",
						""path"":""/tmp/snapfifo"",
						""query"": {
							""chunk_ms"":""20"",
							""codec"":""flac"",
							""name"":""stream 1"",
							""sampleformat"":""48000:16:2""
						},
						""raw"": ""pipe:///tmp/snapfifo?name=stream 1"",""scheme"":""pipe""
					}
				}
			}
		}";
		return notification.ReplaceLineEndings("");
	}

	public static string GroupOnMuteNotification()
	{
		const string notification = @"{
			""jsonrpc"": ""2.0"",
			""method"": ""Group.OnMute"",
			""params"": {
				""id"": ""4dcc4e3b-c699-a04b-7f0c-8260d23c43e1"",
				""mute"": true
			}
		}";

		return notification.ReplaceLineEndings("");
	}

	public static string GroupOnStreamChangedNotification()
	{
		const string notification = @"{
			""jsonrpc"": ""2.0"",
			""method"": ""Group.OnStreamChanged"",
			""params"": {
				""id"": ""4dcc4e3b-c699-a04b-7f0c-8260d23c43e1"",
				""stream_id"": ""stream 2""
			}
		}";

		return notification.ReplaceLineEndings("");
	}

	public static string GroupOnNameChangedNotification()
	{
		const string notification = @"{
			""jsonrpc"": ""2.0"",
			""method"": ""Group.OnNameChanged"",
			""params"": {
				""id"": ""4dcc4e3b-c699-a04b-7f0c-8260d23c43e1"",
				""name"": ""GroundFloor""
			}
		}";

		return notification.ReplaceLineEndings("");
	}

	public static string StreamOnPropertiesNotification()
	{
		const string notification = @"{
			""jsonrpc"": ""2.0"",
			""method"": ""Stream.OnProperties"",
			""params"": {
				""id"": ""stream 1"",
				""properties"": {
					""canControl"": true,
					""canGoNext"": true,
					""canGoPrevious"": true,
					""canPause"": true,
					""canPlay"": true,
					""canSeek"": true,
					""metadata"": {
						""album"": ""Test Album"",
						""artist"": [""Test Artist""],
						""title"": ""Test Track"",
						""artUrl"": ""http://example.com/art.jpg"",
						""artData"": {
							""data"": ""base64data"",
							""extension"": ""jpg""
						}
					}
				}
			}
		}";

		return notification.ReplaceLineEndings("");
	}

	public static string ServerOnUpdateNotification()
	{
		const string notification = @"{
			""jsonrpc"": ""2.0"",
			""method"": ""Server.OnUpdate"",
			""params"": {
				""server"": {
					""groups"": [{
						""clients"": [{
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
						}],
						""id"": ""4dcc4e3b-c699-a04b-7f0c-8260d23c43e1"",
						""muted"": false,
						""name"": ""Kitchen"",
						""stream_id"": ""stream 1""
					}],
					""server"": {
						""host"": {
							""arch"": ""x86_64"",
							""ip"": ""127.0.0.1"",
							""mac"": """",
							""name"": ""SnapServer"",
							""os"": ""Raspbian GNU/Linux 10 (buster)""
						},
						""snapserver"": {
							""controlProtocolVersion"": 1,
							""name"": ""Snapserver"",
							""protocolVersion"": 1,
							""version"": ""0.10.0""
						}
					},
					""streams"": [{
						""id"": ""stream 1"",
						""status"": ""idle"",
						""uri"": {
							""fragment"": """",
							""host"": """",
							""path"": ""/tmp/snapfifo"",
							""query"": {
								""chunk_ms"": ""20"",
								""codec"": ""flac"",
								""name"": ""stream 1"",
								""sampleformat"": ""48000:16:2""
							},
							""raw"": ""pipe:///tmp/snapfifo?name=stream 1"",
							""scheme"": ""pipe""
						}
					}]
				}
			}
		}";

		return notification.ReplaceLineEndings("");
	}
}
