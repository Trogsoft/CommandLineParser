using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trogsoft.CommandLine.Tests
{
    [Verb("test")]
    public class TestVerb : Verb
    {
        [Operation(true)]
        public int Default()
        {
            return 15;
        }

        [Operation]
        public int ActionA()
        {
            return 16;
        }

        [Operation]
        [Parameter('a', HelpText = "Pick a number between 1 and about 2 billion", IsRequired = true)]
        public int ActionWithParameter(int a)
        {
            return a;
        }

        [Operation]
        public int ActionWithUnconfiguredParameter(int a)
        {
            return a;
        }

    }

    [Verb("testb")]
    public class SecondTestVerb : Verb
    {
        [Operation(true)]
        public int Default(int a)
        {
            return a;
        }

        [Operation]
        [Parameter('i', "items", ListSeparator = ",")]
        public int CountItemsInList(List<string> items)
        {
            return items.Count;
        }

        [Operation]
        public int ListError1(List<int> items)
        {
            return items.Sum();
        }

    }

    [Verb("greet")]
    public class GreetVerb : Verb
    {
        [Operation(true)]
        public int DefaultAction(string name)
        {
            Console.WriteLine($"Hello, {name}.");
            return 0;
        }
    }

}
