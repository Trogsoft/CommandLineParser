using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    public abstract class TypeResolver<T> : ITypeResolver<T>
    {
        public abstract ResolutionResult<T> Resolve(string value);

        protected ResolutionResult<T> InvalidValue() => new ResolutionResult<T>();
        protected ResolutionResult<T> UnsupportedValue() => new ResolutionResult<T>();
        protected ResolutionResult<T> Failed() => new ResolutionResult<T>();
        protected ResolutionResult<T> Success(T value) => new ResolutionResult<T>(value);

    }
}
