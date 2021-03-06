﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class ParameterAttribute : Attribute
    {
        public char ShortName { get; set; }
        public string LongName { get; set; }
        public string HelpText { get; set; }
        public bool IsRequired { get; set; } = true;
        public object Default { get; set; }
        public string Parameter { get; set; }
        public string ListSeparator { get; set; } = " ";
        public int Position { get; set; } = -1;
        internal Type Type { get; set; }
        internal bool isSimpleType { get; set; }

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
