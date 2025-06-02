using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace WiseHR_Frontend.Services
{
    public interface IChatService : IAsyncDisposable
    {
        bool IsConnected { get; }
        event Action<string> OnChunkReceived;
        event Action<string> OnStatusReceived;
        event Action OnConnectionStateChanged;
        Task InitializeAsync();
        Task SendQueryAsync(string user, string message, string? userId);
    }

    public class ChatService : IChatService
    {
        private readonly NavigationManager _navigationManager;
        private HubConnection? _hubConnection;
        private bool _isConnected;
        private const string BACKEND_URL = "https://localhost:7021";
        private readonly SynchronizationContext? _synchronizationContext;

        public bool IsConnected => _isConnected;

        public event Action<string>? OnChunkReceived;
        public event Action<string>? OnStatusReceived;
        public event Action? OnConnectionStateChanged;
        public event Action<string, string>? OnMessageReceived;

        public ChatService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
            _synchronizationContext = SynchronizationContext.Current;
        }

        private void InvokeOnUIThread(Action action)
        {
            if (_synchronizationContext != null)
            {
                _synchronizationContext.Post(_ => action(), null);
            }
            else
            {
                action();
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                var hubUrl = $"{BACKEND_URL}/chathub";
                Console.WriteLine($"Connecting to chat hub at: {hubUrl}");
                
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl)
                    .WithAutomaticReconnect()
                    .Build();

                // Set up event handlers
                _hubConnection.On<string>("ReceiveChunk", chunk => 
                    InvokeOnUIThread(() => OnChunkReceived?.Invoke(chunk)));
                
                _hubConnection.On<string>("ReceiveStatus", status => 
                    InvokeOnUIThread(() => OnStatusReceived?.Invoke(status)));
                _hubConnection.On<string, string>("ReceiveMessage", (displayName, message) =>
          InvokeOnUIThread(() => OnMessageReceived?.Invoke(displayName, message)));

                _hubConnection.Reconnecting += _ =>
                {
                    InvokeOnUIThread(() =>
                    {
                        _isConnected = false;
                        OnConnectionStateChanged?.Invoke();
                    });
                    return Task.CompletedTask;
                };

                _hubConnection.Reconnected += _ =>
                {
                    InvokeOnUIThread(() =>
                    {
                        _isConnected = true;
                        OnConnectionStateChanged?.Invoke();
                    });
                    return Task.CompletedTask;
                };

                _hubConnection.Closed += _ =>
                {
                    InvokeOnUIThread(() =>
                    {
                        _isConnected = false;
                        OnConnectionStateChanged?.Invoke();
                    });
                    return Task.CompletedTask;
                };

                await _hubConnection.StartAsync();
                _isConnected = true;
                OnConnectionStateChanged?.Invoke();
                Console.WriteLine("Successfully connected to chat hub");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize chat service: {ex.Message}");
                _isConnected = false;
                OnConnectionStateChanged?.Invoke();
                throw;
            }
        }

        public async Task SendQueryAsync(string user, string message, string? userId)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
            {
                Console.WriteLine($"Chat service not connected. State: {_hubConnection?.State}");
                throw new InvalidOperationException("Chat service is not connected");
            }

            Console.WriteLine($"Sending query - User: {user}, Message: {message}, UserId: {userId}");
            try
            {
                await _hubConnection.SendAsync("SendQuery", user, message, userId);
                Console.WriteLine("Query sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending query: {ex.Message}");
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                if (_hubConnection.State == HubConnectionState.Connected)
                {
                    await _hubConnection.StopAsync();
                }
                await _hubConnection.DisposeAsync();
            }
        }
    }
}