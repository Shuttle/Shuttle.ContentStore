using System;
using System.Configuration;
using Shuttle.Core.Configuration;
using Shuttle.Core.Logging;

namespace Shuttle.ContentStore.McAfee
{
    public class McAfeeSection : ConfigurationSection
    {
        [ConfigurationProperty("username", IsRequired = true)]
        public string Username => (string) this["username"];

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password => (string) this["password"];

        [ConfigurationProperty("url", IsRequired = true)]
        public string Url => (string) this["url"];

        [ConfigurationProperty("analyzerProfile", IsRequired = true)]
        public string AnalyzerProfile => (string) this["analyzerProfile"];

        [ConfigurationProperty("acceptableSeverity", IsRequired = false, DefaultValue = -1)]
        public int AcceptableSeverity => (int) this["acceptableSeverity"];

        [ConfigurationProperty("sessionDuration", IsRequired = false, DefaultValue = "01:00:00")]
        public string SessionDuration => (string) this["sessionDuration"];

        [ConfigurationProperty("heartbeatInterval", IsRequired = false, DefaultValue = "00:00:05")]
        public string HeartbeatInterval => (string) this["heartbeatInterval"];

        [ConfigurationProperty("pollScanInterval", IsRequired = false, DefaultValue = "00:00:05")]
        public string PollScanInterval => (string) this["pollScanInterval"];

        public static IMcAfeeConfiguration Configuration()
        {
            var section = ConfigurationSectionProvider.Open<McAfeeSection>("shuttle", "mcafee");

            return Configuration(section);
        }

        private static IMcAfeeConfiguration Configuration(McAfeeSection section)
        {
            if (section == null)
            {
                throw new ConfigurationErrorsException("Could not find a 'mcafee' configuration section.");
            }

            if (!TimeSpan.TryParse(section.SessionDuration, out var sessionDuration))
            {
                sessionDuration = TimeSpan.FromHours(1);
            }

            if (!TimeSpan.TryParse(section.HeartbeatInterval, out var heartbeatInterval))
            {
                heartbeatInterval = TimeSpan.FromSeconds(5);
            }

            if (!TimeSpan.TryParse(section.PollScanInterval, out var pollScanInterval))
            {
                pollScanInterval = TimeSpan.FromSeconds(5);
            }

            var result = new McAfeeConfiguration(section.Url, section.Username, section.Password, sessionDuration, section.AnalyzerProfile,
                section.AcceptableSeverity, heartbeatInterval, pollScanInterval);

            Log.Information($"[mcafee] : url = {result.Url}");
            Log.Information($"[mcafee] : username = {result.Username}");
            Log.Information($"[mcafee] : session duration = {result.SessionDuration}");
            Log.Information($"[mcafee] : analyzer profile = {result.AnalyzerProfile}");
            Log.Information($"[mcafee] : acceptable severity = {result.AcceptableSeverity}");
            Log.Information($"[mcafee] : heartbeat interval = {result.HeartbeatInterval}");
            Log.Information($"[mcafee] : poll scan interval = {result.PollScanInterval}");

            return result;
        }

        public static IMcAfeeConfiguration Configuration(string path)
        {
            return Configuration(ConfigurationSectionProvider.OpenFile<McAfeeSection>("shuttle", "mcafee", path));
        }
    }
}