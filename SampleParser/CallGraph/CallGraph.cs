using System;
using System.Collections.Generic;
using System.Linq;

namespace SampleParser
{
    public class CallGraph : List<Thread>
    {
        // CallGraph:
        // * Thread[] - list of threads

        public CallGraph Charge(Func<Frame, bool> filter)
        {
            // Maybe clone the callgraph first, then run the mutating operation.
            foreach (var frame in Find(filter).Reverse().ToArray())
                frame.Charge();

            return this;
        }

        public CallGraph Prune(Func<Frame, bool> filter)
        {
            foreach (var frame in Find(filter).ToArray())
                frame.Prune();

            return this;
        }

        internal IEnumerable<Frame> Find(Func<Frame, bool> filter)
        {
            return Find(this.Select(x => x.Frame), filter);
        }

        static IEnumerable<Frame> Find(IEnumerable<Frame> items, Func<Frame, bool> filter)
        {
            foreach (var frame in items)
            {
                if (filter(frame))
                    yield return frame;

                foreach (var recursive in Find(frame, filter))
                    yield return recursive;
            }
        }
    }
}
