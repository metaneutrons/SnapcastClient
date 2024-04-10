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

	[Test]
	public void Test_ClientGetStatusAsync()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"bla:bla:bla\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Client.GetStatus\"}";

		ConnectionMock.Setup(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
				.Returns(ServerResponses.ClientGetStatusResponse())
				.Returns((string?)null);
		});

		var responseTask = Client.ClientGetStatusAsync("bla:bla:bla");
		var response = responseTask.Result;

		ConnectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);

		Assert.IsNotNull(response);
		ValidateClient(response);
	}

	[Test]
	public void Test_ClientSetVolumeAsync()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"bla:bla:bla\",\"volume\":{\"muted\":false,\"percent\":36}},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Client.SetVolume\"}";

		ConnectionMock.Setup(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
				.Returns(ServerResponses.ClientSetVolumeResponse())
				.Returns((string?)null);
		});

		Client.ClientSetVolumeAsync("bla:bla:bla", 36).Wait();
		ConnectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_ClientSetLatencyAsync()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"bla:bla:bla\",\"latency\":10},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Client.SetLatency\"}";

		ConnectionMock.Setup(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
				.Returns(ServerResponses.ClientSetLatencyResponse())
				.Returns((string?)null);
		});

		Client.ClientSetLatencyAsync("bla:bla:bla", 10).Wait();
		ConnectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_ClientSetNameAsync()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"bla:bla:bla\",\"name\":\"Laptop\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Client.SetName\"}";

		ConnectionMock.Setup(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
				.Returns(ServerResponses.ClientSetNameResponse())
				.Returns((string?)null);
		});

		Client.ClientSetNameAsync("bla:bla:bla", "Laptop").Wait();
		ConnectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_GroupGetStatusAsync()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"4dcc4e3b-c699-a04b-7f0c-8260d23c43e1\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Group.GetStatus\"}";

		ConnectionMock.Setup(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.GroupGetStatusResponse())
			.Returns((string?)null);
		});

		var responseTask = Client.GroupGetStatusAsync("4dcc4e3b-c699-a04b-7f0c-8260d23c43e1");
		var response = responseTask.Result;
		ConnectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);

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
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"4dcc4e3b-c699-a04b-7f0c-8260d23c43e1\",\"mute\":true},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Group.SetMute\"}";

		ConnectionMock.Setup(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.GroupSetMuteResponse())
			.Returns((string?)null);
		});
		Client.GroupSetMuteAsync("4dcc4e3b-c699-a04b-7f0c-8260d23c43e1", true).Wait();
		ConnectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_GroupSetStreamAsync()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"4dcc4e3b-c699-a04b-7f0c-8260d23c43e1\",\"stream_id\":\"stream 1\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Group.SetStream\"}";

		ConnectionMock.Setup(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.GroupSetStreamResponse())
			.Returns((string?)null);
		});

		Client.GroupSetStreamAsync("4dcc4e3b-c699-a04b-7f0c-8260d23c43e1", "stream 1").Wait();
		ConnectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_GroupSetClientsAsync()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"4dcc4e3b-c699-a04b-7f0c-8260d23c43e1\",\"clients\":[\"00:21:6a:7d:74:fc\"]},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Group.SetClients\"}";

		ConnectionMock.Setup(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.GroupSetClientsResponse())
			.Returns((string?)null);
		});

		Client.GroupSetClientsAsync("4dcc4e3b-c699-a04b-7f0c-8260d23c43e1", [ "00:21:6a:7d:74:fc" ]).Wait();
		ConnectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_GroupSetNameAsync()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"4dcc4e3b-c699-a04b-7f0c-8260d23c43e1\",\"name\":\"GroundFloor\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Group.SetName\"}";

		ConnectionMock.Setup(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.GroupSetNameResponse())
			.Returns((string?)null);
		});

		Client.GroupSetNameAsync("4dcc4e3b-c699-a04b-7f0c-8260d23c43e1", "GroundFloor").Wait();
		ConnectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_ServerGetRpcVersionAsync()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		var expectedCommand = "{\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Server.GetRPCVersion\"}";

		ConnectionMock.Setup(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.ServerGetRpcVersionResponse())
			.Returns((string?)null);
		});

		var responseTask = Client.ServerGetRpcVersionAsync();
		var response = responseTask.Result;
		ConnectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);

		Assert.IsNotNull(response);

		Assert.That(response.Major, Is.EqualTo(2));
		Assert.That(response.Minor, Is.EqualTo(0));
		Assert.That(response.Patch, Is.EqualTo(0));
	}

	[Test]
	public void Test_ServerGetStatusAsync()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		var expectedCommand = "{\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Server.GetStatus\"}";

		ConnectionMock.Setup(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.ServerGetStatusResponse())
			.Returns((string?)null);
		});

		var responseTask = Client.ServerGetStatusAsync();
		var response = responseTask.Result;
		ConnectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);

		Assert.IsNotNull(response);

		var serverInfo = response.ServerInfo;
		Assert.That(serverInfo.SnapServer.Version, Is.EqualTo("0.10.0"));
		Assert.That(serverInfo.SnapServer.ProtocolVersion, Is.EqualTo(1));
		Assert.That(serverInfo.SnapServer.ControlProtocolVersion, Is.EqualTo(1));
		Assert.That(serverInfo.SnapServer.Name, Is.EqualTo("Snapserver"));

		Assert.That(serverInfo.Host.Arch, Is.EqualTo("x86_64"));
		Assert.That(serverInfo.Host.Ip, Is.EqualTo("127.0.0.1"));
		Assert.That(serverInfo.Host.Mac, Is.EqualTo(""));
		Assert.That(serverInfo.Host.Name, Is.EqualTo("SnapServer"));
		Assert.That(serverInfo.Host.Os, Is.EqualTo("Raspbian GNU/Linux 10 (buster)"));

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
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		var expectedCommand = "{\"params\":{\"id\":\"bla:bla:bla\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Server.DeleteClient\"}";

		ConnectionMock.Setup(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
				.Returns(ServerResponses.ServerDeleteClientResponse())
				.Returns((string?)null);
		});

		Client.ServerDeleteClientAsync("bla:bla:bla").Wait();
		ConnectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);
	}

	[Test]
	public void Test_StreamAddStreamAsync()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		var expectedCommand = "{\"params\":{\"streamUri\":\"pipe:///tmp/snapfifo?name=stream 2\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Stream.AddStream\"}";

		ConnectionMock.Setup(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerResponses.StreamAddStreamResponse())
			.Returns((string?)null);
		});

		var responseTask = Client.StreamAddStreamAsync("pipe:///tmp/snapfifo?name=stream 2");
		var response = responseTask.Result;
		ConnectionMock.Verify(c => c.Send(It.Is<string>(s => s == expectedCommand)), Times.Once);

		Assert.That(response, Is.EqualTo("stream 2"));
	}

	[Test]
	public void Test_OnClientConnect()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		var tcs = new TaskCompletionSource<SnapClient>();
		Client.OnClientConnect = client =>
		{
			tcs.SetResult(client);
		};

		ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerNotifications.ClientConnectNotification())
			.Returns((string?)null);

		var client = tcs.Task.Result;
		ValidateClient(client);
	}

	[Test]
	public void Test_OnClientDisconnect()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		var tcs = new TaskCompletionSource<SnapClient>();
		Client.OnClientDisconnect = client =>
		{
			tcs.SetResult(client);
		};

		ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerNotifications.ClientDisconnectNotification())
			.Returns((string?)null);

		var client = tcs.Task.Result;
		ValidateClient(client);
	}

	[Test]
	public void Test_OnClientConnect_Null()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerNotifications.ClientConnectNotification())
			.Returns((string?)null);
	}

	// Test Client error handling
	[Test]
	public void Test_ClientGetStatusAsync_ClientNotFound()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		ConnectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
				.Returns(ServerErrors.ClientNotFound())
				.Returns((string?)null);
		});

		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await Client.ClientGetStatusAsync("bla:bla:bla"));
		Assert.That(exception.Message, Is.EqualTo("Internal error: Client not found"));
		
	}

	[Test]
	public void Test_ClientSetVolumeAsync_ClientNotFound()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		ConnectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.ClientNotFound())
			.Returns((string?)null);
		});

		var responseTask = Client.ClientSetVolumeAsync("bla:bla:bla", 36);
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Client not found"));
	}

	[Test]
	public void Test_ClientSetLatencyAsync_ClientNotFound()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		ConnectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.ClientNotFound())
			.Returns((string?)null);
		});

		var responseTask = Client.ClientSetLatencyAsync("bla:bla:bla", 10);
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Client not found"));
	}

	[Test]
	public void Test_ClientSetNameAsync_ClientNotFound()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		ConnectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.ClientNotFound())
			.Returns((string?)null);
		});

		var responseTask = Client.ClientSetNameAsync("bla:bla:bla", "Laptop");
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Client not found"));
	}

	[Test]
	public void Test_GroupGetStatusAsync_GroupNotFound()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		ConnectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.GroupNotFound())
			.Returns((string?)null);
		});

		var responseTask = Client.GroupGetStatusAsync("bla:bla:bla");
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Group not found"));
	}

	[Test]
	public void Test_GroupSetMuteAsync_GroupNotFound()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		ConnectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.GroupNotFound())
			.Returns((string?)null);
		});

		var responseTask = Client.GroupSetMuteAsync("bla:bla:bla", true);
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Group not found"));
	}

	[Test]
	public void Test_GroupSetStreamAsync_GroupNotFound()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		ConnectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.GroupNotFound())
			.Returns((string?)null);
		});

		var responseTask = Client.GroupSetStreamAsync("bla:bla:bla", "stream 1");
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Group not found"));
	}

	[Test]
	public void Test_GroupSetStreamAsync_StreamNotFound()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		ConnectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.StreamNotFound())
			.Returns((string?)null);
		});

		var responseTask = Client.GroupSetStreamAsync("4dcc4e3b-c699-a04b-7f0c-8260d23c43e1", "stream 1");
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Stream not found"));
	}

	[Test]
	public void Test_GroupSetClientsAsync_GroupNotFound()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		ConnectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.GroupNotFound())
			.Returns((string?)null);
		});

		var responseTask = Client.GroupSetClientsAsync("bla:bla:bla", [ "00:21:6a:7d:74:fc" ]);
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Group not found"));
	}

	[Test]
	public void Test_GroupSetNameAsync_GroupNotFound()
	{
		Mock<IConnection> ConnectionMock = new Mock<IConnection>();
		Client Client = new Client(ConnectionMock.Object);

		ConnectionMock.SetupSequence(c => c.Read()).Returns((string?)null);
		ConnectionMock.Setup(c => c.Send(It.IsAny<string>())).Callback(() =>
		{
			ConnectionMock.SetupSequence(c => c.Read())
			.Returns(ServerErrors.GroupNotFound())
			.Returns((string?)null);
		});

		var responseTask = Client.GroupSetNameAsync("bla:bla:bla", "GroundFloor");
		var exception = Assert.ThrowsAsync<Commands.CommandException>(async () => await responseTask);
		Assert.That(exception.Message, Is.EqualTo("Internal error: Group not found"));
	}
}
