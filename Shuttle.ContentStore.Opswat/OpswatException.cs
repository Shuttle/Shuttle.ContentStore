using System;

namespace Shuttle.ContentStore.Opswat
{
    public class OpswatException : Exception
    {
        public OpswatException(string message) : base(message)
        {
        }

        public OpswatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}