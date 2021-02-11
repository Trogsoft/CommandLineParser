using System;
using System.Runtime.Serialization;

namespace Trogsoft.CommandLine
{
    [Serializable]
    internal class InvalidParameterException : Exception
    {

        public InvalidParameterException()
        {
        }

        public InvalidParameterException(ParameterAttribute paraConfig)
        {
            ParameterInfo = paraConfig;
        }

        public InvalidParameterException(string message) : base(message)
        {
        }

        public InvalidParameterException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidParameterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ParameterAttribute ParameterInfo { get; }
    }
}