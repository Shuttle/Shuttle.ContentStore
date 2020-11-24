using System;

namespace Shuttle.ContentStore.McAfee
{
    public interface IMcAfeeConfiguration
    {
        string Username { get; }
        string Password { get; }
        string Url { get; }
        TimeSpan SessionDuration { get; }
        TimeSpan PollScanInterval { get; }
        TimeSpan HeartbeatInterval { get; }
        string AnalyzerProfile { get; }
        int AcceptableSeverity { get; }
    }
}