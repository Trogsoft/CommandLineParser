using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trogsoft.CommandLine.Tests
{
    [Verb("test", true, HelpText = "Some simple numeric tests")]
    public class TestVerb : Verb
    {
        [Operation(true, HelpText = "If the parameter is carrot, returns 15, otherwise 27.")]
        [Parameter('v', HelpText = "Carrot, or not.")]
        public int Default(string v = "carrot")
        {
            if (v == "carrot")
            {
                return 15;
            }
            else
            {
                return 27;
            }
        }

        [Operation(HelpText = "Returns 16.")]
        public int ActionA()
        {
            return 16;
        }

        [Operation(HelpText = "Returns the numer you feed to it.")]
        [Parameter('a', HelpText = "Pick a number between 1 and about 2 billion", IsRequired = true)]
        public int ActionWithParameter(int a)
        {
            return a;
        }

        [Operation(HelpText = "Returns the number you feed to it, but the parameter is explicit.")]
        public int ActionWithUnconfiguredParameter(int a)
        {
            return a;
        }

    }

    [Verb("testb", HelpText = "Some additional tests with lists and cheese.")]
    public class SecondTestVerb : Verb
    {
        [Operation(true, HelpText = "Returns the number you pass to it.")]
        public int Default(int a)
        {
            return a;
        }

        [Operation(HelpText = "Returns number of items in a comma separated list.")]
        [Parameter('i', "items", ListSeparator = ",", HelpText = "A comma separated list of items.")]
        public int CountItemsInList(List<string> items)
        {
            return items.Count;
        }

        [Operation(HelpText = "Returns the sum of numeric items in a list.")]
        public int ListError1(List<int> items)
        {
            return items.Sum();
        }

        [Operation("cheese", HelpText = "Cheddar is worth 0; every other cheese 9.")]
        public int NamedSomethingElse(string cheeseName)
        {
            return cheeseName == "cheddar" ? 0 : 9;
        }

        [Operation("pospara", HelpText = "Testing positional parameters.")]
        [Parameter("path", Position = 0, HelpText = "A path.", IsRequired = true)]
        public int PositionalParameters(string path, string filename)
        {
            var result = 2;
            if (!string.IsNullOrWhiteSpace(path) && path == "path/path/path") result--;
            if (!string.IsNullOrWhiteSpace(filename) && filename == "fn.txt") result--;
            return result;
        }

        [Operation]
        public int EnumTest(EnumTest e)
        {
            return 0;
        }

        [Operation]
        public int TestDecimal(decimal val) => (int)Math.Floor(Math.Abs(val));

        [Operation]
        public int TestFloat(float val) => (int)Math.Floor(Math.Abs(val));

        [Operation]
        public int TestDouble(double val) => (int)Math.Floor(Math.Abs(val));



    }

    public enum EnumTest
    {
        EVA,
        EVB,
        Chicken
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

    [Verb("testc")]
    public class TestCVerb: Verb
    {
        [Operation]
        public int Action(string name)
        {
            return 0;
        }
    }

    [Verb("testd")]
    public class TestDVerb : Verb
    {
        [Operation(true)]
        [Parameter("name", Position = 0)]
        public int Action(string name)
        {
            return 0;
        }

        [Operation("model")]
        public int ModelAction(TestModel model)
        {
            return model.EndNumber - model.StartNumber;
        }

        [Operation("model2")]
        public int ModelAction2(TestModel2 model)
        {
            if (model.StringWithDefault != "defaultValue") return 16;
            return model.i;
        }

        [Operation("complexmodel")]
        public int ComplexModelTest(ComplexModel model)
        {
            var guid = new Guid("12E198EC-9C9E-44B9-9AC2-251E6157F850");
            if (model.Time.DayOfWeek == DayOfWeek.Monday && model.Guid == guid) return 0;
            return 1;
        }

        [Operation("datetime")]
        public int DateTimeaction(DateTime time)
        {
            if (time.DayOfWeek == DayOfWeek.Monday) return 0;
            return 1;
        }

        [Operation("uri")]
        public int UriAction(Uri uri) => 0;

        [Operation("guid")]
        public int GuidAction(Guid guid) => 0;

        [Operation("listofdecimals")]
        public int ListOfDecimals(List<decimal> decimals)
        {
            return (int)Math.Floor(Math.Abs(decimals.Sum()));
        }

    }

    [Verb("teste")]
    public class TestEVerb : Verb
    {
        [Operation(true)]
        [Parameter("name", Position = 0, IsRequired = true)]
        public int Action(string name)
        {
            return 0;
        }

        [Operation]
        public int Longwrap()
        {
            WriteTextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque non magna risus. Integer vestibulum lobortis sapien ut cursus. Maecenas vestibulum felis suscipit felis pretium molestie. Proin in sagittis magna. Quisque turpis massa, tempus nec dolor sit amet, sagittis volutpat enim. Vestibulum tempor consectetur orci in gravida. Curabitur auctor tortor et sapien hendrerit bibendum. Cras lobortis et nibh vitae hendrerit. Vivamus enim libero, sollicitudin id lacus sit amet, lacinia hendrerit leo.");
            Console.WriteLine();
            Console.Write("           ");
            WriteTextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque non magna risus. Integer vestibulum lobortis sapien ut cursus. Maecenas vestibulum felis suscipit felis pretium molestie. Proin in sagittis magna. Quisque turpis massa, tempus nec dolor sit amet, sagittis volutpat enim. Vestibulum tempor consectetur orci in gravida. Curabitur auctor tortor et sapien hendrerit bibendum. Cras lobortis et nibh vitae hendrerit. Vivamus enim libero, sollicitudin id lacus sit amet, lacinia hendrerit leo.");
            Console.WriteLine();
            Console.Write("                                                                   something: ");
            WriteTextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque non magna risus. Integer vestibulum lobortis sapien ut cursus. Maecenas vestibulum felis suscipit felis pretium molestie. Proin in sagittis magna. Quisque turpis massa, tempus nec dolor sit amet, sagittis volutpat enim. Vestibulum tempor consectetur orci in gravida. Curabitur auctor tortor et sapien hendrerit bibendum. Cras lobortis et nibh vitae hendrerit. Vivamus enim libero, sollicitudin id lacus sit amet, lacinia hendrerit leo.");
            return 0;
        }

    }

}
