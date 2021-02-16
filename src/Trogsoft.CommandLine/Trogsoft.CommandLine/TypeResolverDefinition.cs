using System;

namespace Trogsoft.CommandLine
{
    internal class TypeResolverDefinition
    {
        public Type Converter { get; set; }
        public Type DestinationType { get; set; }
    }
}