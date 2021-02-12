using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    [AttributeUsage(AttributeTargets.Class)]
    public class VerbAttribute : Attribute
    {

        public VerbAttribute(string name, bool isDefault) : this(name)
        {
            IsDefault = isDefault;
        }

        public VerbAttribute(string name)
        {
            Name = name;
        }

        public string HelpText { get; set; }

        public string Name { get; }
        public bool IsDefault { get; }
    }
}
