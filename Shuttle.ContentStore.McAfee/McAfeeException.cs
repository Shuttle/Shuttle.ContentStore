using System;

namespace Shuttle.ContentStore.McAfee
{
    public class McAfeeException : Exception
    {
        public McAfeeException(string message) : base(message)
        {
        }

        public McAfeeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}