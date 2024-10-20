using Microsoft.AspNetCore.SignalR.Client;

namespace LocalScreenShare.Client.Proxy
{
    public class HubConnectionProxy : IHubConnectionProxy
    {
        public required HubConnection HubConnection { get; init; }

        public HubConnectionState State
            => HubConnection.State;

        public ValueTask DisposeAsync()
            => HubConnection.DisposeAsync();

        public IDisposable On<T1, T2>(string methodName, Action<T1, T2> handler)
            => HubConnection.On(methodName, handler);

        public IDisposable On<T1>(string methodName, Action<T1> handler)
            => HubConnection.On(methodName, handler);

        public Task SendAsync(string methodName, object? arg1, object? arg2, CancellationToken cancellationToken = default)
            => HubConnection.SendAsync(methodName, arg1, arg2, cancellationToken);

        public Task SendAsync(string methodName, CancellationToken cancellationToken = default)
            => HubConnection.SendAsync(methodName, cancellationToken);

        public Task SendAsync(string methodName, object? arg1, CancellationToken cancellationToken = default)
            => HubConnection.SendAsync(methodName, arg1, cancellationToken);

        public Task StartAsync(CancellationToken cancellationToken = default)
            => HubConnection.StartAsync(cancellationToken);
    }
}
