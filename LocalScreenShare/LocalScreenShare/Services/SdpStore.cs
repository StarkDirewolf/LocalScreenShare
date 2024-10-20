namespace LocalScreenShare.Services;

/// <inheritdoc cref="ISdpStore"/>
public class SdpStore : ISdpStore
{
	private string? _sdp;
	private string? _hostCandidate;
	private string? _clientCandidate;

	public void Add(string sdp)
		=> _sdp = sdp;

	public string? Get()
		=> _sdp;

	public void AddHostCandidate(string candidate)
		=> _hostCandidate = candidate;

	public void AddClientCandidate(string candidate)
		=> _clientCandidate = candidate;

	public string? GetHostCandidate()
		=> _hostCandidate;

	public string? GetClientCandidate()
		=> _clientCandidate;
}

