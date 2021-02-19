using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Trogsoft.CommandLine
{
    [Serializable]
    internal class MultipleModelParametersException : Exception
    {
        private MethodInfo method;


        public MultipleModelParametersException(MethodInfo method) : base($"Operation '{method.Name}' on '{method.DeclaringType.Name}' defines multiple model parameters.  This is unsupported.")
        {
            this.method = method;
            this.HResult = ParserErrorCodes.MULTIPLE_MODEL_PARAMETERS;
        }

        public MultipleModelParametersException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MultipleModelParametersException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}