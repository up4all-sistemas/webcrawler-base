using System.Runtime.Serialization;

namespace Up4All.WebCrawler.Framework.Handlers.Exception
{
    public class MissingRequiredDataException : System.Exception
    {
        public MissingRequiredDataException()
        {
        }

        public MissingRequiredDataException(string message) : base(message)
        {
        }

        public MissingRequiredDataException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected MissingRequiredDataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
