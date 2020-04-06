using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SampleParser
{
    internal sealed class SampleFrame : Frame
    {
        int ownSampleCount;
        public override int SampleCount { get; }
        public override string MethodName { get; }
        public override int OwnSampleCount => ownSampleCount;
        public string Tail { get; }

        internal SampleFrame(ReadOnlySpan<char> line)
        {
            SampleCount = int.Parse(GetToken(ref line, wsCount: 1).ToString());
            ownSampleCount = SampleCount;
            MethodName = GetToken(ref line).ToString();
            Tail = line.ToString();
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

            bool CheckWhitespace(ReadOnlySpan<char> line, int i, int count)
            {
                for (int j = 0; j < wsCount; ++j)
                {
                    if (!char.IsWhiteSpace(line[i + j]))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        protected override void OnAddChild(Frame frame)
        {
            base.OnAddChild(frame);

            ownSampleCount -= frame.SampleCount;
        }
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class Frame : IReadOnlyList<Frame>
    {
        object? children;

        public abstract int SampleCount { get; }
        public abstract int OwnSampleCount { get; }
        public abstract string MethodName { get; }

        internal void AddChild(Frame frame)
        {
            if (children is Frame old)
            {
                children = new Frame[2] { old, frame };
            }
            else if (children is Frame[] arr)
            {
                var temp = new Frame[arr.Length + 1];
                Array.Copy(arr, temp, arr.Length);
                temp[arr.Length] = frame;

                children = temp;
            }
            else
            {
                children = frame;
            }
            OnAddChild(frame);
        }

        protected virtual void OnAddChild(Frame frame)
        {
        }

        string DebuggerDisplay => string.Format("{0} +{1} {2}", SampleCount, OwnSampleCount, MethodName);

        public int Count => children is Frame[] arr
            ? arr.Length
            : children is Frame ? 1 : 0;

        public Frame this[int index]
        {
            get
            {
                if (children is Frame[] arr)
                    return arr[index];

                if (index == 0 && children is Frame value)
                {
                    return value;
                }

                throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range. Must be non-negative and less than the size of the collection.");
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(children);

        IEnumerator<Frame> IEnumerable<Frame>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<Frame>
        {
            Frame[]? values;
            Frame? value;
            int index;
            Frame? current;

            public Enumerator(object? children)
            {
                values = children as Frame[];
                value = children as Frame;

                current = default;
                index = 0;
            }

            public Frame Current => current!;
            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (value != null)
                {
                    if (current == null)
                    {
                        current = value;
                        return true;
                    }
                }
                else if (values != null)
                {
                    if (index < values.Length)
                    {
                        current = values[index];
                        index++;
                        return true;
                    }
                }

                return false;
            }

            public void Reset()
            {
                current = default;
                index = 0;
            }
        }
    }
}
