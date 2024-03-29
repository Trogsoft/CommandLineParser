# CommandLineParser

[![Build Status](https://dev.azure.com/trogsoft-ltd/Trogsoft.CommandLineParser/_apis/build/status/Trogsoft.CommandLineParser?branchName=develop)](https://dev.azure.com/trogsoft-ltd/Trogsoft.CommandLineParser/_build/latest?definitionId=8&branchName=develop)

Available on NuGet

```
Install-Package Trogsoft.CommandLine
```

This is a command line argument parser which separates out actions into self contained classes known as verbs.  To create a verb:

```c#
[Verb("greet")]
public class TestVerb : Verb 
{

	[Operation(Default = true)]
	public int DefaultOperation(string name) 
	{
		Console.WriteLine($"Hello, {name}.");
		return 0;
	}

}
```

In `Program.cs` you can then configure the command line parser.

```c#
using Trogsoft.CommandLine;

namespace Test 
{
	public class Test 
	{
		public static int Main(string[] args) 
		{
			var cmdParser = new Parser();
			return cmdParser.Run(args);
		}
	}
}

```

On the command line, you can then call

```
> test greet --name Dave
Hello, Dave.
```

For more detailed documentation, check the [Wiki](https://github.com/Trogsoft/CommandLineParser/wiki).