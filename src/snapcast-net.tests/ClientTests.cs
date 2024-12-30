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

using Moq;
using SnapCastNet.Models;

namespace SnapCastNet.tests;

public class ClientTests
{

	private void ValidateClient(SnapClient client)
	{
		Assert.That(client.Id, Is.EqualTo("00:21:6a:7d:74:fc"));
		Assert.That(client.Connected, Is.True);

		Assert.That(client.Config.Instance, Is.EqualTo(1));
		Assert.That(client.Config.Latency, Is.EqualTo(0));
		Assert.That(client.Config.Name, Is.EqualTo("Kitchen"));
		Assert.That(client.Config.Volume.Muted, Is.False);
		Assert.That(client.Config.Volume.Percent, Is.EqualTo(74));

		Assert.That(client.Host.Arch, Is.EqualTo("x86_64"));
		Assert.That(client.Host.Ip, Is.EqualTo("192.168.0.54"));
		Assert.That(client.Host.Mac, Is.EqualTo("00:21:6a:7d:74:fc"));
		Assert.That(client.Host.Name, Is.EqualTo("Kitchen"));
		Assert.That(client.Host.Os, Is.EqualTo("Raspbian GNU/Linux 10 (buster)"));

		Assert.That(client.LastSeen.Sec, Is.EqualTo(1488025696));
		Assert.That(client.LastSeen.Usec, Is.EqualTo(611255));

		Assert.That(client.ClientInfo.Name, Is.EqualTo("Snapclient"));
		Assert.That(client.ClientInfo.ProtocolVersion, Is.EqualTo(2));
		Assert.That(client.ClientInfo.Version, Is.EqualTo("0.10.0"));
	}

	private void ValidateServerInfo(ServerInfo serverInfo)
	{
		Assert.That(serverInfo.SnapServer.Version, Is.EqualTo("0.10.0"));
		Assert.That(serverInfo.SnapServer.ProtocolVersion, Is.EqualTo(1));
		Assert.That(serverInfo.SnapServer.ControlProtocolVersion, Is.EqualTo(1));
		Assert.That(serverInfo.SnapServer.Name, Is.EqualTo("Snapserver"));

		Assert.That(serverInfo.Host.Arch, Is.EqualTo("x86_64"));
		Assert.That(serverInfo.Host.Ip, Is.EqualTo("127.0.0.1"));
		Assert.That(serverInfo.Host.Mac, Is.EqualTo(""));
		Assert.That(serverInfo.Host.Name, Is.EqualTo("SnapServer"));
		Assert.That(serverInfo.Host.Os, Is.EqualTo("Raspbian GNU/Linux 10 (buster)"));
	}

	[Test]
	public void Test_ClientGetStatusAsync()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"bla:bla:bla\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Client.GetStatus\"}";

		connectionMock.Setup(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
				.Returns(ServerResponses.ClientGetStatusResponse())
				.Returns((string?)null);
		});

		var responseTask = snapClient.ClientGetStatusAsync("bla:bla:bla");
		var response = responseTask.Result;

		connectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);

		Assert.IsNotNull(response);
		ValidateClient(response);
	}

	[Test]
	public void Test_ClientSetVolumeAsync()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"bla:bla:bla\",\"volume\":{\"muted\":false,\"percent\":36}},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Client.SetVolume\"}";

		connectionMock.Setup(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
				.Returns(ServerResponses.ClientSetVolumeResponse())
				.Returns((string?)null);
		});

		snapClient.ClientSetVolumeAsync("bla:bla:bla", 36).Wait();
		connectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_ClientSetLatencyAsync()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"bla:bla:bla\",\"latency\":10},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Client.SetLatency\"}";

		connectionMock.Setup(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
				.Returns(ServerResponses.ClientSetLatencyResponse())
				.Returns((string?)null);
		});

		snapClient.ClientSetLatencyAsync("bla:bla:bla", 10).Wait();
		connectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_ClientSetNameAsync()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"bla:bla:bla\",\"name\":\"Laptop\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Client.SetName\"}";

		connectionMock.Setup(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
				.Returns(ServerResponses.ClientSetNameResponse())
				.Returns((string?)null);
		});

		snapClient.ClientSetNameAsync("bla:bla:bla", "Laptop").Wait();
		connectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_GroupGetStatusAsync()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"4dcc4e3b-c699-a04b-7f0c-8260d23c43e1\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Group.GetStatus\"}";

		connectionMock.Setup(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.GroupGetStatusResponse())
			.Returns((string?)null);
		});

		var responseTask = snapClient.GroupGetStatusAsync("4dcc4e3b-c699-a04b-7f0c-8260d23c43e1");
		var response = responseTask.Result;
		connectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);

		Assert.IsNotNull(response);

		Assert.That(response.Clients.Count, Is.EqualTo(1));
		ValidateClient(response.Clients[0]);

		Assert.That(response.Id, Is.EqualTo("4dcc4e3b-c699-a04b-7f0c-8260d23c43e1"));
		Assert.That(response.Muted, Is.False);
		Assert.That(response.Name, Is.EqualTo(""));
		Assert.That(response.StreamId, Is.EqualTo("stream 1"));
	}

	[Test]
	public void Test_GroupSetMuteAsync()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"4dcc4e3b-c699-a04b-7f0c-8260d23c43e1\",\"mute\":true},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Group.SetMute\"}";

		connectionMock.Setup(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.GroupSetMuteResponse())
			.Returns((string?)null);
		});
		snapClient.GroupSetMuteAsync("4dcc4e3b-c699-a04b-7f0c-8260d23c43e1", true).Wait();
		connectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_GroupSetStreamAsync()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"4dcc4e3b-c699-a04b-7f0c-8260d23c43e1\",\"stream_id\":\"stream 1\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Group.SetStream\"}";

		connectionMock.Setup(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.GroupSetStreamResponse())
			.Returns((string?)null);
		});

		snapClient.GroupSetStreamAsync("4dcc4e3b-c699-a04b-7f0c-8260d23c43e1", "stream 1").Wait();
		connectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_GroupSetClientsAsync()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"4dcc4e3b-c699-a04b-7f0c-8260d23c43e1\",\"clients\":[\"00:21:6a:7d:74:fc\"]},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Group.SetClients\"}";

		connectionMock.Setup(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.GroupSetClientsResponse())
			.Returns((string?)null);
		});

		snapClient.GroupSetClientsAsync("4dcc4e3b-c699-a04b-7f0c-8260d23c43e1", [ "00:21:6a:7d:74:fc" ]).Wait();
		connectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_GroupSetNameAsync()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"4dcc4e3b-c699-a04b-7f0c-8260d23c43e1\",\"name\":\"GroundFloor\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Group.SetName\"}";

		connectionMock.Setup(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.GroupSetNameResponse())
			.Returns((string?)null);
		});

		snapClient.GroupSetNameAsync("4dcc4e3b-c699-a04b-7f0c-8260d23c43e1", "GroundFloor").Wait();
		connectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_ServerGetRpcVersionAsync()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var expectedCommand = "{\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Server.GetRPCVersion\"}";

		connectionMock.Setup(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.ServerGetRpcVersionResponse())
			.Returns((string?)null);
		});

		var responseTask = snapClient.ServerGetRpcVersionAsync();
		var response = responseTask.Result;
		connectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);

		Assert.IsNotNull(response);

		Assert.That(response.Major, Is.EqualTo(2));
		Assert.That(response.Minor, Is.EqualTo(0));
		Assert.That(response.Patch, Is.EqualTo(0));
	}

	[Test]
	public void Test_ServerGetStatusAsync()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var expectedCommand = "{\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Server.GetStatus\"}";

		connectionMock.Setup(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.ServerGetStatusResponse())
			.Returns((string?)null);
		});

		var responseTask = snapClient.ServerGetStatusAsync();
		var response = responseTask.Result;
		connectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);

		Assert.IsNotNull(response);

		ValidateServerInfo(response.ServerInfo);

		var groups = response.Groups;
		Assert.That(groups.Count, Is.EqualTo(1));

		var group = groups[0];
		Assert.That(group.Id, Is.EqualTo("4dcc4e3b-c699-a04b-7f0c-8260d23c43e1"));
		Assert.That(group.StreamId, Is.EqualTo("stream 1"));
		Assert.That(group.Muted, Is.False);
		Assert.That(group.Name, Is.EqualTo("Kitchen"));
		Assert.That(group.Clients.Count, Is.EqualTo(1));

		ValidateClient(group.Clients[0]);

		var streams = response.Streams;
		Assert.That(streams.Count, Is.EqualTo(1));

		var stream = streams[0];
		Assert.That(stream.Id, Is.EqualTo("stream 1"));
		Assert.That(stream.Status, Is.EqualTo("idle"));
		Assert.That(stream.Uri.Fragment, Is.EqualTo(""));
		Assert.That(stream.Uri.Host, Is.EqualTo(""));
		Assert.That(stream.Uri.Path, Is.EqualTo("/tmp/snapfifo"));
		Assert.That(stream.Uri.Raw, Is.EqualTo("pipe:///tmp/snapfifo?name=stream 1"));
		Assert.That(stream.Uri.Scheme, Is.EqualTo("pipe"));
	}

	[Test]
	public void Test_ServerDeleteClientAsync()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"bla:bla:bla\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Server.DeleteClient\"}";

		connectionMock.Setup(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
				.Returns(ServerResponses.ServerDeleteClientResponse())
				.Returns((string?)null);
		});

		snapClient.ServerDeleteClientAsync("bla:bla:bla").Wait();
		connectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_StreamAddStreamAsync()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var expectedCommand = "{\"params\":{\"streamUri\":\"pipe:///tmp/snapfifo?name=stream 2\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Stream.AddStream\"}";

		connectionMock.Setup(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.StreamAddStreamResponse())
			.Returns((string?)null);
		});

		var responseTask = snapClient.StreamAddStreamAsync("pipe:///tmp/snapfifo?name=stream 2");
		var response = responseTask.Result;
		connectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);

		Assert.That(response, Is.EqualTo("stream 2"));
	}

	[Test]
	public void Test_StreamRemoveStreamAsync()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"stream 2\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Stream.RemoveStream\"}";

		connectionMock.Setup(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.StreamRemoveStreamResponse())
			.Returns((string?)null);
		});

		snapClient.StreamRemoveStreamAsync("stream 2").Wait();
		connectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_OnClientConnect()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var tcs = new TaskCompletionSource<SnapClient>();
		snapClient.OnClientConnect = client =>
		{
			tcs.SetResult(client);
		};

		connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerNotifications.ClientConnectNotification())
			.Returns((string?)null);

		var client = tcs.Task.Result;
		ValidateClient(client);
	}

	[Test]
	public void Test_OnClientDisconnect()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var tcs = new TaskCompletionSource<SnapClient>();
		snapClient.OnClientDisconnect = client =>
		{
			tcs.SetResult(client);
		};

		connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerNotifications.ClientDisconnectNotification())
			.Returns((string?)null);

		var clientResult = tcs.Task.Result;
		ValidateClient(clientResult);
	}

	[Test]
	public void Test_OnClientConnect_Null()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		_ = new Client(connectionMock.Object);

		connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerNotifications.ClientConnectNotification())
			.Returns((string?)null);
	}

	[Test]
	public void Test_OnClientVolumeChanged()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var tcs = new TaskCompletionSource<Params.ClientSetVolume>();
		snapClient.OnClientVolumeChanged = volume =>
		{
			tcs.SetResult(volume);
		};

		connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerNotifications.ClientVolumeChangedNotification())
			.Returns((string?)null);

		var result = tcs.Task.Result;
		Assert.That(result.Id, Is.EqualTo("00:21:6a:7d:74:fc"));
		Assert.That(result.Volume.Muted, Is.False);
		Assert.That(result.Volume.Percent, Is.EqualTo(36));
	}

	[Test]
	public void Test_OnClientLatencyChanged()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var tcs = new TaskCompletionSource<Params.ClientSetLatency>();
		snapClient.OnClientLatencyChanged = latency =>
		{
			tcs.SetResult(latency);
		};

		connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerNotifications.ClientLatencyChangedNotification())
			.Returns((string?)null);

		var result = tcs.Task.Result;
		Assert.That(result.Id, Is.EqualTo("00:21:6a:7d:74:fc"));
		Assert.That(result.Latency, Is.EqualTo(50));
	}

	[Test]
	public void Test_OnClientNameChanged()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		var tcs = new TaskCompletionSource<Params.ClientSetName>();
		snapClient.OnClientNameChanged = name =>
		{
			tcs.SetResult(name);
		};

		connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerNotifications.ClientNameChangedNotification())
			.Returns((string?)null);

		var result = tcs.Task.Result;
		Assert.That(result.Id, Is.EqualTo("00:21:6a:7d:74:fc"));
		Assert.That(result.Name, Is.EqualTo("Laptop"));
	}

	[Test]
	public void Test_StreamOnUpdate()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);
		
		var tcs = new TaskCompletionSource<Models.Stream>();
		snapClient.OnStreamUpdate = stream =>
		{
			tcs.SetResult(stream);
			return Task.CompletedTask;
		};

		connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerNotifications.StreamUpdateNotification())
			.Returns((string?)null);

		var result = tcs.Task.Result;
		Assert.That(result.Id, Is.EqualTo("stream 1"));
		Assert.That(result.Status, Is.EqualTo("idle"));
		Assert.That(result.Uri.Fragment, Is.EqualTo("test"));
		Assert.That(result.Uri.Host, Is.EqualTo("localhost"));
		Assert.That(result.Uri.Path, Is.EqualTo("/tmp/snapfifo"));
		Assert.That(result.Uri.Query.Name, Is.EqualTo("stream 1"));
		Assert.That(result.Uri.Query.ChunkMs, Is.EqualTo("20"));
		Assert.That(result.Uri.Query.Codec, Is.EqualTo("flac"));
		Assert.That(result.Uri.Query.SampleFormat, Is.EqualTo("48000:16:2"));
		Assert.That(result.Uri.Raw, Is.EqualTo("pipe:///tmp/snapfifo?name=stream 1"));
		Assert.That(result.Uri.Scheme, Is.EqualTo("pipe"));
	}

	// Test Client error handling
	[Test]
	public void Test_ClientGetStatusAsync_ClientNotFound()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		connectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
				.Returns(ServerErrors.ClientNotFound())
				.Returns((string?)null);
		});

		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await snapClient.ClientGetStatusAsync("bla:bla:bla"));
		Assert.That(exception.Message, Is.EqualTo("Internal error: Client not found"));
		
	}

	[Test]
	public void Test_ClientSetVolumeAsync_ClientNotFound()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		connectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.ClientNotFound())
			.Returns((string?)null);
		});

		var responseTask = snapClient.ClientSetVolumeAsync("bla:bla:bla", 36);
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Client not found"));
	}

	[Test]
	public void Test_ClientSetLatencyAsync_ClientNotFound()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		connectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.ClientNotFound())
			.Returns((string?)null);
		});

		var responseTask = snapClient.ClientSetLatencyAsync("bla:bla:bla", 10);
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Client not found"));
	}

	[Test]
	public void Test_ClientSetNameAsync_ClientNotFound()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		connectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.ClientNotFound())
			.Returns((string?)null);
		});

		var responseTask = snapClient.ClientSetNameAsync("bla:bla:bla", "Laptop");
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Client not found"));
	}

	[Test]
	public void Test_GroupGetStatusAsync_GroupNotFound()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		connectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.GroupNotFound())
			.Returns((string?)null);
		});

		var responseTask = snapClient.GroupGetStatusAsync("bla:bla:bla");
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Group not found"));
	}

	[Test]
	public void Test_GroupSetMuteAsync_GroupNotFound()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		connectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.GroupNotFound())
			.Returns((string?)null);
		});

		var responseTask = snapClient.GroupSetMuteAsync("bla:bla:bla", true);
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Group not found"));
	}

	[Test]
	public void Test_GroupSetStreamAsync_GroupNotFound()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		connectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.GroupNotFound())
			.Returns((string?)null);
		});

		var responseTask = snapClient.GroupSetStreamAsync("bla:bla:bla", "stream 1");
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Group not found"));
	}

	[Test]
	public void Test_GroupSetStreamAsync_StreamNotFound()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		connectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.StreamNotFound())
			.Returns((string?)null);
		});

		var responseTask = snapClient.GroupSetStreamAsync("4dcc4e3b-c699-a04b-7f0c-8260d23c43e1", "stream 1");
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Stream not found"));
	}

	[Test]
	public void Test_GroupSetClientsAsync_GroupNotFound()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		connectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.GroupNotFound())
			.Returns((string?)null);
		});

		var responseTask = snapClient.GroupSetClientsAsync("bla:bla:bla", [ "00:21:6a:7d:74:fc" ]);
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Group not found"));
	}

	[Test]
	public void Test_GroupSetNameAsync_GroupNotFound()
	{
		Mock<IConnection> connectionMock = new Mock<IConnection>();
		Client snapClient = new Client(connectionMock.Object);

		connectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		connectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			connectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.GroupNotFound())
			.Returns((string?)null);
		});

		var responseTask = snapClient.GroupSetNameAsync("bla:bla:bla", "GroundFloor");
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Group not found"));
	}
}
