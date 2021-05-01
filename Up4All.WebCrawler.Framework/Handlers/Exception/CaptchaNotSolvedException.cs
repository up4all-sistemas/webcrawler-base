using System;
using System.Runtime.Serialization;

namespace Up4All.WebCrawler.Framework.Handlers.Exception
{
    public class CaptchaNotSolvedException : System.Exception
    {
        public CaptchaNotSolvedException()
        {
        }

        public CaptchaNotSolvedException(string message) : base(message)
        {
        }

        public CaptchaNotSolvedException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected CaptchaNotSolvedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}