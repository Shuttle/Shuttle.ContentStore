using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.McAfee
{
    public static class ContentExtensions
    {
        public static string GetSubId(this Content content)
        {
            Guard.AgainstNull(content, nameof(content));

            return content.GetPropertyValue("subId");
        }

        public static string GetStatus(this Content content)
        {
            Guard.AgainstNull(content, nameof(content));

            return content.GetPropertyValue("status");
        }

        public static string GetSeverity(this Content content)
        {
            Guard.AgainstNull(content, nameof(content));

            return content.GetPropertyValue("severity");
        }
    }
}