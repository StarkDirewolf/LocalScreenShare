using LocalScreenShare.Client.Constants;
using LocalScreenShare.Client.Proxy;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace LocalScreenShare.Client.Pages;

/// <summary>
/// Code-behind for Stream Page.
/// </summary>
public sealed partial class StreamPage
{
    internal static IJSObjectReference module = null!;

    internal static IHubConnectionProxy? hubConnectionProxy;

    public bool IsConnected =>
        hubConnectionProxy?.State == HubConnectionState.Connected;

    [Inject]
    internal IJSRuntime JSRuntime { get; set; } = null!;

    [JSInvokable]
    public static async Task ReceiveLocalSdpAnswerAsync(string sdpAnswerJson)
    {
        await hubConnectionProxy?.SendAsync(CSMethod.SignalHub.ReturnAnswer, sdpAnswerJson)!;
    }

    [JSInvokable]
    public static async Task ReceiveHostCandidateAsync(string candidateJson)
    {
        await hubConnectionProxy?.SendAsync(CSMethod.SignalHub.StoreHostCandidate, candidateJson)!;
    }

    [JSInvokable]
    public static async Task ReceiveClientCandidateAsync(string candidateJson)
    {
        await hubConnectionProxy?.SendAsync(CSMethod.SignalHub.StoreClientCandidate, candidateJson)!;
    }

    public async ValueTask DisposeAsync()
    {
        if (hubConnectionProxy is not null)
        {
            await hubConnectionProxy.DisposeAsync();
        }

        if (module is not null)
        {
            await module.DisposeAsync();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (hubConnectionProxy == null)
        {
            hubConnectionProxy = new HubConnectionProxy
            {
                HubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri(CSMethod.SignalHub.Navigation))
            .Build()
            };

            hubConnectionProxy.On<string>(CSMethod.StreamPage.ReceiveSignal, async signal =>
            {
                if (!string.IsNullOrEmpty(signal))
                {
                    await module.InvokeVoidAsync(JSMethod.Stream.ReceiveSignal, signal);
                }
            });

            await hubConnectionProxy.StartAsync();
        }
    }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", JSMethod.Stream.Filename);

            hubConnectionProxy?.SendAsync(CSMethod.SignalHub.GetSdps);
            hubConnectionProxy?.SendAsync(CSMethod.SignalHub.GetHostCandidate);
        }
    }

    /// <summary>
    /// Called by the Start button, triggering screen capture and WebRTC offer creation.
    /// </summary>
    /// <returns>A task of this asynchronous action.</returns>
    internal async Task Start()
    {
        var sdpJson = await module.InvokeAsync<string>(JSMethod.Stream.CaptureScreen);
        await hubConnectionProxy?.SendAsync(CSMethod.SignalHub.StoreSdp, sdpJson)!;
    }
}