using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SampleParser
{
    // Maybe use ObservableCollection

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Frame : IReadOnlyList<Frame>
    {
        internal Frame? Parent { get; private set; }
        readonly List<Frame> children = new List<Frame>();

        public Frame(int sampleCount, int ownSampleCount, string methodName)
        {
            SampleCount = sampleCount;
            OwnSampleCount = ownSampleCount;
            MethodName = methodName;
        }

        public int SampleCount { get; protected set; }
        public int OwnSampleCount { get; protected set; }
        public string MethodName { get; protected set; }

        internal void AddChild(Frame frame, bool subtractFromOwnTime)
        {
            frame.Parent = this;
            children.Add(frame);

            if (subtractFromOwnTime)
                OwnSampleCount -= frame.SampleCount;

            //children.TrimExcess();
        }

        // Pruning removes this subtree and propagates its cost reduction upwards.
        internal void Prune()
        {
            if (Parent == null)
            {
                return;
            }

            Parent.children.Remove(this);

            var node = Parent;
            while (node != null)
            {
                node.SampleCount -= SampleCount;
                node = node.Parent;
            }
        }

        // Charging removes this frame and adds its value to the parents
        internal void Charge()
        {
            if (Parent == null)
            {
                return;
            }

            Parent.OwnSampleCount += OwnSampleCount;

            int index = Parent.children.IndexOf(this);
            Parent.children.RemoveAt(index);
            Parent.children.InsertRange(index, children);
        }

        public IEnumerable<Frame> Recurse()
        {
            yield return this;

            foreach (var frame in children)
            {
                foreach (var val in frame.Recurse())
                    yield return val;
            }
        }

        string DebuggerDisplay => string.Format("{0} +{1} {2}", SampleCount, OwnSampleCount, MethodName);

        public int Count => children.Count;

        public Frame this[int index] => children[index];

        public Enumerator GetEnumerator() => new Enumerator(children);

        IEnumerator<Frame> IEnumerable<Frame>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<Frame>
        {
            List<Frame>.Enumerator underlying;

            public Enumerator(List<Frame> children)
            {
                underlying = children.GetEnumerator();
            }

            public Frame Current => underlying.Current;
            object IEnumerator.Current => Current;

            public void Dispose() => underlying.Dispose();

            public bool MoveNext() => underlying.MoveNext();

            public void Reset() { }
        }
    }
}
