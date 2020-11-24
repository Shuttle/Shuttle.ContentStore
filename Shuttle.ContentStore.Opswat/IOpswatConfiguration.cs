using System;

namespace Shuttle.ContentStore.Opswat
{
    public interface IOpswatConfiguration
    {
        string ApiKey { get; }
        string ApiUrl { get; }
        string SandboxFileExtensions { get; }
        TimeSpan PollScanInterval { get; }
        TimeSpan PollSandboxInterval { get; }
        bool ShouldSandbox(string extension);
        bool SampleSharingAllowed { get; }
    }
}