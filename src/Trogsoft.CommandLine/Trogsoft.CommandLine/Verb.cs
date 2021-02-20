using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trogsoft.CommandLine
{
    public class Verb
    {

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
