using LocalScreenShare.Client.Constants;
using LocalScreenShare.Services;
using Microsoft.AspNetCore.SignalR;

namespace LocalScreenShare.Hubs;

/// <summary>
/// A SignalR hub for handling WebRTC connections as a signaling server.
/// </summary>
public class SignalHub : Hub
{
    private ISdpStore _sdpStore;

    public SignalHub(ISdpStore sdpStore)
    {
        _sdpStore = sdpStore;
    }

    public void StoreSdpJson(string sdpJson)
    {
        _sdpStore.Add(sdpJson);
    }

    public Task GetSdps()
    {
        var sdps = _sdpStore.Get();

        if (sdps != null && sdps.Length > 0)
        {
            return Clients.Caller.SendAsync(CSMethod.StreamPage.ReceiveSignal, _sdpStore.Get());
        }

        return Task.CompletedTask;
    }

    public void StoreHostCandidate(string candidate)
    {
        _sdpStore.AddHostCandidate(candidate);
    }
    public void StoreClientCandidate(string candidate)
    {
        _sdpStore.AddClientCandidate(candidate);
    }

    public Task GetHostCandidate()
    {
        var hostCandidate = _sdpStore.GetHostCandidate();
        if (hostCandidate != null)
            return Clients.Caller.SendAsync(CSMethod.StreamPage.ReceiveSignal, _sdpStore.GetHostCandidate());
        else return Task.CompletedTask;
    }

    public Task ReturnClientCandidate(string candidate)
    {
        return Clients.Others.SendAsync(CSMethod.StreamPage.ReceiveSignal, candidate);
    }

    public Task ReturnAnswer(string sdpAnswerJson)
    {
        return Clients.Others.SendAsync(CSMethod.StreamPage.ReceiveSignal, sdpAnswerJson);
    }
}