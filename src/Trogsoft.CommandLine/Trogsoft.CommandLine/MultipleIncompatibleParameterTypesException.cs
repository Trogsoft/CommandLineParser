using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Trogsoft.CommandLine
{
    [Serializable]
    internal class MultipleIncompatibleParameterTypesException : Exception
    {
        private MethodInfo method;

        public MultipleIncompatibleParameterTypesException(MethodInfo method) :  base($"Method '{method.Name}' on '{method.DeclaringType.Name}' declares multiple incompatible parameter types.  You cannot mix primitive and model parameters.")
        {
            this.method = method;
            this.HResult = ParserErrorCodes.ERR_MULTIPLE_PARAMETER_TYPES;
        }


        public MultipleIncompatibleParameterTypesException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MultipleIncompatibleParameterTypesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}