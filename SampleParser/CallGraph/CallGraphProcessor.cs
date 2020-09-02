using System;
using System.Collections.Generic;

namespace SampleParser.Internal
{
    sealed class CallGraphProcessor : Processor
    {
        public CallGraph CallGraph { get; } = new CallGraph();

        ParsingFrame? lastFrame;

        public override void ProcessLine(ReadOnlySpan<char> line)
        {
            var frame = new ParsingFrame(line);

            // Starting a new thread
            if (frame.LastSymbolIndex == -1)
            {
                CallGraph.Add(new Thread(frame.Frame));
            }
            else
            {
                var parent = lastFrame!.GetParentFor(frame);

                parent.Frame.AddChild(frame.Frame, true);
                frame.Parent = parent;
            }

            lastFrame = frame;
        }

        class ParsingFrame
        {
            static readonly char[] knownSymbols = { '+', '!', ':', '|' };

            public int LastSymbolIndex;
            public int NumberIndex;

            public ParsingFrame? Parent;
            public SampleFrame Frame;

            public ParsingFrame(ReadOnlySpan<char> line)
            {
                Split(line, out var prefix, out var frameContent);

                LastSymbolIndex = prefix.LastIndexOfAny(knownSymbols);
                NumberIndex = prefix.Length;

                Frame = ParseSampleFrame(frameContent);
            }

            public ParsingFrame GetParentFor(ParsingFrame frame)
            {
                ParsingFrame parent = this;

                while (frame.LastSymbolIndex < parent.LastSymbolIndex || parent.NumberIndex == frame.NumberIndex)
                {
                    parent = parent.Parent!;
                }

                return parent;
            }

            void Split(ReadOnlySpan<char> line, out ReadOnlySpan<char> prefix, out ReadOnlySpan<char> frameContent)
            {
                int i;
                for (i = 0; i < line.Length; ++i)
                {
                    if (char.IsNumber(line[i]))
                    {
                        break;
                    }
                }

                prefix = line.Slice(0, i);
                frameContent = line.Slice(i);
            }

            SampleFrame ParseSampleFrame(ReadOnlySpan<char> line)
            {
                var sampleCount = int.Parse(GetToken(ref line, wsCount: 1).ToString());
                var methodName = GetToken(ref line).ToString();
                var tail = line.ToString();

                return new SampleFrame(sampleCount, sampleCount, methodName, tail);
            }

            ReadOnlySpan<char> GetToken(ref ReadOnlySpan<char> line, int wsCount = 2)
            {
                int i;
                line = line.TrimStart();

                for (i = 0; i < line.Length; ++i)
                {
                    if (CheckWhitespace(line, i, wsCount))
                        break;
                }

                var res = line.Slice(0, i);
                var temp = line.Slice(i);
                line = temp;
                return res;

                static bool CheckWhitespace(ReadOnlySpan<char> line, int i, int count)
                {
                    for (int j = 0; j < count; ++j)
                    {
                        if (!char.IsWhiteSpace(line[i + j]))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
        }
    }
}
