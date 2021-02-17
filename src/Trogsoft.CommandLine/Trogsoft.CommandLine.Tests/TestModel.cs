using System;

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

    public class ComplexModel
    {
        public DateTime Time { get; set; }
        public Guid Guid { get; set; } = new Guid("12E198EC-9C9E-44B9-9AC2-251E6157F850");
    }

}