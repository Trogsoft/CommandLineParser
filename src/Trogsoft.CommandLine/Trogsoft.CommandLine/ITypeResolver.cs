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
        T Resolve(string value);
    }
}
