using System;
using System.Runtime.Serialization;

namespace Up4All.WebCrawler.Framework.Services
{
    [Serializable]
    public class UnavailableSourceException : Exception
    {
        public UnavailableSourceException()
        {
        }

        public UnavailableSourceException(string message) : base(message)
        {
        }

        public UnavailableSourceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnavailableSourceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}