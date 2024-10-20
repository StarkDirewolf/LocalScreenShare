namespace LocalScreenShare.Services;
public interface ISdpStore
{
    void Add(string sdp);

    string Get();
}
