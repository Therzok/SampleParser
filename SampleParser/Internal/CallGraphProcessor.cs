using System;
using System.Collections.Generic;

namespace SampleParser.Internal
{
    class CallGraphProcessor : Processor
    {
        readonly List<SampleFrame> frames = new List<SampleFrame>();

        public Frame[] Result => frames.ToArray();

        ParsingFrame? lastFrame;

        public override void ProcessLine(ReadOnlySpan<char> line)
        {
            var frame = new ParsingFrame(line);

            // Starting a new thread
            if (frame.LastSymbolIndex == -1)
            {
                frames.Add(frame.Frame);
            }
            else
            {
                var parent = lastFrame!.GetParentFor(frame);

                parent.Frame.AddChild(frame.Frame);
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

                Frame = new SampleFrame(frameContent);
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
        }
    }
}
