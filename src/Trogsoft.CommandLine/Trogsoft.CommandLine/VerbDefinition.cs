using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    internal class VerbDefinition
    {
        public Type Type { get; set; }
        public VerbAttribute Verb { get; set; }

        public VerbDefinition(VerbAttribute verb, Type type)
        {
            Verb = verb;
            Type = type;
        }
    }
}
