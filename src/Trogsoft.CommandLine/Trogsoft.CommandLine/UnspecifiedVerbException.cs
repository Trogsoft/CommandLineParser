using System;
using System.Runtime.Serialization;

namespace Trogsoft.CommandLine
{
    [Serializable]
    internal class UnspecifiedVerbException : Exception
    {
        public UnspecifiedVerbException(string verb) : base($"The verb {verb} could not be found.")
        {
            HResult = ParserErrorCodes.ERR_INVALID_VERB;
        }

        public UnspecifiedVerbException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnspecifiedVerbException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}