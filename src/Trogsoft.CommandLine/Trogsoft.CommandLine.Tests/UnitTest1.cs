using NUnit.Framework;
using System;

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
            Assert.Throws<FormatException>(() => parser.Run(new string[] { "testb", "ListError1", "-i", "a,b,c" }));
        }

        [Test(Description = "Tests the unconfigured parameter named 'items' as well as the appropriate casting.  This action adds together each number in the list.")]
        public void IntList()
        {
            Assert.AreEqual(6, parser.Run(new string[] { "testb", "ListError1", "--items", "1 2 3" }));
        }

    }
}