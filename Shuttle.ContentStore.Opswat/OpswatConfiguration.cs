using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.Opswat
{
    public class OpswatConfiguration : IOpswatConfiguration
    {
        private readonly List<string> _sandboxFileExtensions;

        public OpswatConfiguration(string apiUrl, string apiKey, string sandboxFileExtensions)
        {
            Guard.AgainstNullOrEmptyString(apiUrl, nameof(apiUrl));
            Guard.AgainstNullOrEmptyString(apiKey, nameof(apiKey));

            ApiUrl = apiUrl;
            ApiKey = apiKey;
            SandboxFileExtensions = sandboxFileExtensions ?? string.Empty;
            PollSandboxInterval = TimeSpan.FromSeconds(30);
            PollScanInterval = TimeSpan.FromSeconds(5);
            SampleSharingAllowed = false;

            _sandboxFileExtensions = string.IsNullOrWhiteSpace(sandboxFileExtensions)
                ? new List<string>()
                : sandboxFileExtensions
                    .Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(item =>
                    {
                        var extension = item.ToLowerInvariant();

                        return extension.StartsWith(".")
                            ? extension
                            : $".{extension}";
                    })
                    .ToList();
        }

        public string ApiKey { get; }
        public string ApiUrl { get; }
        public string SandboxFileExtensions { get; }
        public TimeSpan PollScanInterval { get; }
        public TimeSpan PollSandboxInterval { get; }

        public bool ShouldSandbox(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return false;
            }

            var value = extension.StartsWith(".")
                ? extension
                : $".{extension}";

            return _sandboxFileExtensions.Contains(value.ToLowerInvariant());
        }

        public bool SampleSharingAllowed { get; private set; }

        public void AllowSampleSharing()
        {
            SampleSharingAllowed = true;
        }
    }
}