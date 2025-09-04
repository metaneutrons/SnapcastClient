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

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SnapcastClient.Commands;
using SnapcastClient.Models;
using SnapcastClient.Params;
using SnapcastClient.Responses;

namespace SnapcastClient;

/// <summary>
/// Main client for communicating with a Snapcast server via TCP connection.
/// Provides methods for controlling clients, groups, streams, and server operations.
/// </summary>
/// <summary>
/// Main client for communicating with a Snapcast server via TCP connection.
/// Provides methods for controlling clients, groups, streams, and server operations.
/// </summary>
public class Client : IClient, IDisposable
{
    private readonly IConnection Connection;
    private readonly ILogger<Client>? _logger;
    private readonly CommandFactory CommandFactory = new CommandFactory();

    private readonly Mutex CommandMutex = new Mutex();
    private readonly Thread ResponseReader;
    private readonly CancellationTokenSource _listenerCancellation = new();
    private bool Listening = true;
    private readonly Dictionary<uint, IResponseHandler> ResponseHandlers =
        new Dictionary<uint, IResponseHandler>();
    private bool _disposed = false;

    /// <summary>
    /// Event fired when a client connects to the server.
    /// </summary>
    /// <summary>
    /// Event fired when a client connects to the server.
    /// </summary>
    public Action<SnapClient>? OnClientConnect { set; get; }

    /// <summary>
    /// Event fired when a client disconnects from the server.
    /// </summary>
    /// <summary>
    /// Event fired when a client disconnects from the server.
    /// </summary>
    public Action<SnapClient>? OnClientDisconnect { set; get; }

    /// <summary>
    /// Event fired when a client's volume changes.
    /// </summary>
    /// <summary>
    /// Event fired when a client\'s volume changes.
    /// </summary>
    public Action<Params.ClientSetVolume>? OnClientVolumeChanged { set; get; }

    /// <summary>
    /// Event fired when a client's latency changes.
    /// </summary>
    /// <summary>
    /// Event fired when a client\'s latency changes.
    /// </summary>
    public Action<Params.ClientSetLatency>? OnClientLatencyChanged { set; get; }

    /// <summary>
    /// Event fired when a client's name changes.
    /// </summary>
    /// <summary>
    /// Event fired when a client\'s name changes.
    /// </summary>
    public Action<Params.ClientSetName>? OnClientNameChanged { set; get; }

    /// <summary>
    /// Event fired when a group's mute status changes.
    /// </summary>
    /// <summary>
    /// Event fired when a group\'s mute status changes.
    /// </summary>
    public Action<Params.GroupOnMute>? OnGroupMute { set; get; }

    /// <summary>
    /// Event fired when a group's stream changes.
    /// </summary>
    /// <summary>
    /// Event fired when a group\'s stream changes.
    /// </summary>
    public Action<Params.GroupOnStreamChanged>? OnGroupStreamChanged { set; get; }

    /// <summary>
    /// Event fired when a group's name changes.
    /// </summary>
    /// <summary>
    /// Event fired when a group\'s name changes.
    /// </summary>
    public Action<Params.GroupOnNameChanged>? OnGroupNameChanged { set; get; }

    /// <summary>
    /// Event fired when stream properties change.
    /// </summary>
    /// <summary>
    /// Event fired when stream properties change.
    /// </summary>
    public Action<Params.StreamOnProperties>? OnStreamProperties { set; get; }

    /// <summary>
    /// Event fired when a stream is updated.
    /// </summary>
    /// <summary>
    /// Event fired when a stream is updated.
    /// </summary>
    public Func<Models.Stream, Task>? OnStreamUpdate { set; get; }

    /// <summary>
    /// Event fired when the server is updated.
    /// </summary>
    /// <summary>
    /// Event fired when the server is updated.
    /// </summary>
    public Action<Models.Server>? OnServerUpdate { set; get; }

    /// <summary>
    /// Initializes a new instance of the Client class.
    /// </summary>
    /// <param name="connection">The connection to use for communication with the server.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    public Client(IConnection connection, ILogger<Client>? logger = null)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger;

        _logger?.LogInformation("Initializing Snapcast client");

        // Start both legacy thread-based and new async-based response listeners
        ResponseReader = new Thread(ListenForResponses) { Name = "SnapCastResponseReader" };
        ResponseReader.Start();

        // Start the new enterprise-grade async response listener
        _ = Task.Run(async () => await ListenForResponsesAsync(_listenerCancellation.Token));

        _logger?.LogDebug("Response reader thread and async listener started");
    }

    /// <summary>
    /// Releases all resources used by the Client.
    /// </summary>
    /// <summary>
    /// Releases all resources used by the Client.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _logger?.LogInformation("Disposing Snapcast client");

        Listening = false;
        _listenerCancellation.Cancel();

        // Wait for both legacy thread and async processing to complete
        ResponseReader?.Join(TimeSpan.FromSeconds(5)); // Wait up to 5 seconds for thread to finish

        // Stop the async message processing pipeline
        try
        {
            Connection.StopMessageProcessingAsync().Wait(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error stopping async message processing during disposal");
        }

        if (Connection is IDisposable disposableConnection)
        {
            disposableConnection.Dispose();
        }

        CommandMutex?.Dispose();
        _listenerCancellation?.Dispose();
        _disposed = true;

        _logger?.LogDebug("Snapcast client disposed");
    }

    private void ListenForResponses()
    {
        _logger?.LogDebug("Starting response listener thread");

        while (Listening)
        {
            try
            {
                var response = Connection.Read();
                if (response == null)
                {
                    // Add a small delay to prevent tight CPU loop when connection is not available
                    Thread.Sleep(100);
                    continue;
                }

                if (_logger?.IsEnabled(LogLevel.Trace) == true)
                {
                    _logger.LogTrace("Received response: {Response}", response);
                }

                response.Split('\n').ToList().ForEach(HandleResponse);
            }
            catch (Exception ex)
            {
                if (Listening) // Only log if we're still supposed to be listening
                {
                    _logger?.LogError(ex, "Error in response listener thread");
                }

                // Add a small delay after exceptions to prevent tight error loops
                Thread.Sleep(100);
            }
        }

        _logger?.LogDebug("Response listener thread stopped");
    }

    private void HandleResponse(string response)
    {
        if (response.Length == 0)
            return;

        RpcResponsePeek peek;
        try
        {
            peek = JsonConvert.DeserializeObject<RpcResponsePeek>(response);
        }
        catch (JsonReaderException e)
        {
            _logger?.LogError(e, "Error parsing JSON response: {Response}", response);
            return;
        }
        catch (JsonSerializationException e)
        {
            _logger?.LogError(
                e,
                "JSON serialization error. Response length: {Length}, Response: {Response}",
                response.Length,
                response.Length > 500 ? response.Substring(0, 500) + "..." : response
            );
            return;
        }
        catch (ArgumentException e)
        {
            _logger?.LogError(
                e,
                "Argument error during JSON deserialization. Response: {Response}",
                response.Length > 200 ? response.Substring(0, 200) + "..." : response
            );
            return;
        }

        if (peek.Id != null)
        {
            var id = peek.Id.Value;
            _logger?.LogDebug("Handling response for command ID: {CommandId}", id);

            CommandMutex.WaitOne();

            IResponseHandler? responseHandler = null;
            try
            {
                responseHandler = ResponseHandlers[id];
            }
            catch (KeyNotFoundException)
            {
                CommandMutex.ReleaseMutex();
                _logger?.LogWarning(
                    "No response handler found for command ID: {CommandId}. Available handlers: {HandlerIds}",
                    id,
                    string.Join(", ", ResponseHandlers.Keys)
                );
                return;
            }

            try
            {
                if (peek.Error == null)
                {
                    responseHandler.HandleResponse(response);
                    _logger?.LogDebug(
                        "Successfully handled response for command ID: {CommandId}",
                        id
                    );
                }
                else
                {
                    _logger?.LogWarning(
                        "Command ID {CommandId} returned error: {Error}",
                        id,
                        peek.Error.Value
                    );
                    responseHandler.HandleError(peek.Error.Value);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error handling response for command ID: {CommandId}", id);
            }
            finally
            {
                ResponseHandlers.Remove(id);
                CommandMutex.ReleaseMutex();
            }
        }
        else
        {
            _logger?.LogDebug("Handling notification: {Method}", peek.Method);
            HandleNotification(peek.Method, response);
        }
    }

    private void HandleNotification(string method, string response)
    {
        if (method == "Client.OnConnect")
        {
            var notification = JsonConvert.DeserializeObject<RpcNotification<ClientStatus>>(
                response
            );
            OnClientConnect?.Invoke(notification.Params.Client);
        }
        else if (method == "Client.OnDisconnect")
        {
            var notification = JsonConvert.DeserializeObject<RpcNotification<ClientStatus>>(
                response
            );
            OnClientDisconnect?.Invoke(notification.Params.Client);
        }
        else if (method == "Client.OnVolumeChanged")
        {
            var notification = JsonConvert.DeserializeObject<
                RpcNotification<Params.ClientSetVolume>
            >(response);
            OnClientVolumeChanged?.Invoke(notification.Params);
        }
        else if (method == "Client.OnLatencyChanged")
        {
            var notification = JsonConvert.DeserializeObject<
                RpcNotification<Params.ClientSetLatency>
            >(response);
            OnClientLatencyChanged?.Invoke(notification.Params);
        }
        else if (method == "Client.OnNameChanged")
        {
            var notification = JsonConvert.DeserializeObject<RpcNotification<Params.ClientSetName>>(
                response
            );
            OnClientNameChanged?.Invoke(notification.Params);
        }
        else if (method == "Group.OnMute")
        {
            var notification = JsonConvert.DeserializeObject<RpcNotification<Params.GroupOnMute>>(
                response
            );
            OnGroupMute?.Invoke(notification.Params);
        }
        else if (method == "Group.OnStreamChanged")
        {
            var notification = JsonConvert.DeserializeObject<
                RpcNotification<Params.GroupOnStreamChanged>
            >(response);
            OnGroupStreamChanged?.Invoke(notification.Params);
        }
        else if (method == "Group.OnNameChanged")
        {
            var notification = JsonConvert.DeserializeObject<
                RpcNotification<Params.GroupOnNameChanged>
            >(response);
            OnGroupNameChanged?.Invoke(notification.Params);
        }
        else if (method == "Stream.OnProperties")
        {
            var notification = JsonConvert.DeserializeObject<
                RpcNotification<Params.StreamOnProperties>
            >(response);
            OnStreamProperties?.Invoke(notification.Params);
        }
        else if (method == "Stream.OnUpdate")
        {
            var notification = JsonConvert.DeserializeObject<
                RpcNotification<Params.StreamOnUpdate>
            >(response);
            OnStreamUpdate?.Invoke(notification.Params.Stream);
        }
        else if (method == "Server.OnUpdate")
        {
            var notification = JsonConvert.DeserializeObject<
                RpcNotification<Params.ServerOnUpdate>
            >(response);
            OnServerUpdate?.Invoke(notification.Params.Server);
        }
    }

    /// <summary>
    /// Enterprise-grade async response listener using producer-consumer pattern (Phase 2).
    /// This method integrates with the TcpConnection's message processing pipeline.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop listening.</param>
    /// <returns>Task representing the async listening operation.</returns>
    private async Task ListenForResponsesAsync(CancellationToken cancellationToken)
    {
        _logger?.LogDebug("Starting enterprise-grade async response listener");

        try
        {
            // Subscribe to the connection's message events
            Connection.MessageReceived += OnMessageReceived;
            Connection.ProcessingError += OnProcessingError;
            Connection.ConnectionHealthChanged += OnConnectionHealthChanged;

            // Start the producer-consumer message processing pipeline
            await Connection.StartMessageProcessingAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger?.LogDebug("Async response listener cancelled");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in async response listener");
        }
        finally
        {
            // Unsubscribe from events
            Connection.MessageReceived -= OnMessageReceived;
            Connection.ProcessingError -= OnProcessingError;
            Connection.ConnectionHealthChanged -= OnConnectionHealthChanged;

            _logger?.LogDebug("Async response listener stopped");
        }
    }

    /// <summary>
    /// Handles messages received from the producer-consumer pipeline.
    /// </summary>
    /// <param name="message">The received message.</param>
    private void OnMessageReceived(string message)
    {
        try
        {
            if (_logger?.IsEnabled(LogLevel.Trace) == true)
            {
                _logger.LogTrace("Async listener received message: {Message}", message);
            }

            // Process the message using the existing HandleResponse logic
            message.Split('\n').ToList().ForEach(HandleResponse);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling async message: {Message}", message);
        }
    }

    /// <summary>
    /// Handles processing errors from the producer-consumer pipeline.
    /// </summary>
    /// <param name="error">The processing error.</param>
    private void OnProcessingError(Exception error)
    {
        _logger?.LogError(error, "Processing error in message pipeline");
    }

    /// <summary>
    /// Handles connection health changes from the producer-consumer pipeline.
    /// </summary>
    /// <param name="isHealthy">True if the connection is healthy, false otherwise.</param>
    private void OnConnectionHealthChanged(bool isHealthy)
    {
        if (isHealthy)
        {
            _logger?.LogInformation("Connection health restored");
            
            // Log detailed restoration info
            try
            {
                var diagnostics = Connection.GetDiagnostics();
                _logger?.LogDebug("Connection restored - Response time improved, Total messages: {TotalMessages}, Downtime: {TotalDowntime:F1}s", 
                    diagnostics.HealthStats.TotalMessages, 
                    diagnostics.HealthStats.TotalDowntime.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Could not retrieve connection diagnostics on health restoration");
            }
        }
        else
        {
            // Enhanced degradation logging with detailed reasons
            try
            {
                var diagnostics = Connection.GetDiagnostics();
                var healthStats = diagnostics.HealthStats;
                var circuitBreakerStats = diagnostics.CircuitBreakerStats;
                
                _logger?.LogWarning("Connection health degraded - Last message: {TimeSinceLastMessage:F1}s ago (threshold: 30.0s), " +
                    "Total messages: {TotalMessages}, Circuit breaker: {CircuitBreakerState}, " +
                    "Failed attempts: {FailedAttempts}",
                    healthStats.TimeSinceLastMessage.TotalSeconds,
                    healthStats.TotalMessages,
                    circuitBreakerStats.State,
                    circuitBreakerStats.FailureCount);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning("Connection health degraded - Unable to retrieve detailed diagnostics: {Error}", ex.Message);
            }
        }
    }

    private void Execute(ICommand command, IResponseHandler responseHandler)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Client));

        _logger?.LogDebug(
            "Executing command: {CommandType} with ID: {CommandId}",
            command.GetType().Name,
            command.Id
        );

        CommandMutex.WaitOne();
        try
        {
            ResponseHandlers.Add(command.Id, responseHandler);
            var json = command.toJson();

            if (_logger?.IsEnabled(LogLevel.Trace) == true)
            {
                _logger.LogTrace("Sending command JSON: {Json}", json);
            }

            Connection.Send(json);
            _logger?.LogDebug("Command sent successfully: {CommandId}", command.Id);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Failed to execute command: {CommandType} with ID: {CommandId}",
                command.GetType().Name,
                command.Id
            );
            ResponseHandlers.Remove(command.Id);
            throw;
        }
        finally
        {
            CommandMutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// Retrieves the status of the SnapClient.
    /// </summary>
    /// <param name="id">The ID of the client.</param>
    /// <returns>The status of the client.</returns>
    public async Task<Models.SnapClient> ClientGetStatusAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(
                "Client ID cannot be null, empty, or whitespace",
                nameof(id)
            );

        _logger?.LogInformation("Getting status for client: {ClientId}", id);

        var command = CommandFactory.createCommand(
            CommandType.CLIENT_GET_STATUS,
            new Params.ClientGetStatus { Id = id }
        );
        if (command == null)
        {
            _logger?.LogError(
                "Failed to create Client.GetStatus command for client: {ClientId}",
                id
            );
            throw new Exception("Failed to create Client.GetStatus command");
        }

        var tcs = new TaskCompletionSource<ClientStatus>();
        Execute(
            command,
            new ResponseHandler<ClientStatus>(
                tcs.SetResult,
                e => tcs.SetException(new CommandException(e))
            )
        );

        try
        {
            var response = await tcs.Task;
            _logger?.LogDebug("Successfully retrieved status for client: {ClientId}", id);
            return response.Client;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get status for client: {ClientId}", id);
            throw;
        }
    }

    /// <summary>
    /// Sets the volume of a client.
    /// </summary>
    /// <param name="id">The ID of the client.</param>
    /// <param name="volume">The volume level to set.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ClientSetVolumeAsync(string id, int volume)
    {
        await ClientSetVolumeAsync(id, volume, false);
    }

    /// <summary>
    /// Sets the volume and mute state of a client.
    /// </summary>
    /// <param name="id">The ID of the client.</param>
    /// <param name="volume">The volume level to set.</param>
    /// <param name="muted">Whether the client should be muted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ClientSetVolumeAsync(string id, int volume, bool muted)
    {
        var command = CommandFactory.createCommand(
            CommandType.CLIENT_SET_VOLUME,
            new Params.ClientSetVolume
            {
                Id = id,
                Volume = new Params.ClientVolume { Muted = muted, Percent = volume },
            }
        );
        if (command == null)
            throw new Exception("Failed to create Client.SetVolume command");

        var tcs = new TaskCompletionSource<VolumeSet>();
        Execute(
            command,
            new ResponseHandler<VolumeSet>(
                tcs.SetResult,
                e => tcs.SetException(new CommandException(e))
            )
        );
        await tcs.Task;
    }

    /// <summary>
    /// Sets the latency of a client.
    /// </summary>
    /// <param name="id">The ID of the client.</param>
    /// <param name="latency">The latency value to set.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ClientSetLatencyAsync(string id, int latency)
    {
        var command = CommandFactory.createCommand(
            CommandType.CLIENT_SET_LATENCY,
            new Params.ClientSetLatency { Id = id, Latency = latency }
        );
        if (command == null)
            throw new Exception("Failed to create Client.SetLatency command");

        var tcs = new TaskCompletionSource<LatencySet>();
        Execute(
            command,
            new ResponseHandler<LatencySet>(
                tcs.SetResult,
                e => tcs.SetException(new CommandException(e))
            )
        );
        await tcs.Task;
    }

    /// <summary>
    /// Sets the name of a client.
    /// </summary>
    /// <param name="id">The ID of the client.</param>
    /// <param name="name">The name to set for the client.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ClientSetNameAsync(string id, string name)
    {
        var command = CommandFactory.createCommand(
            CommandType.CLIENT_SET_NAME,
            new Params.ClientSetName { Id = id, Name = name }
        );
        if (command == null)
            throw new Exception("Failed to create Client.SetName command");

        var tcs = new TaskCompletionSource<NameSet>();
        Execute(
            command,
            new ResponseHandler<NameSet>(
                tcs.SetResult,
                e => tcs.SetException(new CommandException(e))
            )
        );
        await tcs.Task;
    }

    /// <summary>
    /// Gets the status of a group.
    /// </summary>
    /// <param name="id">The ID of the group.</param>
    /// <returns>The requested group.</returns>
    public async Task<Models.Group> GroupGetStatusAsync(string id)
    {
        var command = CommandFactory.createCommand(
            CommandType.GROUP_GET_STATUS,
            new Params.GroupGetStatus { Id = id }
        );
        if (command == null)
            throw new Exception("Failed to create Group.GetStatus command");

        var tcs = new TaskCompletionSource<GroupStatus>();
        Execute(
            command,
            new ResponseHandler<GroupStatus>(
                tcs.SetResult,
                e => tcs.SetException(new CommandException(e))
            )
        );

        var response = await tcs.Task;
        return response.Group;
    }

    /// <summary>
    /// Sets the mute state of a group.
    /// </summary>
    /// <param name="id">The ID of the group.</param>
    /// <param name="mute">The mute state to set.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GroupSetMuteAsync(string id, bool mute)
    {
        var command = CommandFactory.createCommand(
            CommandType.GROUP_SET_MUTE,
            new Params.GroupSetMute { Id = id, Mute = mute }
        );
        if (command == null)
            throw new Exception("Failed to create Group.SetMute command");

        var tcs = new TaskCompletionSource<GroupMuteSet>();
        Execute(
            command,
            new ResponseHandler<GroupMuteSet>(
                tcs.SetResult,
                e => tcs.SetException(new CommandException(e))
            )
        );
        await tcs.Task;
    }

    /// <summary>
    /// Sets the stream for a group.
    /// </summary>
    /// <param name="id">The ID of the group.</param>
    /// <param name="streamId">The ID of the stream to set.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GroupSetStreamAsync(string id, string streamId)
    {
        var command = CommandFactory.createCommand(
            CommandType.GROUP_SET_STREAM,
            new Params.GroupSetStream { Id = id, StreamId = streamId }
        );
        if (command == null)
            throw new Exception("Failed to create Group.SetStream command");

        var tcs = new TaskCompletionSource<GroupStreamSet>();
        Execute(
            command,
            new ResponseHandler<GroupStreamSet>(
                tcs.SetResult,
                e => tcs.SetException(new CommandException(e))
            )
        );
        await tcs.Task;
    }

    /// <summary>
    /// Sets the clients of a group.
    /// </summary>
    /// <param name="id">The ID of the group.</param>
    /// <param name="clients">The list of client IDs to set.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GroupSetClientsAsync(string id, List<string> clients)
    {
        var command = CommandFactory.createCommand(
            CommandType.GROUP_SET_CLIENTS,
            new Params.GroupSetClients { Id = id, Clients = clients }
        );
        if (command == null)
            throw new Exception("Failed to create Group.SetClients command");

        var tcs = new TaskCompletionSource<GroupClientsSet>();
        Execute(
            command,
            new ResponseHandler<GroupClientsSet>(
                tcs.SetResult,
                e => tcs.SetException(new CommandException(e))
            )
        );
        await tcs.Task;
    }

    /// <summary>
    /// Sets the name of a group.
    /// </summary>
    /// <param name="id">The ID of the group.</param>
    /// <param name="name">The name to set for the group.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GroupSetNameAsync(string id, string name)
    {
        var command = CommandFactory.createCommand(
            CommandType.GROUP_SET_NAME,
            new Params.GroupSetName { Id = id, Name = name }
        );
        if (command == null)
            throw new Exception("Failed to create Group.SetName command");

        var tcs = new TaskCompletionSource<GroupNameSet>();
        Execute(
            command,
            new ResponseHandler<GroupNameSet>(
                tcs.SetResult,
                e => tcs.SetException(new CommandException(e))
            )
        );
        await tcs.Task;
    }

    /// <summary>
    /// Retrieves the RPC version of the server.
    /// </summary>
    /// <returns>The RPC version of the server.</returns>
    public async Task<Models.RpcVersion> ServerGetRpcVersionAsync()
    {
        var command = CommandFactory.createCommand(
            CommandType.SERVER_GET_RPC_VERSION,
            new NullParams()
        );
        if (command == null)
            throw new Exception("Failed to create Server.GetRpcVersion command");

        var tcs = new TaskCompletionSource<RpcVersion>();
        Execute(
            command,
            new ResponseHandler<RpcVersion>(
                tcs.SetResult,
                e => tcs.SetException(new CommandException(e))
            )
        );
        return await tcs.Task;
    }

    /// <summary>
    /// Retrieves the status of the server.
    /// </summary>
    /// <returns>The status of the server.</returns>
    public async Task<Models.Server> ServerGetStatusAsync()
    {
        var command = CommandFactory.createCommand(CommandType.SERVER_GET_STATUS, new NullParams());
        if (command == null)
            throw new Exception("Failed to create Server.GetStatus command");

        var tcs = new TaskCompletionSource<ServerStatus>();
        Execute(command, new ResponseHandler<ServerStatus>(tcs.SetResult));

        var response = await tcs.Task;
        return response.Server;
    }

    /// <summary>
    /// Deletes a client from the server.
    /// </summary>
    /// <param name="id">The ID of the client to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ServerDeleteClientAsync(string id)
    {
        var command = CommandFactory.createCommand(
            CommandType.SERVER_DELETE_CLIENT,
            new Params.ServerDeleteClient { Id = id }
        );
        if (command == null)
            throw new Exception("Failed to create Server.DeleteClient command");

        var tcs = new TaskCompletionSource<DeleteClient>();
        Execute(command, new ResponseHandler<DeleteClient>(tcs.SetResult));
        await tcs.Task;
    }

    /// <summary>
    /// Adds a stream to the server.
    /// </summary>
    /// <param name="streamUri">The URI of the stream to add.</param>
    /// <returns>The ID of the added stream.</returns>
    public async Task<string> StreamAddStreamAsync(string streamUri)
    {
        var command = CommandFactory.createCommand(
            CommandType.STREAM_ADD_STREAM,
            new Params.StreamAddStream { StreamUri = streamUri }
        );
        if (command == null)
            throw new Exception("Failed to create Stream.AddStream command");

        var tcs = new TaskCompletionSource<AddRemove>();
        Execute(command, new ResponseHandler<AddRemove>(tcs.SetResult));

        var response = await tcs.Task;
        return response.StreamId;
    }

    /// <summary>
    /// Removes a stream from the server.
    /// </summary>
    /// <param name="id">The ID of the stream to remove.</param>
    /// <returns>The ID of the removed stream.</returns>
    public async Task<string> StreamRemoveStreamAsync(string id)
    {
        var command = CommandFactory.createCommand(
            CommandType.STREAM_REMOVE_STREAM,
            new Params.StreamRemoveStream { Id = id }
        );
        if (command == null)
            throw new Exception("Failed to create Stream.RemoveStream command");

        var tcs = new TaskCompletionSource<AddRemove>();
        Execute(command, new ResponseHandler<AddRemove>(tcs.SetResult));

        var response = await tcs.Task;
        return response.StreamId;
    }

    /// <summary>
    /// Controls a stream (play, pause, next, previous, seek, etc.).
    /// </summary>
    /// <param name="id">The ID of the stream to control.</param>
    /// <param name="command">The control command to execute.</param>
    /// <param name="parameters">Optional parameters for the command.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StreamControlAsync(
        string id,
        string command,
        Dictionary<string, object>? parameters = null
    )
    {
        var commandObj = CommandFactory.createCommand(
            CommandType.STREAM_CONTROL,
            new Params.StreamControl
            {
                Id = id,
                Command = command,
                Params = parameters,
            }
        );
        if (commandObj == null)
            throw new Exception("Failed to create Stream.Control command");

        var tcs = new TaskCompletionSource<string>();
        Execute(
            commandObj,
            new ResponseHandler<string>(
                tcs.SetResult,
                e => tcs.SetException(new CommandException(e))
            )
        );
        await tcs.Task;
    }

    /// <summary>
    /// Sets a property of a stream.
    /// </summary>
    /// <param name="id">The ID of the stream.</param>
    /// <param name="property">The property name to set.</param>
    /// <param name="value">The value to set for the property.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StreamSetPropertyAsync(string id, string property, object value)
    {
        var command = CommandFactory.createCommand(
            CommandType.STREAM_SET_PROPERTY,
            new Params.StreamSetProperty
            {
                Id = id,
                Property = property,
                Value = value,
            }
        );
        if (command == null)
            throw new Exception("Failed to create Stream.SetProperty command");

        var tcs = new TaskCompletionSource<string>();
        Execute(
            command,
            new ResponseHandler<string>(
                tcs.SetResult,
                e => tcs.SetException(new CommandException(e))
            )
        );
        await tcs.Task;
    }

    // Convenience methods for common Stream.Control commands

    /// <summary>
    /// Plays a stream.
    /// </summary>
    /// <param name="streamId">The ID of the stream to play.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StreamPlayAsync(string streamId)
    {
        await StreamControlAsync(streamId, "play");
    }

    /// <summary>
    /// Pauses a stream.
    /// </summary>
    /// <param name="streamId">The ID of the stream to pause.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StreamPauseAsync(string streamId)
    {
        await StreamControlAsync(streamId, "pause");
    }

    /// <summary>
    /// Skips to the next track in a stream.
    /// </summary>
    /// <param name="streamId">The ID of the stream.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StreamNextAsync(string streamId)
    {
        await StreamControlAsync(streamId, "next");
    }

    /// <summary>
    /// Skips to the previous track in a stream.
    /// </summary>
    /// <param name="streamId">The ID of the stream.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StreamPreviousAsync(string streamId)
    {
        await StreamControlAsync(streamId, "previous");
    }

    /// <summary>
    /// Seeks to a specific position in a stream.
    /// </summary>
    /// <param name="streamId">The ID of the stream.</param>
    /// <param name="position">The position to seek to (in seconds).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StreamSeekAsync(string streamId, double position)
    {
        var parameters = new Dictionary<string, object> { { "position", position } };
        await StreamControlAsync(streamId, "setPosition", parameters);
    }

    /// <summary>
    /// Seeks by an offset in a stream.
    /// </summary>
    /// <param name="streamId">The ID of the stream.</param>
    /// <param name="offset">The offset to seek by (in seconds).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StreamSeekByOffsetAsync(string streamId, double offset)
    {
        var parameters = new Dictionary<string, object> { { "offset", offset } };
        await StreamControlAsync(streamId, "seek", parameters);
    }

    // Convenience methods for common Stream.SetProperty commands

    /// <summary>
    /// Sets the volume of a stream.
    /// </summary>
    /// <param name="streamId">The ID of the stream.</param>
    /// <param name="volume">The volume level (0-100).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StreamSetVolumeAsync(string streamId, int volume)
    {
        await StreamSetPropertyAsync(streamId, "volume", volume);
    }

    /// <summary>
    /// Sets the mute state of a stream.
    /// </summary>
    /// <param name="streamId">The ID of the stream.</param>
    /// <param name="mute">Whether to mute the stream.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StreamSetMuteAsync(string streamId, bool mute)
    {
        await StreamSetPropertyAsync(streamId, "mute", mute);
    }

    /// <summary>
    /// Sets the shuffle mode of a stream.
    /// </summary>
    /// <param name="streamId">The ID of the stream.</param>
    /// <param name="shuffle">Whether to enable shuffle mode.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StreamSetShuffleAsync(string streamId, bool shuffle)
    {
        await StreamSetPropertyAsync(streamId, "shuffle", shuffle);
    }

    /// <summary>
    /// Sets the loop status of a stream.
    /// </summary>
    /// <param name="streamId">The ID of the stream.</param>
    /// <param name="loopStatus">The loop status ("none", "track", or "playlist").</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StreamSetLoopStatusAsync(string streamId, string loopStatus)
    {
        await StreamSetPropertyAsync(streamId, "loopStatus", loopStatus);
    }

    /// <summary>
    /// Sets the playback rate of a stream.
    /// </summary>
    /// <param name="streamId">The ID of the stream.</param>
    /// <param name="rate">The playback rate (1.0 = normal speed).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StreamSetRateAsync(string streamId, double rate)
    {
        await StreamSetPropertyAsync(streamId, "rate", rate);
    }
}
