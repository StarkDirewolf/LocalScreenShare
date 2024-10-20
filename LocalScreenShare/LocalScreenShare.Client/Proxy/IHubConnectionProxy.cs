using Microsoft.AspNetCore.SignalR.Client;

namespace LocalScreenShare.Client.Proxy
{
    public interface IHubConnectionProxy
    {
        /// <inheritdoc cref="HubConnection.State" />
        public HubConnectionState State { get; }

        /// <inheritdoc cref="HubConnection.DisposeAsync" />
        public ValueTask DisposeAsync();

        /// <inheritdoc cref="HubConnection.StartAsync(CancellationToken)" />
        public Task StartAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc cref="HubConnectionExtensions.SendAsync(HubConnection, string, CancellationToken)" />
        public Task SendAsync(string methodName, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="HubConnectionExtensions.SendAsync(HubConnection, string, object?, CancellationToken)" />
        public Task SendAsync(string methodName, object? arg1, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="HubConnectionExtensions.SendAsync(HubConnection, string, object?, object?, CancellationToken)" />
        public Task SendAsync(string methodName, object? arg1, object? arg2, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="HubConnectionExtensions.On{T1}(HubConnection, string, Action{T1})" />
        public IDisposable On<T1>(string methodName, Action<T1> handler);

        /// <inheritdoc cref="HubConnectionExtensions.On{T1, T2}(HubConnection, string, Action{T1, T2})" />
        public IDisposable On<T1, T2>(string methodName, Action<T1, T2> handler);
    }
}
