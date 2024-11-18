namespace LocalScreenShare.Services;
public interface ISdpStore
{
    void Add(string sdp);

    string Get();

    void AddHostCandidate(string candidate);

    void AddClientCandidate(string candidate);

    string GetHostCandidate();

    string GetClientCandidate();
}
