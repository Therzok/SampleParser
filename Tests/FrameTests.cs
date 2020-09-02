using System;
using System.Linq;
using Xunit;

using static Tests.Helpers;

namespace Tests
{
    public class FrameTests
    {
        const string jsCore = @"
    7940 Thread_2238240: JavaScriptCore bmalloc scavenger
    + 7940 thread_start  (in libsystem_pthread.dylib) + 15  [0x7fff68aa583b]
    +   7940 _pthread_start  (in libsystem_pthread.dylib) + 148  [0x7fff68aa9e65]
    +     7940 void* std::__1::__thread_proxy<std::__1::tuple<std::__1::unique_ptr<std::__1::__thread_struct, std::__1::default_delete<std::__1::__thread_struct> >, void (*)(bmalloc::Scavenger*), bmalloc::Scavenger*> >(void*)  (in JavaScriptCore) + 39  [0x7fff358c1d17]
    +       7940 bmalloc::Scavenger::threadEntryPoint(bmalloc::Scavenger*)  (in JavaScriptCore) + 9  [0x7fff358bf4f9]
    +         4639 bmalloc::Scavenger::threadRunLoop()  (in JavaScriptCore) + 331  [0x7fff358bf97b]
    +         ! 4639 void std::__1::condition_variable_any::wait<std::__1::unique_lock<bmalloc::Mutex> >(std::__1::unique_lock<bmalloc::Mutex>&)  (in JavaScriptCore) + 84  [0x7fff358bb464]
    +         !   4639 std::__1::condition_variable::wait(std::__1::unique_lock<std::__1::mutex>&)  (in libc++.1.dylib) + 18  [0x7fff659bdc2a]
    +         !     4639 _pthread_cond_wait  (in libsystem_pthread.dylib) + 701  [0x7fff68aaa185]
    +         !       4639 __psynch_cvwait  (in libsystem_kernel.dylib) + 10  [0x7fff689e8ce6]
    +         3301 bmalloc::Scavenger::threadRunLoop()  (in JavaScriptCore) + 774  [0x7fff358bfb36]
    +           3301 std::__1::condition_variable::__do_timed_wait(std::__1::unique_lock<std::__1::mutex>&, std::__1::chrono::time_point<std::__1::chrono::system_clock, std::__1::chrono::duration<long long, std::__1::ratio<1l, 1000000000l> > >)  (in libc++.1.dylib) + 93  [0x7fff659bdcc1]
    +             3301 _pthread_cond_wait  (in libsystem_pthread.dylib) + 701  [0x7fff68aaa185]
    +               3301 __psynch_cvwait  (in libsystem_kernel.dylib) + 10  [0x7fff689e8ce6]";

        [Fact]
        public void TestPrune()
        {
            // All frames except __psynch_cvwait have 0 own sample time.
            var callGraph = ProcessCallGraph(jsCore);
            var initialFrames = callGraph[0].Frame.Recurse().ToArray();

            var willRemoveFrames = initialFrames
                .Where(x => x.MethodName == "__psynch_cvwait" || x.MethodName == "_pthread_cond_wait")
                .ToArray();

            Assert.Equal(4, willRemoveFrames.Length);
            Assert.All(initialFrames, x =>
            {
                Assert.NotEqual(0, x.SampleCount);

                if (x.MethodName == "__psynch_cvwait")
                    Assert.NotEqual(0, x.OwnSampleCount);
                else
                    Assert.Equal(0, x.OwnSampleCount);
            });

            // _pthread_cond_wait removed, do_timed_wait increased own time
            callGraph.Prune("_pthread_cond_wait");

            var frames = callGraph[0].Frame.Recurse().ToArray();
            Assert.Equal(initialFrames.Length - willRemoveFrames.Length, frames.Length);

            Assert.DoesNotContain(frames, x => x.MethodName == "__psynch_cvwait" || x.MethodName == "_pthread_cond_wait");
            Assert.All(frames, x =>
            {
                Assert.Equal(0, x.SampleCount);
                Assert.Equal(0, x.OwnSampleCount);
            });
        }

        [Theory]
        [InlineData("_pthread_cond_wait", false)]
        [InlineData("__psynch_cvwait", true)]
        public void Charge(string chargedMethodName, bool hasOwnSampleCount)
        {
            // All frames except __psynch_cvwait have 0 own sample time.
            var callGraph = ProcessCallGraph(jsCore);
            var initialFrames = callGraph[0].Frame.Recurse().ToArray();

            var willRemoveFrames = initialFrames
                .Where(x => x.MethodName == chargedMethodName)
                .Select(x => (frame: x, parent: x.Parent))
                .ToArray();

            Assert.Equal(2, willRemoveFrames.Length);
            Assert.All(willRemoveFrames, (tuple) =>
            {
                var (x, _) = tuple;

                Assert.NotEqual(0, x.SampleCount);
                Assert.Equal(hasOwnSampleCount, x.OwnSampleCount != 0);
            });

            // _pthread_cond_wait removed, do_timed_wait increased own time
            callGraph.Charge(chargedMethodName);

            var frames = callGraph[0].Frame.Recurse().ToArray();
            Assert.Equal(initialFrames.Length - willRemoveFrames.Length, frames.Length);

            Assert.DoesNotContain(frames, x => x.MethodName == chargedMethodName);

            foreach (var (frame, parent) in willRemoveFrames)
            {
                Assert.Equal(parent.OwnSampleCount, frame.OwnSampleCount);
            }
        }
    }
}
