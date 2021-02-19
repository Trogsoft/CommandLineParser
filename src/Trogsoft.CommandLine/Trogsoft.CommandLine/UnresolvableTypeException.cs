using System;
using System.Runtime.Serialization;

namespace Trogsoft.CommandLine
{
    [Serializable]
    internal class UnresolvableTypeException : Exception
    {
        public UnresolvableTypeException()
        {
        }

        public UnresolvableTypeException(string message) : base(message)
        {
        }

        public UnresolvableTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnresolvableTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}