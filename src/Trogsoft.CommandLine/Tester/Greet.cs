using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trogsoft.CommandLine;

namespace Tester
{
    //[Verb("greet", HelpText = "Greet someone.")]
    //public class Greet : Verb
    //{

    //    [Operation(isDefault: true)]
    //    [Parameter("name", HelpText = "The name of the person to greet.", IsRequired = true, Position = 0)]
    //    public int DefaultOperation(string name)
    //    {
    //        Console.WriteLine($"Hello, {name}.");
    //        return 0;
    //    }

    //}
    [Verb("myverb", HelpText = "Provides access to some operations")]
    public class MyVerb : Verb
    {
        [Operation(true)]
        public int MyMethod()
        {
            Console.WriteLine("Nice.");
            return 0;
        }
    }
}
