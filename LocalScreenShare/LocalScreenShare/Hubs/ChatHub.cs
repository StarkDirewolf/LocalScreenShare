using LocalScreenShare.Services;
using Microsoft.AspNetCore.SignalR;
using System.Drawing;

namespace LocalScreenShare.Hubs;

public class ChatHub : Hub
{
    private ISdpStore _sdpStore;

    public ChatHub(ISdpStore sdpStore)
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
            return Clients.Caller.SendAsync("ReceiveAllSdpJsons", _sdpStore.Get());
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
        // Some kind of error going on
    }

    public Task GetHostCandidate()
    {
        var hostCandidate = _sdpStore.GetHostCandidate();
        if (hostCandidate != null)
            return Clients.Caller.SendAsync("ReceiveCandidateJson", _sdpStore.GetHostCandidate());
        else return Task.CompletedTask;
    }

    public Task ReturnClientCandidate(string candidate)
    {
        return Clients.Others.SendAsync("ReceiveCandidateJson", candidate);
    }

    public Task ReturnAnswer(string sdpAnswerJson)
    {
        return Clients.Others.SendAsync("ReceiveSdpAnswerJson", sdpAnswerJson);
    }
}