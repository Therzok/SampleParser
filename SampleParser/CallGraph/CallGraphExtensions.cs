using System;
namespace SampleParser
{
    public static class CallGraphExtensions
    {
        static readonly string[] waitFrames =
        {
            "semaphore_timedwait_trap",
            "semaphore_wait_trap",
            "__psynch_cvwait",
            "__psynch_mutexwait",
            "__workq_kernreturn",
            "mach_msg_trap",
        };

        public static CallGraph CpuTime(this CallGraph callGraph)
        {
            return callGraph.Prune(x => Array.IndexOf(waitFrames, x.MethodName) >= 0);
        }
    }
}
