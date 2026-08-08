using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.GLibSynchronizationContext</c> — what makes <c>await</c> work in a
    /// Gtk application.
    /// </summary>
    /// <remarks>
    /// <c>Application.Init</c> installs this context on the thread that called it,
    /// which is what lets an <c>await</c> in an event handler resume on the Gtk
    /// thread instead of on a thread-pool thread that may not touch a widget. It
    /// is hand-written, it is sixty lines, and none of it was covered.
    ///
    /// Every test here compares <c>ManagedThreadId</c>, because "the continuation
    /// ran" is not the question — the question is *where*, and a continuation
    /// that resumed on the wrong thread satisfies any test that merely waits for
    /// a flag. Each positive is paired with the arrangement that must **not**
    /// come back to the Gtk thread, so the assertion is reporting the context
    /// rather than the fact that a task completed.
    /// </remarks>
    public class AsyncContextTests : GtkTestBase
    {
        public AsyncContextTests(GtkFixture fixture) : base(fixture) { }

        static int ThreadId => Thread.CurrentThread.ManagedThreadId;

        /// <summary>Turns the main loop until <paramref name="condition"/> holds.
        /// Continuations posted by this context arrive as idle sources, so
        /// nothing resumes unless somebody is running the loop.</summary>
        static bool PumpUntil(Func<bool> condition, int timeoutMs = 5000)
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            while (!condition() && clock.ElapsedMilliseconds < timeoutMs)
            {
                if (Gtk.Application.EventsPending())
                    Gtk.Application.RunIteration(false);
                else
                    Thread.Sleep(1);
            }

            return condition();
        }

        // ---------------------------------------------------------- the context

        [Fact]
        public void Initialising_gtk_leaves_a_glib_synchronization_context_behind()
        {
            Run(() => Assert.IsType<GLib.GLibSynchronizationContext>(SynchronizationContext.Current));
        }

        [Fact]
        public void A_thread_that_never_initialised_gtk_has_no_glib_context()
        {
            // The contrast that gives the test above its meaning: the context is
            // installed per thread by Init, not process-wide.
            SynchronizationContext seen = new SynchronizationContext();
            var worker = new Thread(() => seen = SynchronizationContext.Current);

            worker.Start();
            worker.Join();

            Assert.Null(seen);
        }

        // ------------------------------------------------------------ Post

        [Fact]
        public void Post_does_not_run_the_callback_before_it_returns()
        {
            // Post is the asynchronous half: it queues an idle and comes back.
            // If it ran inline, an await would reenter the caller.
            Run(() =>
            {
                var context = SynchronizationContext.Current;
                bool ran = false;

                context.Post(_ => ran = true, null);

                Assert.False(ran, "Post should queue the callback, not run it");

                Assert.True(PumpUntil(() => ran), "and the loop should then run it");
            });
        }

        [Fact]
        public void Post_from_another_thread_runs_the_callback_on_the_gtk_thread()
        {
            Run(() =>
            {
                var context = SynchronizationContext.Current;
                int gtkThread = ThreadId;
                int ranOn = -1;
                int postedFrom = -1;

                var worker = new Thread(() =>
                {
                    postedFrom = ThreadId;
                    context.Post(_ => ranOn = ThreadId, null);
                });

                worker.Start();
                worker.Join();

                Assert.True(PumpUntil(() => ranOn != -1), "the callback should have run");

                Assert.Equal(gtkThread, ranOn);
                Assert.NotEqual(gtkThread, postedFrom);
            });
        }

        [Fact]
        public void Two_posts_run_in_the_order_they_were_made()
        {
            // Both become idle sources at the same priority, and GLib dispatches
            // those first-in-first-out. Ordering is what a caller posting a
            // sequence of UI updates is relying on.
            Run(() =>
            {
                var context = SynchronizationContext.Current;
                var order = new System.Collections.Generic.List<int>();

                for (int i = 0; i < 5; i++)
                {
                    int n = i;
                    context.Post(_ => order.Add(n), null);
                }

                Assert.True(PumpUntil(() => order.Count == 5), "all five should run");

                Assert.Equal(new[] { 0, 1, 2, 3, 4 }, order);
            });
        }

        // ------------------------------------------------------------ Send

        [Fact]
        public void Send_from_another_thread_returns_only_after_the_callback_has_run()
        {
            // Send is the blocking half. The oracle is what the *caller* can see
            // once it returns: a value written by the callback on the Gtk thread.
            // A Send that returned early would read the value it started with.
            Run(() =>
            {
                var context = SynchronizationContext.Current;
                int gtkThread = ThreadId;

                int shared = 0;
                int observedAfterSend = -1;
                int ranOn = -1;

                var worker = new Thread(() =>
                {
                    context.Send(_ =>
                    {
                        ranOn = ThreadId;
                        Thread.Sleep(20);     // so an early return would be visible
                        shared = 42;
                    }, null);

                    observedAfterSend = shared;
                });

                worker.Start();

                Assert.True(PumpUntil(() => !worker.IsAlive), "the worker should finish");
                worker.Join();

                Assert.Equal(42, observedAfterSend);
                Assert.Equal(gtkThread, ranOn);
            });
        }

        [Fact]
        public void An_exception_inside_Send_reaches_the_thread_that_called_it()
        {
            // The callback runs on the Gtk thread, so without this the exception
            // would be lost in an idle handler and the caller would carry on as
            // though the work had succeeded.
            Run(() =>
            {
                var context = SynchronizationContext.Current;
                Exception caught = null;

                var worker = new Thread(() =>
                {
                    try
                    {
                        context.Send(_ => throw new InvalidOperationException("from the callback"), null);
                    }
                    catch (Exception e)
                    {
                        caught = e;
                    }
                });

                worker.Start();

                Assert.True(PumpUntil(() => !worker.IsAlive), "the worker should finish");
                worker.Join();

                var error = Assert.IsType<InvalidOperationException>(caught);
                Assert.Equal("from the callback", error.Message);
            });
        }

        // ------------------------------------------------------------- await

        [Fact]
        public void An_await_resumes_on_the_gtk_thread()
        {
            // The reason the context exists. Everything after an await in an
            // event handler must be able to touch widgets, and only the captured
            // context makes that true.
            Run(() =>
            {
                int gtkThread = ThreadId;
                int resumedOn = -1;
                int ranOffThreadOn = -1;

                async Task Work()
                {
                    await Task.Run(() =>
                    {
                        ranOffThreadOn = ThreadId;
                        Thread.Sleep(10);
                    });

                    resumedOn = ThreadId;
                }

                var task = Work();

                Assert.True(PumpUntil(() => task.IsCompleted), "the task should finish");
                task.GetAwaiter().GetResult();          // surface anything it threw

                Assert.NotEqual(gtkThread, ranOffThreadOn);   // the work really left
                Assert.Equal(gtkThread, resumedOn);           // and the continuation came back
            });
        }

        [Fact]
        public void ConfigureAwait_false_gives_up_the_gtk_thread()
        {
            // The trap on the other side: a library that awaits with
            // ConfigureAwait(false) resumes wherever the task completed, so
            // anything after it must not touch a widget. Pinned because it is the
            // difference between working code and a crash that depends on timing.
            Run(() =>
            {
                int gtkThread = ThreadId;
                int resumedOn = -1;

                async Task Work()
                {
                    await Task.Run(() => Thread.Sleep(10)).ConfigureAwait(false);
                    resumedOn = ThreadId;
                }

                var task = Work();

                Assert.True(task.Wait(5000), "the task should finish without the loop being pumped");

                Assert.NotEqual(gtkThread, resumedOn);
            });
        }

        [Fact]
        public void Without_the_context_a_continuation_does_not_come_back_to_the_gtk_thread()
        {
            // The control for An_await_resumes_on_the_gtk_thread: with the
            // context removed the identical code resumes elsewhere, so that test
            // is reporting the context rather than some property of the fixture.
            Run(() =>
            {
                var installed = SynchronizationContext.Current;
                int gtkThread = ThreadId;
                int resumedOn = -1;

                try
                {
                    SynchronizationContext.SetSynchronizationContext(null);

                    async Task Work()
                    {
                        await Task.Run(() => Thread.Sleep(10));
                        resumedOn = ThreadId;
                    }

                    var task = Work();

                    Assert.True(task.Wait(5000), "the task should finish without the loop being pumped");
                    Assert.NotEqual(gtkThread, resumedOn);
                }
                finally
                {
                    // Every later test on this thread depends on it being back.
                    SynchronizationContext.SetSynchronizationContext(installed);
                }
            });
        }

        [Fact]
        public void An_exception_thrown_after_an_await_is_carried_on_the_task()
        {
            // A continuation that throws runs inside a GLib idle handler, where
            // there is no caller to catch it. It has to arrive on the Task
            // instead, or an async void handler would take the process down.
            Run(() =>
            {
                async Task Work()
                {
                    await Task.Run(() => Thread.Sleep(5));
                    throw new InvalidOperationException("after the await");
                }

                var task = Work();

                Assert.True(PumpUntil(() => task.IsCompleted), "the task should finish");

                var error = Assert.Throws<InvalidOperationException>(() => task.GetAwaiter().GetResult());
                Assert.Equal("after the await", error.Message);
            });
        }

        [Fact]
        public void A_chain_of_awaits_stays_on_the_gtk_thread_throughout()
        {
            // One resumption proves the context is captured; three prove it is
            // still installed on the resumed thread, which is what makes the
            // second await behave like the first.
            Run(() =>
            {
                int gtkThread = ThreadId;
                var resumptions = new System.Collections.Generic.List<int>();

                async Task Work()
                {
                    for (int i = 0; i < 3; i++)
                    {
                        await Task.Run(() => Thread.Sleep(5));
                        resumptions.Add(ThreadId);
                    }
                }

                var task = Work();

                Assert.True(PumpUntil(() => task.IsCompleted), "the task should finish");
                task.GetAwaiter().GetResult();

                Assert.Equal(new[] { gtkThread, gtkThread, gtkThread }, resumptions);
            });
        }

        [Fact]
        public void An_already_completed_task_still_resumes_through_the_loop()
        {
            // A task that is finished before it is awaited takes the synchronous
            // path in the awaiter, so the continuation runs inline rather than
            // being posted. Worth pinning: it is the one case where code after an
            // await runs without the loop turning at all.
            Run(() =>
            {
                int gtkThread = ThreadId;
                int resumedOn = -1;

                async Task Work()
                {
                    await Task.CompletedTask;
                    resumedOn = ThreadId;
                }

                var task = Work();

                Assert.True(task.IsCompleted, "an already-completed await should not suspend");
                Assert.Equal(gtkThread, resumedOn);
            });
        }
    }
}
