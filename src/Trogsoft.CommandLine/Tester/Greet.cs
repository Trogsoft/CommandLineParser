using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trogsoft.CommandLine;

namespace Tester
{
    [Verb("greet")]
    public class Greet : Verb
    {

        [Operation(isDefault: true)]
        public int DefaultOperation(string name)
        {
            Console.WriteLine($"Hello, {name}.");
            return 0;
        }

    }
}
