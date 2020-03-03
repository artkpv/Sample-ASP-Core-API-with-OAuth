using System;
using System.Runtime.Serialization;

namespace BA.WebAPI
{
    public class BAException : Exception
    {
        public BAException()
        {
        }

        public BAException(string message) : base(message)
        {
        }

        public BAException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BAException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
