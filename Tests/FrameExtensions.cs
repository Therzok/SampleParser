using System;
using System.Text;
using SampleParser;

namespace Tests
{
    public static class FrameExtensions
    {
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
    }
}
