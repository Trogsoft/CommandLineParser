using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ParameterAttribute : Attribute
    {
        public char ShortName { get; set; }
        public string LongName { get; set; }
        public string HelpText { get; set; }
        public bool IsRequired { get; set; }
        public object Default { get; set; }
        public string Parameter { get; set; }
        public string ListSeparator { get; set; } = " ";
        public int Position { get; set; } = -1;

        public ParameterAttribute()
        {

        }

        public ParameterAttribute(string longName)
        {
            LongName = longName;
        }

        public ParameterAttribute(char shortName)
        {
            ShortName = shortName;
        }

        public ParameterAttribute(char shortName, string longName)
        {
            ShortName = shortName;
            LongName = longName;
        }
    }
}
