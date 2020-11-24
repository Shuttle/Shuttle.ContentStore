using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.Opswat
{
    public static class DocumentExtensions
    {
        public static string GetDataId(this Document document)
        {
            Guard.AgainstNull(document, nameof(document));

            return document.GetPropertyValue("data_id");
        }

        public static string GetSandboxId(this Document document)
        {
            Guard.AgainstNull(document, nameof(document));

            return document.GetPropertyValue("sandbox_id");
        }

        public static string GetSha256(this Document document)
        {
            Guard.AgainstNull(document, nameof(document));

            return document.GetPropertyValue("sha256");
        }

        public static string GetScanResult(this Document document)
        {
            Guard.AgainstNull(document, nameof(document));

            return document.GetPropertyValue("scan_all_result_i");
        }

        public static string GetProgressPercentage(this Document document)
        {
            Guard.AgainstNull(document, nameof(document));

            return document.GetPropertyValue("progress_percentage");
        }
    }
}