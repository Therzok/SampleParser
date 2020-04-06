using System;
namespace SampleParser.Internal
{
    abstract class Processor
    {
        public abstract void ProcessLine(ReadOnlySpan<char> line);
    }

    class NullProcessor : Processor
    {
        public override void ProcessLine(ReadOnlySpan<char> line)
        {
        }
    }
}
