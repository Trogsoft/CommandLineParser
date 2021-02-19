using System;
using System.Runtime.Serialization;

namespace Trogsoft.CommandLine
{
    [Serializable]
    internal class InvalidParameterException : Exception
    {

        public InvalidParameterException(ParameterAttribute paraConfig) : base($"The parameter {paraConfig.LongName ?? paraConfig.ShortName.ToString()} contains an invalid value.")
        {
            ParameterInfo = paraConfig;
            HResult = ParserErrorCodes.ERR_INVALID_PARAMETER;
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