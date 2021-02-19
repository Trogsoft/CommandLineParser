namespace Trogsoft.CommandLine
{
    internal class CommandLineParameterInfo
    {
        public bool Exists { get; set; }
        public int Position { get; set; }
        public string Value { get; set; }
        public bool HasValue { get; set; }
    }
}