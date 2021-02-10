using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trogsoft.CommandLine;

namespace Tester
{
    [Verb("greet", HelpText = "Greet someone.")]
    public class Greet : Verb
    {

        [Operation(isDefault: true)]
        [Parameter("name", HelpText = "The name of the person to greet.", IsRequired = true)]
        [Parameter("greeting", HelpText = "The greeting you would like to use.", IsRequired = false)]
        public int DefaultOperation(string name, string greeting)
        {
            Console.WriteLine($"Hello, {name}.");
            return 0;
        }

    }
}
