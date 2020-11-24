using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.McAfee
{
    public class McAfeeConfiguration : IMcAfeeConfiguration
    {
        public McAfeeConfiguration(string url, string username, string password, TimeSpan sessionDuration,
            string analyzerProfile, int acceptableSeverity, TimeSpan heartbeatInterval, TimeSpan pollScanInterval)
        {
            Guard.AgainstNullOrEmptyString(url, nameof(url));
            Guard.AgainstNullOrEmptyString(username, nameof(username));
            Guard.AgainstNullOrEmptyString(password, nameof(password));
            Guard.AgainstNullOrEmptyString(analyzerProfile, nameof(analyzerProfile));

            Url = url;
            Username = username;
            Password = password;
            SessionDuration = sessionDuration;
            AnalyzerProfile = analyzerProfile;
            AcceptableSeverity = acceptableSeverity;
            PollScanInterval = pollScanInterval;
            HeartbeatInterval = heartbeatInterval;
        }

        public string Username { get; }
        public string Password { get; }
        public string Url { get; }
        public TimeSpan SessionDuration { get; }
        public TimeSpan PollScanInterval { get; }
        public TimeSpan HeartbeatInterval { get; }
        public string AnalyzerProfile { get; }
        public int AcceptableSeverity { get; }
    }
}