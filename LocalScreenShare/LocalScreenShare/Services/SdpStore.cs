namespace LocalScreenShare.Services;

public class SdpStore : ISdpStore
{
    private string _sdp;

    public void Add(string sdp)
        => _sdp = sdp;

    public string Get()
        => _sdp;
}

