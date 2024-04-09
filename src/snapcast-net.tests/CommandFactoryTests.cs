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

using SnapCastNet.Commands;
using SnapCastNet.Params;

namespace SnapCastNet.tests;

public class CommandFactoryTests
{
	private CommandFactory CommandFactory;

	[SetUp]
	public void Setup()
	{
		CommandFactory = new CommandFactory();
	}

	[Test]
	public void Test_createCommand_with_CLIENT_GET_STATUS_CommandType_generates_correct_json()
	{
		var command = CommandFactory.createCommand(CommandType.CLIENT_GET_STATUS, new ClientGetStatus { Id = "bla:bla:bla" });
		Assert.That(command, Is.Not.Null);
		Assert.That(command.toJson(), Is.EqualTo("{\"params\":{\"id\":\"bla:bla:bla\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Client.GetStatus\"}"));
	}

	[Test]
	public void Test_createCommand_with_CLIENT_SET_VOLUME_CommandType_generates_correct_json()
	{
		var command = CommandFactory.createCommand(CommandType.CLIENT_SET_VOLUME, new ClientSetVolume { Id = "bla:bla:bla", Volume = new ClientVolume { Muted = false, Percent = 50 } });
		Assert.That(command, Is.Not.Null);
		Assert.That(command.toJson(), Is.EqualTo("{\"params\":{\"id\":\"bla:bla:bla\",\"volume\":{\"muted\":false,\"percent\":50}},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Client.SetVolume\"}"));
	}

	[Test]
	public void Test_createCommand_with_CLIENT_SET_LATENCY_CommandType_generates_correct_json()
	{
		var command = CommandFactory.createCommand(CommandType.CLIENT_SET_LATENCY, new ClientSetLatency { Id = "bla:bla:bla", Latency = 50 });
		Assert.That(command, Is.Not.Null);
		Assert.That(command.toJson(), Is.EqualTo("{\"params\":{\"id\":\"bla:bla:bla\",\"latency\":50},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Client.SetLatency\"}"));
	}

	[Test]
	public void Test_createCommand_with_CLIENT_SET_NAME_CommandType_generates_correct_json()
	{
		var command = CommandFactory.createCommand(CommandType.CLIENT_SET_NAME, new ClientSetName { Id = "bla:bla:bla", Name = "new name" });
		Assert.That(command, Is.Not.Null);
		Assert.That(command.toJson(), Is.EqualTo("{\"params\":{\"id\":\"bla:bla:bla\",\"name\":\"new name\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Client.SetName\"}"));
	}

	[Test]
	public void Test_createCommand_with_GROUP_GET_STATUS_CommandType_generates_correct_json()
	{
		var command = CommandFactory.createCommand(CommandType.GROUP_GET_STATUS, new GroupGetStatus { Id = "bla:bla:bla" });
		Assert.That(command, Is.Not.Null);
		Assert.That(command.toJson(), Is.EqualTo("{\"params\":{\"id\":\"bla:bla:bla\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Group.GetStatus\"}"));
	}

	[Test]
	public void Test_createCommand_with_GROUP_SET_MUTE_CommandType_generates_correct_json()
	{
		var command = CommandFactory.createCommand(CommandType.GROUP_SET_MUTE, new GroupSetMute { Id = "bla:bla:bla", Mute = true });
		Assert.That(command, Is.Not.Null);
		Assert.That(command.toJson(), Is.EqualTo("{\"params\":{\"id\":\"bla:bla:bla\",\"mute\":true},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Group.SetMute\"}"));
	}

	[Test]
	public void Test_createCommand_with_GROUP_SET_STREAM_CommandType_generates_correct_json()
	{
		var command = CommandFactory.createCommand(CommandType.GROUP_SET_STREAM, new GroupSetStream { Id = "bla:bla:bla", StreamId = "stream" });
		Assert.That(command, Is.Not.Null);
		Assert.That(command.toJson(), Is.EqualTo("{\"params\":{\"id\":\"bla:bla:bla\",\"stream_id\":\"stream\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Group.SetStream\"}"));
	}

	[Test]
	public void Test_createCommand_with_GROUP_SET_CLIENTS_CommandType_generates_correct_json()
	{
		var command = CommandFactory.createCommand(CommandType.GROUP_SET_CLIENTS, new GroupSetClients { Id = "bla:bla:bla", Clients = [ "client1", "client2" ] });
		Assert.That(command, Is.Not.Null);
		Assert.That(command.toJson(), Is.EqualTo("{\"params\":{\"id\":\"bla:bla:bla\",\"clients\":[\"client1\",\"client2\"]},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Group.SetClients\"}"));
	}

	[Test]
	public void Test_createCommand_with_SERVER_GET_RPC_VERSION_CommandType_generates_correct_json()
	{
		var command = CommandFactory.createCommand(CommandType.SERVER_GET_RPC_VERSION, new NullParams());
		Assert.That(command, Is.Not.Null);
		Assert.That(command.toJson(), Is.EqualTo("{\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Server.GetRPCVersion\"}"));
	}

	[Test]
	public void Test_createCommand_with_SERVER_GET_STATUS_CommandType_generates_correct_json()
	{
		var command = CommandFactory.createCommand(CommandType.SERVER_GET_STATUS, new NullParams());
		Assert.That(command, Is.Not.Null);
		Assert.That(command.toJson(), Is.EqualTo("{\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Server.GetStatus\"}"));
	}

	[Test]
	public void Test_createCommand_with_SERVER_DELETE_CLIENT_CommandType_generates_correct_json()
	{
		var command = CommandFactory.createCommand(CommandType.SERVER_DELETE_CLIENT, new ClientId { Id = "bla:bla:bla" });
		Assert.That(command, Is.Not.Null);
		Assert.That(command.toJson(), Is.EqualTo("{\"params\":{\"id\":\"bla:bla:bla\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Server.DeleteClient\"}"));
	}

	[Test]
	public void Test_createCommand_with_GROUP_SET_NAME_CommandType_generates_correct_json()
	{
		var command = CommandFactory.createCommand(CommandType.GROUP_SET_NAME, new GroupSetName { Id = "bla:bla:bla", Name = "new name" });
		Assert.That(command, Is.Not.Null);
		Assert.That(command.toJson(), Is.EqualTo("{\"params\":{\"id\":\"bla:bla:bla\",\"name\":\"new name\"},\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"Group.SetName\"}"));
	}
}