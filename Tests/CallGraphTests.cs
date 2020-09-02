using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using SampleParser.Internal;
using SampleParser;
using Tests;
using Xunit;

using static Tests.Helpers;

namespace Tests
{
    public class CallGraphTests
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

        const string expectedJsCore = @"
0 7940 0 Thread_2238240: JavaScriptCore bmalloc scavenger
1 7940 0 thread_start
2 7940 0 _pthread_start
3 7940 0 void* std::__1::__thread_proxy<std::__1::tuple<std::__1::unique_ptr<std::__1::__thread_struct, std::__1::default_delete<std::__1::__thread_struct> >, void (*)(bmalloc::Scavenger*), bmalloc::Scavenger*> >(void*)
4 7940 0 bmalloc::Scavenger::threadEntryPoint(bmalloc::Scavenger*)
5 4639 0 bmalloc::Scavenger::threadRunLoop()
6 4639 0 void std::__1::condition_variable_any::wait<std::__1::unique_lock<bmalloc::Mutex> >(std::__1::unique_lock<bmalloc::Mutex>&)
7 4639 0 std::__1::condition_variable::wait(std::__1::unique_lock<std::__1::mutex>&)
8 4639 0 _pthread_cond_wait
9 4639 4639 __psynch_cvwait
5 3301 0 bmalloc::Scavenger::threadRunLoop()
6 3301 0 std::__1::condition_variable::__do_timed_wait(std::__1::unique_lock<std::__1::mutex>&, std::__1::chrono::time_point<std::__1::chrono::system_clock, std::__1::chrono::duration<long long, std::__1::ratio<1l, 1000000000l> > >)
7 3301 0 _pthread_cond_wait
8 3301 3301 __psynch_cvwait
";

        [Fact]
        public void JavaScriptCore()
        {
            var callGraph = ProcessCallGraph(jsCore);

            Assert.Single(callGraph);
            Assert.Equal(expectedJsCore, callGraph[0].ToPrefixNotation());
        }

        [Fact]
        public void JavaScriptCoreTwice()
        {
            var callGraph = ProcessCallGraph(jsCore + Environment.NewLine + jsCore);

            Assert.Equal(2, callGraph.Count);
            Assert.Equal(expectedJsCore, callGraph[0].ToPrefixNotation());
            Assert.Equal(expectedJsCore, callGraph[1].ToPrefixNotation());
        }

        const string threeChildren = @"
    101 Thread_2238240: Made up example
    + 101 main  (in fake.dylib) + 0  [0x7fff68aa583a]
    +   100 childAll  (in fake.dylib) + 0  [0x7fff68aa583b]
    +   ! 33 childOne  (in fake.dylib) + 0  [0x7fff68aa9e65]
    +   ! | 33 compute  (in fake.dylib) + 0  [0x7fff68aa9e65]
    +   ! 33 childTwo  (in fake.dylib) + 0  [0x7fff68aa9e65]
    +   ! 33 childThree  (in fake.dylib) + 0  [0x7fff68aa9e65]
    +   1 childNone  (in fake.dylib) + 0  [0x7fff68aa583b]";

        const string expectedThreeChildren = @"
0 101 0 Thread_2238240: Made up example
1 101 0 main
2 100 1 childAll
3 33 0 childOne
4 33 33 compute
3 33 33 childTwo
3 33 33 childThree
2 1 1 childNone
";

        [Fact]
        public void ThreeChildren()
        {
            var callGraph = ProcessCallGraph(threeChildren);

            Assert.Single(callGraph);
            Assert.Equal(expectedThreeChildren, callGraph[0].ToPrefixNotation());
        }
    }
}
