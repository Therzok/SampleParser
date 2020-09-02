using System;
using SampleParser;
using Xunit;

namespace Tests
{
    public class SampleFileTests
    {
        [Fact]
        public void Test1()
        {
            _ = SampleFile.Parse("/Users/Therzok/Desktop/sample_startup.txt");
        }
    }
}
