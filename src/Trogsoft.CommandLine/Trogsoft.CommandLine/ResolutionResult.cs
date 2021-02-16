using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    public class ResolutionResult<T> : ResolutionResult
    {
        internal ResolutionResult(T result) : base(true)
        {
            Result = result;
        }

        internal ResolutionResult() : base(false)
        {
        }

        public T Result { get; }
    }

    public class ResolutionResult
    {
        internal ResolutionResult(bool success)
        {
            Success = success;
        }

        public bool Success { get; }
    }
}
