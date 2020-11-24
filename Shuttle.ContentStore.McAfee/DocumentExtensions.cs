using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.McAfee
{
    public static class DocumentExtensions
    {
        public static string GetSubId(this Document document)
        {
            Guard.AgainstNull(document, nameof(document));

            return document.GetPropertyValue("subId");
        }

        public static string GetStatus(this Document document)
        {
            Guard.AgainstNull(document, nameof(document));

            return document.GetPropertyValue("status");
        }

        public static string GetSeverity(this Document document)
        {
            Guard.AgainstNull(document, nameof(document));

            return document.GetPropertyValue("severity");
        }
    }
}