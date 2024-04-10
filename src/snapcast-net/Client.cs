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

using Newtonsoft.Json;
using SnapCastNet.Commands;
using SnapCastNet.Models;
using SnapCastNet.Params;
using SnapCastNet.Responses;

namespace SnapCastNet;

public class Client : IClient
{
	private IConnection Connection;
	private CommandFactory CommandFactory = new CommandFactory();

	private Mutex CommandMutex = new Mutex();
	private Thread ResponseReader;
	private bool Listening = true;
	private Dictionary<uint, IResponseHandler> ResponseHandlers = new Dictionary<uint, IResponseHandler>();

	public Action<SnapClient>? OnClientConnect { set; get; }
	public Action<SnapClient>? OnClientDisconnect { set; get; }

	public Client(IConnection connection)
	{
		Connection = connection;

        ResponseReader = new Thread(ListenForResponses)
        {
            Name = "SnapCastResponseReader"
        };
        ResponseReader.Start();
	}

	~Client()
	{
		Listening = false;
		ResponseReader.Join();
	}

	private void ListenForResponses()
	{
		while (Listening)
		{
			var response = Connection.Read();
			if (response == null)
				continue;

			response.Split('\n').ToList().ForEach(HandleResponse);
		}
	}

	private void HandleResponse(string response)
	{
		if (response.Length == 0)
			return;

		var peek = JsonConvert.DeserializeObject<RpcResponsePeek>(response);
		if (peek.Id != null)
		{
			var id = peek.Id.Value;
			CommandMutex.WaitOne();

			IResponseHandler? responseHandler = null;
			try
			{
				responseHandler = ResponseHandlers[id];
			}
			catch (KeyNotFoundException)
			{
				CommandMutex.ReleaseMutex();
				var responseHandlerIds = new List<int>{};
				foreach (var key in ResponseHandlers.Keys)
					Console.WriteLine(key);
				throw;
			}

			if (peek.Error == null)
				responseHandler.HandleResponse(response);
			else
				responseHandler.HandleError(peek.Error.Value);

            ResponseHandlers.Remove(id);
			CommandMutex.ReleaseMutex();
		}
		else
		{
			HandleNotification(peek.Method, response);
		}
	}

	private void HandleNotification(string method, string response)
	{
		if(method == "Client.OnConnect")
		{
			var notification = JsonConvert.DeserializeObject<RpcNotification<ClientStatus>>(response);
			OnClientConnect?.Invoke(notification.Params.Client);
		}
		else if(method == "Client.OnDisconnect")
		{
			var notification = JsonConvert.DeserializeObject<RpcNotification<ClientStatus>>(response);
			OnClientDisconnect?.Invoke(notification.Params.Client);
		}
	}

	private void Execute(ICommand command, IResponseHandler responseHandler)
	{
		CommandMutex.WaitOne();
		ResponseHandlers.Add(command.Id, responseHandler);
		Connection.Send(command.toJson());
		CommandMutex.ReleaseMutex();
	}

	public async Task<Models.SnapClient> ClientGetStatusAsync(string id)
	{
		var command = CommandFactory.createCommand(CommandType.CLIENT_GET_STATUS, new Params.ClientGetStatus { Id = id });
		if(command == null)
			throw new Exception("Failed to create Client.GetStatus command");

		var tcs = new TaskCompletionSource<ClientStatus>();
		Execute(command, new ResponseHandler<ClientStatus>(tcs.SetResult, e => tcs.SetException(new CommandException(e))));

		var response = await tcs.Task;
		return response.Client;
	}

	public async Task ClientSetVolumeAsync(string id, int volume)
	{
		var command = CommandFactory.createCommand(CommandType.CLIENT_SET_VOLUME,
			new Params.ClientSetVolume {
				Id = id,
				Volume = new Params.ClientVolume { Muted = false, Percent = volume }
			}
		);
		if(command == null)
			throw new Exception("Failed to create Client.SetVolume command");

		var tcs = new TaskCompletionSource<VolumeSet>();
		Execute(command, new ResponseHandler<VolumeSet>(tcs.SetResult, e => tcs.SetException(new CommandException(e))));
		await tcs.Task;
	}

	public async Task ClientSetLatencyAsync(string id, int latency)
	{
		var command = CommandFactory.createCommand(CommandType.CLIENT_SET_LATENCY, new Params.ClientSetLatency { Id = id, Latency = latency });
		if(command == null)
			throw new Exception("Failed to create Client.SetLatency command");
	
		var tcs = new TaskCompletionSource<LatencySet>();
		Execute(command, new ResponseHandler<LatencySet>(tcs.SetResult, e => tcs.SetException(new CommandException(e))));
		await tcs.Task;
	}

	public async Task ClientSetNameAsync(string id, string name)
	{
		var command = CommandFactory.createCommand(CommandType.CLIENT_SET_NAME, new Params.ClientSetName { Id = id, Name = name });
		if(command == null)
			throw new Exception("Failed to create Client.SetName command");

		var tcs = new TaskCompletionSource<NameSet>();
		Execute(command, new ResponseHandler<NameSet>(tcs.SetResult, e => tcs.SetException(new CommandException(e))));
		await tcs.Task;
	}

	public async Task<Models.Group> GroupGetStatusAsync(string id)
	{
		var command = CommandFactory.createCommand(CommandType.GROUP_GET_STATUS, new Params.GroupGetStatus { Id = id });
		if(command == null)
			throw new Exception("Failed to create Group.GetStatus command");

		var tcs = new TaskCompletionSource<GroupStatus>();
		Execute(command, new ResponseHandler<GroupStatus>(tcs.SetResult, e => tcs.SetException(new CommandException(e))));

		var response = await tcs.Task;
		return response.Group;
	}

	public async Task GroupSetMuteAsync(string id, bool mute)
	{
		var command = CommandFactory.createCommand(CommandType.GROUP_SET_MUTE, new Params.GroupSetMute { Id = id, Mute = mute });
		if(command == null)
			throw new Exception("Failed to create Group.SetMute command");

		var tcs = new TaskCompletionSource<GroupMuteSet>();
		Execute(command, new ResponseHandler<GroupMuteSet>(tcs.SetResult, e => tcs.SetException(new CommandException(e))));
		await tcs.Task;
	}

	public async Task GroupSetStreamAsync(string id, string streamId)
	{
		var command = CommandFactory.createCommand(CommandType.GROUP_SET_STREAM, new Params.GroupSetStream { Id = id, StreamId = streamId });
		if(command == null)
			throw new Exception("Failed to create Group.SetStream command");

		var tcs = new TaskCompletionSource<GroupStreamSet>();
		Execute(command, new ResponseHandler<GroupStreamSet>(tcs.SetResult, e => tcs.SetException(new CommandException(e))));
		await tcs.Task;
	}

	public async Task GroupSetClientsAsync(string id, List<string> clients)
	{
		var command = CommandFactory.createCommand(CommandType.GROUP_SET_CLIENTS, new Params.GroupSetClients { Id = id, Clients = clients });
		if(command == null)
			throw new Exception("Failed to create Group.SetClients command");

		var tcs = new TaskCompletionSource<GroupClientsSet>();
		Execute(command, new ResponseHandler<GroupClientsSet>(tcs.SetResult, e => tcs.SetException(new CommandException(e))));
		await tcs.Task;
	}

	public async Task GroupSetNameAsync(string id, string name)
	{
		var command = CommandFactory.createCommand(CommandType.GROUP_SET_NAME, new Params.GroupSetName { Id = id, Name = name });
		if(command == null)
			throw new Exception("Failed to create Group.SetName command");
	
		var tcs = new TaskCompletionSource<GroupNameSet>();
		Execute(command, new ResponseHandler<GroupNameSet>(tcs.SetResult, e => tcs.SetException(new CommandException(e))));
		await tcs.Task;
	}

	public async Task<Models.RpcVersion> ServerGetRpcVersionAsync()
	{
		var command = CommandFactory.createCommand(CommandType.SERVER_GET_RPC_VERSION, new NullParams());
		if(command == null)
			throw new Exception("Failed to create Server.GetRpcVersion command");

		var tcs = new TaskCompletionSource<RpcVersion>();
		Execute(command, new ResponseHandler<RpcVersion>(tcs.SetResult, e => tcs.SetException(new CommandException(e))));
		return await tcs.Task;
	}

	public async Task<Models.Server> ServerGetStatusAsync()
	{
		var command = CommandFactory.createCommand(CommandType.SERVER_GET_STATUS, new NullParams());
		if(command == null)
			throw new Exception("Failed to create Server.GetStatus command");

		var tcs = new TaskCompletionSource<ServerStatus>();
		Execute(command, new ResponseHandler<ServerStatus>(tcs.SetResult));

		var response = await tcs.Task;
		return response.Server;
	}

	public async Task<string> StreamAddStreamAsync(string streamUri)
	{
		var command = CommandFactory.createCommand(CommandType.STREAM_ADD_STREAM, new Params.StreamAddStream { StreamUri = streamUri });
		if (command == null)
			throw new Exception("Failed to create Stream.AddStream command");

		var tcs = new TaskCompletionSource<AddRemove>();
		Execute(command, new ResponseHandler<AddRemove>(tcs.SetResult));

		var response = await tcs.Task;
		return response.StreamId;
	}
}
