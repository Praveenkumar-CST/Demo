using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace WiseHR.Services
{
    public interface IChatService : IAsyncDisposable
    {
        bool IsConnected { get; }
        event Action<string> OnChunkReceived;
        event Action<string> OnStatusReceived;
        event Action OnConnectionStateChanged;
        Task InitializeAsync();
        Task SendQueryAsync(string user, string message, string? jwtToken, string sessionId);
    }

    public class ChatService : IChatService
    {
        private readonly NavigationManager _navigationManager;
        private readonly HttpClient _httpClient;
        private HubConnection? _hubConnection;
        private bool _isConnected;
        private readonly SynchronizationContext? _synchronizationContext;
        private readonly IJSRuntime _jsRuntime;

        public bool IsConnected => _isConnected;

        public event Action<string>? OnChunkReceived;
        public event Action<string>? OnStatusReceived;
        public event Action? OnConnectionStateChanged;
        public event Action<string, string>? OnMessageReceived;

        public ChatService(NavigationManager navigationManager, HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _navigationManager = navigationManager;
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
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
                var hubUrl = $"{_httpClient.BaseAddress}chathub";
                Console.WriteLine($"Connecting to chat hub at: {hubUrl}");
                
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl)
                    .WithAutomaticReconnect()
                    .Build();

                // Set up event handlers
                _hubConnection.On<string>("ReceiveChunk", chunk => 
                {
                    Console.WriteLine($"Raw Chunk Received: {chunk}");
                    InvokeOnUIThread(() => OnChunkReceived?.Invoke(chunk));
                });
                
                _hubConnection.On<string>("ReceiveStatus", status => 
                {
                    Console.WriteLine($"Raw Status Received: {status}");
                    InvokeOnUIThread(() => OnStatusReceived?.Invoke(status));
                });
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

        public async Task SendQueryAsync(string user, string message, string? jwtToken, string sessionId)
        {
            if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
            {
                Console.WriteLine($"Chat service not connected. State: {_hubConnection?.State}");
                throw new InvalidOperationException("Chat service is not connected");
            }

            var fetchedToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (string.IsNullOrEmpty(fetchedToken))
            {
                Console.WriteLine("No auth token found in localStorage.");
                throw new InvalidOperationException("Authentication token not found.");
            }

            Console.WriteLine($"Sending query - User: {user}, Message: {message}, SessionId: {sessionId}");
            try
            {
                await _hubConnection.SendAsync("SendQuery", user, message, fetchedToken, sessionId);
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