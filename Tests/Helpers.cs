using System;
using System.IO;
using System.Text;
using SampleParser;
using SampleParser.Internal;

namespace Tests
{
    static class Helpers
    {
        public static string ToPrefixNotation(this Thread t)
            => ToPrefixNotation(t.Frame);

        public static string ToPrefixNotation(this Frame f)
        {
            var sb = new StringBuilder().AppendLine();
            ToPrefixNotation(f, sb, 0);
            return sb.ToString();
        }

        static void ToPrefixNotation(Frame f, StringBuilder sb, int depth)
        {
            sb.AppendFormat("{0} {1} {2} {3}", depth, f.SampleCount, f.OwnSampleCount, f.MethodName)
              .AppendLine();
            foreach (var child in f)
            {
                ToPrefixNotation(child, sb, depth + 1);
            }
        }

        public static CallGraph ProcessCallGraph(string text)
        {
            var processor = new CallGraphProcessor();

            using var reader = new StringReader(text);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                if (line.Length == 0)
                    continue;

                processor.ProcessLine(line);
            }

            return processor.CallGraph;
        }
    }
}
