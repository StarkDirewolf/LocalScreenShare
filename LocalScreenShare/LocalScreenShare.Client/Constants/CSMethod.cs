namespace LocalScreenShare.Client.Constants;

/// <summary>
/// A static class containing constants for C# methods.
/// </summary>
public static class CSMethod
{
    /// <summary>
    /// Constants for the Stream Page.
    /// </summary>
    public static class StreamPage
    {
        public const string ReceiveSignal = "ReceiveSignal";
    }

    /// <summary>
    /// Constants for the Signal Hub.
    /// </summary>
    public static class SignalHub
    {
        public const string Navigation = "/signalhub";
        public const string GetSdps = "GetSdps";
        public const string GetHostCandidate = "GetHostCandidate";
        public const string StoreSdp = "StoreSdpJson";
        public const string ReturnAnswer = "ReturnAnswer";
        public const string StoreHostCandidate = "StoreHostCandidate";
        public const string StoreClientCandidate = "StoreClientCandidate";
    }
}
