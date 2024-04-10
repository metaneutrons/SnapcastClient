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

internal class ServerResponses
{
	public static string ClientGetStatusResponse()
	{
		const string response = @"{
			""id"": 0,
			""jsonrpc"": ""2.0"",
			""result"": {
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
				}
			}
		}";

		return response.ReplaceLineEndings("");
	}

	public static string ClientSetVolumeResponse()
	{
		const string response = @"{
			""id"": 0,
			""jsonrpc"": ""2.0"",
			""result"": {
				""volume"": {
					""muted"": false,
					""percent"": 36
				}
			}
		}";

		return response.ReplaceLineEndings("");
	}

	public static string ClientSetLatencyResponse()
	{
		const string response = @"{
			""id"": 0,
			""jsonrpc"": ""2.0"",
			""result"": {
				""latency"": 10
			}
		}";

		return response.ReplaceLineEndings("");
	}

	public static string ClientSetNameResponse()
	{
		const string response = @"{
			""id"": 0,
			""jsonrpc"": ""2.0"",
			""result"": {
				""name"": ""Laptop""
			}
		}";
		return response.ReplaceLineEndings("");
	}

	public static string GroupGetStatusResponse()
	{
		const string response = @"{
			""id"": 0,
			""jsonrpc"": ""2.0"",
			""result"": {
				""group"": {
					""clients"": [
						{
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
						}
					],
					""id"": ""4dcc4e3b-c699-a04b-7f0c-8260d23c43e1"",
					""muted"": false,
					""name"": """",
					""stream_id"": ""stream 1""
				}
			}
		}";

		return response.ReplaceLineEndings("");
	}

	public static string GroupSetMuteResponse()
	{
		const string response = @"{
			""id"": 0,
			""jsonrpc"": ""2.0"",
			""result"": {
				""mute"": true
			}
		}";
		return response.ReplaceLineEndings("");
	}

	public static string GroupSetStreamResponse()
	{
		const string response = @"{
			""id"": 0,
			""jsonrpc"": ""2.0"",
			""result"": {
				""stream_id"": ""stream 1""
			}
		}";
		return response.ReplaceLineEndings("");
	}

	public static string GroupSetClientsResponse()
	{
		return ServerGetStatusResponse();
	}

	public static string ServerGetRpcVersionResponse()
	{
		const string response = @"{
			""id"": 0,
			""jsonrpc"": ""2.0"",
			""result"": {
				""major"": 2,
				""minor"": 0,
				""patch"": 0
			}
		}";
		return response.ReplaceLineEndings("");
	}

	internal static string ServerGetStatusResponse()
	{
		const string response = @"{
			""id"": 0,
			""jsonrpc"": ""2.0"",
			""result"": {
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

		return response.ReplaceLineEndings("");
	}

	public static string GroupSetNameResponse()
	{
		const string response = @"{
			""id"": 0,
			""jsonrpc"": ""2.0"",
			""result"": {
				""name"": ""GroundFloor""
			}
		}";
		return response.ReplaceLineEndings("");
	}

	public static string StreamAddStreamResponse()
	{
		const string response = @"{
			""id"": 0,
			""jsonrpc"": ""2.0"",
			""result"": {
				""stream_id"": ""stream 2""
			}
		}";
		return response.ReplaceLineEndings("");
	}
}
