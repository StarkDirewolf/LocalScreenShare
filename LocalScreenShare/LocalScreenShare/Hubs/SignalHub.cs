using LocalScreenShare.Client.Constants;
using LocalScreenShare.Services;
using Microsoft.AspNetCore.SignalR;

namespace LocalScreenShare.Hubs;

/// <summary>
/// A SignalR hub for handling WebRTC connections as a signaling server.
/// </summary>
public class SignalHub(ISdpStore _sdpStore) : Hub
{
	public void StoreSdpJson(string sdpJson)
		=> _sdpStore.Add(sdpJson);

	public Task GetSdps()
	{
		var sdps = _sdpStore.Get();

		return sdps != null && sdps.Length > 0
			? Clients.Caller.SendAsync(CSMethod.StreamPage.ReceiveSignal, sdps)
			: Task.CompletedTask;
	}

	public void StoreHostCandidate(string candidate)
		=> _sdpStore.AddHostCandidate(candidate);

	public void StoreClientCandidate(string candidate)
		=> _sdpStore.AddClientCandidate(candidate);

	public Task GetHostCandidate()
	{
		var hostCandidate = _sdpStore.GetHostCandidate();

		return hostCandidate != null
			? Clients.Caller.SendAsync(CSMethod.StreamPage.ReceiveSignal, hostCandidate)
			: Task.CompletedTask;
	}

	public Task ReturnClientCandidate(string candidate)
		=> Clients.Others.SendAsync(CSMethod.StreamPage.ReceiveSignal, candidate);

	public Task ReturnAnswer(string sdpAnswerJson)
		=> Clients.Others.SendAsync(CSMethod.StreamPage.ReceiveSignal, sdpAnswerJson);
}