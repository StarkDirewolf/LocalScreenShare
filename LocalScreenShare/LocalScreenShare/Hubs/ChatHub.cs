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
        return Clients.Caller.SendAsync("ReceiveAllSdpJsons", _sdpStore.Get());
    }

    public Task ReceiveLocalAnswer(string sdpAnswerJson)
    {
        return Clients.Others.SendAsync("ReceiveSdpAnswerJson", sdpAnswerJson);
    }
}