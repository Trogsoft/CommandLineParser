using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    public class VerbAttribute : Attribute
    {
        public VerbAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
