using System;
namespace SampleParser
{
    internal sealed class SampleFrame : Frame
    {
        public string Tail { get; }

        public SampleFrame(int sampleCount, int ownSampleCount, string methodName, string tail)
            : base(sampleCount, ownSampleCount, methodName)
        {
            Tail = tail;
        }
    }
}
