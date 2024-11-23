namespace LocalScreenShare.Services;

/// <summary>
/// A service to store SDP information for WebRTC connections.
/// </summary>
public interface ISdpStore
{
    void Add(string sdp);

    string Get();

    void AddHostCandidate(string candidate);

    void AddClientCandidate(string candidate);

    string GetHostCandidate();

    string GetClientCandidate();
}
