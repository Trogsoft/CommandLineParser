namespace Trogsoft.CommandLine.Tests
{
    public class TestModel
    {
        [Parameter('s',"start", IsRequired = true)]
        public int StartNumber { get; set; }

        [Parameter('e', "end", IsRequired = true)]
        public int EndNumber { get; set; }

        public int ReadOnlyProperty { get; private set; }
    }

    public class TestModel2
    {
        public int i { get; set; }
        public string StringWithDefault { get; set; } = "defaultValue";
        public string StringWithoutDefault { get; set; }
    }

}