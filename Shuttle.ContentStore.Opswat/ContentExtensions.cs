using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.Opswat
{
    public static class ContentExtensions
    {
        public static string GetDataId(this Content content)
        {
            Guard.AgainstNull(content, nameof(content));

            return content.GetPropertyValue("data_id");
        }

        public static string GetSandboxId(this Content content)
        {
            Guard.AgainstNull(content, nameof(content));

            return content.GetPropertyValue("sandbox_id");
        }

        public static string GetSha256(this Content content)
        {
            Guard.AgainstNull(content, nameof(content));

            return content.GetPropertyValue("sha256");
        }

        public static string GetScanResult(this Content content)
        {
            Guard.AgainstNull(content, nameof(content));

            return content.GetPropertyValue("scan_all_result_i");
        }

        public static string GetProgressPercentage(this Content content)
        {
            Guard.AgainstNull(content, nameof(content));

            return content.GetPropertyValue("progress_percentage");
        }
    }
}