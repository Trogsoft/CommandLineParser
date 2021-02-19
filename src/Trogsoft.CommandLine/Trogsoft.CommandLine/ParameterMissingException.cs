using System;
using System.Runtime.Serialization;

namespace Trogsoft.CommandLine
{
    [Serializable]
    internal class ParameterMissingException : Exception
    {
        public ParameterMissingException(ParameterAttribute paraConfig) : base($"The required parameter {paraConfig.LongName ?? paraConfig.ShortName.ToString()} is missing.")
        {
            HResult = ParserErrorCodes.ERR_PARAMETER_MISSING;
            ParameterInfo = paraConfig;
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