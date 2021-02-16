using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    public interface IModelResolver
    {
        object Resolve(string[] args, Type targetType);
    }
}
