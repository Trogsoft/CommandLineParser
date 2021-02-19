using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trogsoft.CommandLine
{
    internal static class TypeResolver
    {

        private static List<TypeResolverDefinition> typeResolvers = new List<TypeResolverDefinition>();

        static TypeResolver()
        {
            var typeHandlers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x =>
                x.GetTypes()
                .Where(y => typeof(ITypeResolver).IsAssignableFrom(y) && y.IsPublic && !y.IsAbstract && !y.IsInterface)
            ).ToList();

            typeHandlers.ToList().ForEach(x =>
            {
                var tr = new TypeResolverDefinition();
                tr.Converter = x;
                foreach (var i in x.GetInterfaces())
                {
                    if (i.Name.StartsWith(nameof(ITypeResolver)) && i.IsGenericType)
                    {
                        tr.DestinationType = i.GetGenericArguments().First();
                    }
                }
                typeResolvers.Add(tr);
            });
        }

        internal static bool IsResolvableType(Type t)
        {
            return typeResolvers.Any(x => x.DestinationType == t);
        }

        internal static int Count() => typeResolvers.Count;

        internal static object Resolve(string value, Type toType)
        {

            if (!IsResolvableType(toType))
                throw new UnresolvableTypeException();

            var resolver = typeResolvers.SingleOrDefault(x => x.DestinationType == toType);
            var resolverInstance = Activator.CreateInstance(resolver.Converter);
            var resolveMethod = resolverInstance.GetType().GetMethod("Resolve");

            var res = resolveMethod.Invoke(resolverInstance, new object[] { value });
            return res;

        }

        internal static TypeResolverDefinition GetResolverForType(Type type)
        {
            if (!IsResolvableType(type))
                throw new UnresolvableTypeException();

            return typeResolvers.SingleOrDefault(x => x.DestinationType == type);
        }
    }

    public abstract class TypeResolver<T> : ITypeResolver<T>
    {
        public abstract T Resolve(string value);

    }
}
