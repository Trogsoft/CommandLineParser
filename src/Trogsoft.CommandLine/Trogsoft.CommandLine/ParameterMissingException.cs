using System;
using System.Runtime.Serialization;

namespace Trogsoft.CommandLine
{
    [Serializable]
    internal class ParameterMissingException : Exception
    {
        public ParameterMissingException(ParameterAttribute paraConfig)
        {
            ParameterInfo = paraConfig;
        }

        public ParameterMissingException(string message) : base(message)
        {
        }

        public ParameterMissingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParameterMissingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ParameterAttribute ParameterInfo { get; }
    }
}