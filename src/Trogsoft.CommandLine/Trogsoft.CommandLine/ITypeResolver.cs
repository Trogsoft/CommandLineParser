using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    public interface ITypeResolver
    {

    }

    public interface ITypeResolver<T> : ITypeResolver
    {
        ResolutionResult<T> Resolve(string value);
    }
}
