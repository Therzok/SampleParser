using System;
namespace SampleParser
{
    public class Thread
    {
        public Frame Frame { get; }

        public Thread(Frame frame)
        {
            Frame = frame;
        }
        // Maybe add some interesting metrics like total time
    }
}
