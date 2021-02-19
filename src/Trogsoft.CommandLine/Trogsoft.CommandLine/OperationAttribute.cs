using System;

namespace Trogsoft.CommandLine
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OperationAttribute : Attribute
    {

        public OperationAttribute()
        {
        }

        public OperationAttribute(string name)
        {
            Name = name;
        }

        public OperationAttribute(string name, bool isDefault) : this(isDefault)
        {
            Name = name;
        }

        public OperationAttribute(bool isDefault)
        {
            IsDefault = isDefault;
        }

        public string HelpText { get; set; }
        public bool IsDefault { get; } = false;
        public string Name { get; }
    }
}