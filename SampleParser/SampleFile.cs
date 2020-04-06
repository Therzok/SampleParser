using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using SampleParser.Internal;

namespace SampleParser
{
    public class SampleFile
    {
        public static SampleFile Parse(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            return Parse(stream);
        }

        public static SampleFile Parse(Stream stream)
        {
            using var streamReader = new StreamReader(stream);

            Processor processor = new NullProcessor();
            string line;

            // TODO: PERF: Maybe use Read into an array instead of strings.
            while ((line = streamReader.ReadLine()) != null)
            {
                // If the line has no leading whitespace, it's a header
                if (line.Length == 0)
                {
                    continue;
                }

                if (line[0] != ' ')
                {
                    switch (line)
                    {
                        case Headers.CallGraph:
                            processor = new CallGraphProcessor();
                            break;
                        case Headers.TotalNumbersInStack:
                            // TODO: Implement this
                            processor = new NullProcessor();
                            break;
                    }

                    continue;
                }

                // We have a line starting with 4 whitespace chars, it's part of the processor's job to parse it.
                processor.ProcessLine(line.AsSpan(4));
            }

            return new SampleFile();
        }

        // SampleProcessor - Parse method
        // + HeaderProcessor: general sample data
        // + CallGraphProcessor
        // + - 4 leading spaces <stacktrace>
        // + HitCountProcessor
        // + BinaryImagesProcessor§


        // stacktrace:
        // <number> Thread info
        // <stackframe>

        // stackframe
        // <symbol> <number> <method name>
        // <symbol> <stackframe>
    }
}
