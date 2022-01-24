using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trogsoft.CommandLine
{
    public class Verb
    {

        private void ColoredText(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        protected void WriteError(string text) => ColoredText(text, ConsoleColor.Red);
        protected void WriteWarning(string text) => ColoredText(text, ConsoleColor.Yellow);
        protected void WriteTitle(string text) => ColoredText(text, ConsoleColor.White);

        protected void WriteTextBlock(string text, int maxWidth = 80)
        {
            int indentWidth = Console.CursorLeft;
            if (maxWidth - indentWidth < 10)
            {
                Console.WriteLine();
                indentWidth = 0;
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                var helpWords = text.Split();
                var wCount = 0;
                var line = "";
                List<string> lines = new List<string>();

                foreach (var word in helpWords)
                {
                    line += word + " ";
                    wCount += word.Length + 1;
                    if (wCount >= (maxWidth - indentWidth))
                    {
                        wCount = 0;
                        lines.Add(line);
                        line = "";
                    }
                }

                if (!string.IsNullOrWhiteSpace(line))
                    lines.Add(line);

                foreach (var l in lines)
                {
                    Console.WriteLine(l.Trim());
                    if (l != lines.Last())
                        Console.Write(new string(Enumerable.Range(0, indentWidth).Select(x => ' ').ToArray()));
                }

            }
        }

    }

}
