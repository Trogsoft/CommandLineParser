using System;
using System.Runtime.Serialization;

namespace Trogsoft.CommandLine
{
    [Serializable]
    internal class UnspecifiedOperationException : Exception
    {
        public UnspecifiedOperationException(string verb, string operation) : base($"The operation {operation} could not be found on verb {verb}.")
        {
            HResult = ParserErrorCodes.ERR_INVALID_OPERATION;
        }

        public UnspecifiedOperationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnspecifiedOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}