using System;

namespace Trogsoft.CommandLine
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OperationAttribute : Attribute
    {

        public OperationAttribute()
        {
        }

        public OperationAttribute(bool isDefault)
        {
            IsDefault = isDefault;
        }

        public bool IsDefault { get; } = false;
    }
}