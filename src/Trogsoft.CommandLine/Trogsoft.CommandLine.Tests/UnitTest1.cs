using NUnit.Framework;
using System;
using System.IO;

namespace Trogsoft.CommandLine.Tests
{
    public class Tests
    {

        private Parser parser = new Parser();

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SimpleVerbDefaultActionTest()
        {
            Assert.AreEqual(15, parser.Run(new string[] { "test" }));
        }

        [Test]
        public void SimpleVerbSpecificAction()
        {
            Assert.AreEqual(16, parser.Run(new string[] { "test", "actiona" }));
        }

        [Test]
        public void SingleParameter()
        {
            Assert.AreEqual(17, parser.Run(new string[] { "test", "actionWithParameter", "-a", "17" }));
        }

        [Test]
        public void ActionWithUnconfiguredParameter()
        {
            Assert.AreEqual(18, parser.Run(new string[] { "test", "actionwithunconfiguredparameter", "-a", "18" }));
        }

        [Test]
        public void DefaultActionUnconfiguredParameter()
        {
            Assert.AreEqual(19, parser.Run(new string[] { "testb", "-a", "19" }));
        }

        [Test]
        public void ListCounter()
        {
            Assert.AreEqual(3, parser.Run(new string[] { "testb", "CountItemsInList", "-i", "item1,item2,item3" }));
        }

        [Test]
        public void ListTypeError()
        {
            Assert.Throws<FormatException>(() => parser.Run(new string[] { "testb", "ListError1", "--items", "a,b,c" }));
        }

        [Test(Description = "Tests the unconfigured parameter named 'items' as well as the appropriate casting.  This action adds together each number in the list.")]
        public void IntList()
        {
            Assert.AreEqual(6, parser.Run(new string[] { "testb", "ListError1", "--items", "1 2 3" }));
        }

        [Test]
        public void TestGreet()
        {
            Assert.AreEqual(0, parser.Run(new string[] { "greet", "--name", "Dave" }));
        }

        [Test]
        public void TestGreet2()
        {
            // Missing parameter here.
            Assert.AreEqual(4, parser.Run(new string[] { "greet", "Dave" }));
        }

        [Test]
        public void TestNamedOperation()
        {
            Assert.AreEqual(0, parser.Run(new string[] { "testb", "cheese", "--cheeseName", "cheddar" }));
        }

        [Test]
        public void MissingParameter()
        {
            Assert.AreEqual(4, parser.Run(new string[] { "testb", "cheese" }));
        }

        [Test]
        public void MissingOperation()
        {
            Assert.AreEqual(3, parser.Run(new string[] { "testc" }));
        }

        [Test]
        public void MissingParametersOnDefaultOperation()
        {
            Assert.AreEqual(4, parser.Run(new string[] { "testb" }));
        }

        [Test]
        public void MissingPositionalParameterOnDefaultOperation()
        {
            Assert.AreEqual(0, parser.Run(new string[] { "testd" }));
        }

        [Test]
        public void MissingRequiredPositionalParameterOnDefaultOperation()
        {
            Assert.AreEqual(4, parser.Run(new string[] { "teste" }));
        }

        [Test]
        public void MissingParameterValue()
        {
            Assert.AreEqual(4, parser.Run(new string[] { "testb", "cheese", "--cheeseName" }));
        }

        [Test]
        public void PositionalParameters()
        {
            Assert.AreEqual(0, parser.Run(new string[] { "testb", "pospara", "path/path/path", "--filename", "fn.txt" }));
        }

        [Test]
        public void Enum()
        {
            Assert.AreEqual(0, parser.Run(new string[] { "testb", "enumtest", "-e", "chicken" }));
            Assert.AreEqual(5, parser.Run(new string[] { "testb", "enumtest", "-e", "invalid_value" }));
        }

        [Test]
        public void DefaultVerb()
        {
            Assert.AreEqual(15, parser.Run(new string[] { }));
        }

        [Test]
        public void DefaultVerbWithParameters()
        {
            Assert.AreEqual(27, parser.Run(new string[] { "-v", "potato" }));
        }

        [Test]
        public void ModelTest()
        {
            Assert.AreEqual(2, parser.Run(new string[] { "testd", "model", "-s", "3", "-e", "5" }));
        }

        [Test]
        public void ModelWithOptionalAndDefaultParams()
        {
            Assert.AreEqual(6, parser.Run(new string[] { "testd", "model2", "-i", "6", "--stringWithoutDefault", "value" })); // unspecified optional parameter
            Assert.AreEqual(16, parser.Run(new string[] { "testd", "model2", "-i", "6", "--stringWithDefault", "test", "--stringWithoutDefault", "value" })); // specified optional parameter
            Assert.AreEqual(4, parser.Run(new string[] { "testd", "model2", "-i", "6" })); // missing parameter
        }

    }
}