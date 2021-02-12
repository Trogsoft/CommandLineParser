using System;
using Trogsoft.CommandLine;

namespace Tester
{
    class Program
    {
        static int Main(string[] args)
        {
            var parser = new Parser(true);

            return parser.Run(args);
        }
    }
}
