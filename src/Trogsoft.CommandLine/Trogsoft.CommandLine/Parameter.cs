using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Trogsoft.CommandLine
{
    internal class Parameter
    {
        public Type ParameterType { get; protected set; }
        public string DisplayName { get; set; }

        internal Parameter(System.Reflection.ParameterInfo para, ParameterAttribute paraConfig)
        {
            this.ParameterType = para.ParameterType;
        }

        internal static bool IsSimpleType(Type type)
        {
            var isListOfSimpleThings = false;
            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                if (type.IsGenericType)
                {
                    var firstGenericParameter = type.GetGenericArguments().First();
                    var isResolveableType = TypeResolver.IsResolvableType(firstGenericParameter);
                    isListOfSimpleThings = (firstGenericParameter.IsPrimitive || firstGenericParameter.IsEnum || firstGenericParameter == typeof(string) || isResolveableType);
                }
            }

            return (type.IsPrimitive || type.IsEnum || type == typeof(string) || isListOfSimpleThings);
        }

        internal static Parameter Create(ParameterInfo para, ParameterAttribute paraConfig)
        {
            if (IsSimpleType(para.ParameterType))
            {
                return new SimpleParameter(para, paraConfig);
            }
            else if (TypeResolver.IsResolvableType(para.ParameterType))
            {
                return new ResolvedParameter(para, paraConfig);
            }
            else
            {
                if (paraConfig != null)
                    throw new ModelParmeterHasParameterAttributeException(para);

                return new ModelParameter(para);
            }
        }
    }

    internal class SimpleParameter : Parameter
    {
        internal SimpleParameter(System.Reflection.ParameterInfo para, ParameterAttribute paraConfig) : base(para, paraConfig)
        {
            if (paraConfig == null)
                if (para.Name.Length == 1)
                    paraConfig = new ParameterAttribute { ShortName = (char)para.Name.First() };
                else
                    paraConfig = new ParameterAttribute { LongName = para.Name };

            if (para.HasDefaultValue)
            {
                paraConfig.IsRequired = false;
                paraConfig.Default = para.DefaultValue;
            }

            ParameterInfo = paraConfig;

        }
        public ParameterAttribute ParameterInfo { get; set; }
    }

    internal class ResolvedParameter : SimpleParameter
    {
        internal ResolvedParameter(System.Reflection.ParameterInfo para, ParameterAttribute paraConfig) : base(para, paraConfig)
        {
        }

    }

    internal class ModelParameter : Parameter
    {
        internal ModelParameter(System.Reflection.ParameterInfo para) : base(para, null)
        {
            this.ModelType = para.ParameterType;

            var model = Activator.CreateInstance(para.ParameterType);
            foreach (var prop in para.ParameterType.GetProperties().Where(x => x.GetSetMethod() != null))
            {
                bool isRequired = false;
                var currentValue = prop.GetValue(model);

                var paraConfig = prop.GetCustomAttribute<ParameterAttribute>();

                if (prop.PropertyType.IsValueType)
                    isRequired = currentValue.Equals(Activator.CreateInstance(prop.PropertyType));
                else
                    isRequired = currentValue == null;

                if (paraConfig == null)
                {
                    if (prop.Name.Length == 1)
                        paraConfig = new ParameterAttribute { ShortName = (char)prop.Name.First() };
                    else
                        paraConfig = new ParameterAttribute { LongName = prop.Name };
                }

                paraConfig.isSimpleType = false;
                paraConfig.Type = para.ParameterType;

                paraConfig.IsRequired = isRequired;
                if (!paraConfig.IsRequired)
                    paraConfig.Default = currentValue;

                Parameters.Add(paraConfig);
            }

        }
        public List<ParameterAttribute> Parameters { get; set; } = new List<ParameterAttribute>();
        public Type ModelType { get; }
    }

}
