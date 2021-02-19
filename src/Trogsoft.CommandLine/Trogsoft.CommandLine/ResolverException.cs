using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    public class ResolverException : Exception
    {
        public ResolverException(string message, int result) : base(message)
        {
            this.HResult = result;
        } 
    }
}
