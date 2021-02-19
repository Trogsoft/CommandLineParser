using System;
using Trogsoft.CommandLine;
using Trogsoft.CommandLine.Tests;

namespace Tester
{
    class Program
    {
        static int Main(string[] args)
        {
            TestModel tm = new TestModel();
            var parser = new Parser();
            return parser.Run(args);
        }
    }
}
