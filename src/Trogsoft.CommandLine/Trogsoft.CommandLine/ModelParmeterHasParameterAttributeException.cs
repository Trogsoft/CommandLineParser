using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Trogsoft.CommandLine
{
    [Serializable]
    internal class ModelParmeterHasParameterAttributeException : Exception
    {
        private ParameterInfo para;

        public ModelParmeterHasParameterAttributeException(ParameterInfo para) : base($"Model parameter {para.Name} should have parameter attributes on the method.")
        {
            this.para = para;
            HResult = ParserErrorCodes.ERR_MODEL_TYPE_HAS_PARAMETER_ATTRIBUTE;
        }

        public ModelParmeterHasParameterAttributeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ModelParmeterHasParameterAttributeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}