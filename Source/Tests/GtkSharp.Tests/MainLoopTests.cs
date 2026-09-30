using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.Source</c>, <c>Idle</c>, <c>Timeout</c> and <c>MainContext</c> — the
    /// dispatch machinery every callback in the library eventually arrives
    /// through. The oracles are ordering and arithmetic: a higher-priority source
    /// runs first, a handler returning false runs once, a removed source does not
    /// run at all.
    /// </summary>
    public class MainLoopTests : GtkTestBase
    {
        public MainLoopTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>Runs pending main-loop work without blocking.</summary>
        private static void Drain()
        {
            var guard = 0;
            while (Gtk.Application.EventsPending() && guard++ < 10000)
                Gtk.Application.RunIteration(false);
        }

        /// <summary>
        /// Runs the loop until nothing has been dispatched for longer than the
        /// delay a deferred finalizer uses.
        /// </summary>
        /// <remarks>
        /// <see cref="Drain"/> only clears what is ready <em>now</em>. Every
        /// generated Opaque finalizer in this binding queues its unref onto a
        /// 50 ms timeout, so a source belonging to no test at all can become
        /// ready in the middle of one -- and a timeout outranks an idle, so it
        /// takes the single dispatch a test like
        /// <see cref="Iterating_the_context_dispatches_one_pending_source"/> is
        /// counting. That showed up as roughly one failed run in twenty-five,
        /// always in a test that counts dispatches, never in the same place.
        /// </remarks>
        private static void Quiesce()
        {
            // Capped, because a source that stays ready -- an idle returning
            // true -- would otherwise dispatch forever and hang the run rather
            // than fail it.
            var total = Stopwatch.StartNew();
            var quiet = Stopwatch.StartNew();
            while (quiet.ElapsedMilliseconds < 120 && total.ElapsedMilliseconds < 1000)
                if (GLib.MainContext.Iteration(false))
                    quiet.Restart();
        }

        /// <summary>Blocks until <paramref name="done"/> or the deadline.</summary>
        private static void PumpUntil(Func<bool> done, int milliseconds = 5000)
        {
            var clock = Stopwatch.StartNew();
            while (!done() && clock.ElapsedMilliseconds < milliseconds)
                Gtk.Application.RunIteration(true);
        }

        // --------------------------------------------------------- priorities

        [Fact]
        public void A_higher_priority_idle_runs_before_a_lower_priority_one()
        {
            // This is the whole reason the priority overloads exist, and it is
            // the one thing about them that can be wrong without looking wrong.
            Run(() =>
            {
                var order = new List<string>();

                GLib.Idle.Add(GLib.Priority.DefaultIdle, () => { order.Add("low"); return false; });
                GLib.Idle.Add(GLib.Priority.HighIdle, () => { order.Add("high"); return false; });

                Drain();

                Assert.Equal(new[] { "high", "low" }, order);
            });
        }

        [Fact]
        public void An_integer_priority_orders_the_same_way_the_enum_does()
        {
            Run(() =>
            {
                var order = new List<string>();

                GLib.Idle.Add(200, () => { order.Add("later"); return false; });
                GLib.Idle.Add(100, () => { order.Add("sooner"); return false; });

                Drain();

                Assert.Equal(new[] { "sooner", "later" }, order);
            });
        }

        // ------------------------------------------------------------- idles

        [Fact]
        public void An_idle_returning_false_runs_exactly_once_however_long_the_loop_turns()
        {
            Run(() =>
            {
                var calls = 0;

                GLib.Idle.Add(() => { calls++; return false; });

                Drain();
                Drain();

                Assert.Equal(1, calls);
            });
        }

        [Fact]
        public void An_idle_can_stop_itself_after_a_chosen_number_of_runs()
        {
            Run(() =>
            {
                var calls = 0;

                GLib.Idle.Add(() => { calls++; return calls < 5; });

                Drain();

                Assert.Equal(5, calls);
            });
        }

        [Fact]
        public void Removing_an_idle_by_id_stops_it_before_it_ever_runs()
        {
            Run(() =>
            {
                var calls = 0;

                var id = GLib.Idle.Add(() => { calls++; return true; });
                GLib.Idle.Remove(id);

                Drain();

                Assert.Equal(0, calls);
            });
        }

        [Fact]
        public void Removing_a_source_that_is_already_gone_reports_that_it_was_not_found()
        {
            Run(() =>
            {
                var id = GLib.Idle.Add(() => false);

                Assert.True(GLib.Source.Remove(id));
                Assert.False(GLib.Source.Remove(id));
            });
        }

        // ---------------------------------------------------------- timeouts

        [Fact]
        public void A_timeout_runs_after_its_interval_and_stops_when_it_returns_false()
        {
            Run(() =>
            {
                var calls = 0;

                GLib.Timeout.Add(5, () => { calls++; return false; });

                PumpUntil(() => calls > 0);
                Drain();

                Assert.Equal(1, calls);
            });
        }

        [Fact]
        public void A_repeating_timeout_runs_until_it_says_to_stop()
        {
            Run(() =>
            {
                var calls = 0;

                GLib.Timeout.Add(1, () => { calls++; return calls < 3; });

                PumpUntil(() => calls >= 3);

                Assert.Equal(3, calls);
            });
        }

        [Fact]
        public void A_shorter_timeout_fires_before_a_longer_one()
        {
            Run(() =>
            {
                var order = new List<string>();

                GLib.Timeout.Add(120, () => { order.Add("slow"); return false; });
                GLib.Timeout.Add(5, () => { order.Add("quick"); return false; });

                PumpUntil(() => order.Count == 2);

                Assert.Equal(new[] { "quick", "slow" }, order);
            });
        }

        [Fact]
        public void Removing_a_timeout_stops_it_firing()
        {
            Run(() =>
            {
                var calls = 0;

                var id = GLib.Timeout.Add(5, () => { calls++; return false; });
                GLib.Timeout.Remove(id);

                // Long enough that it would certainly have fired.
                var clock = Stopwatch.StartNew();
                while (clock.ElapsedMilliseconds < 60)
                    Drain();

                Assert.Equal(0, calls);
            });
        }

        [Fact]
        public void A_timeout_handler_that_throws_does_not_stop_the_loop()
        {
            // A managed exception cannot cross back into C, so the binding hands
            // it to ExceptionManager. The loop has to keep turning afterwards or
            // one bad handler takes the application with it.
            Run(() =>
            {
                var thrown = 0;
                var afterwards = 0;

                GLib.ExceptionManager.UnhandledException += Swallow;
                try
                {
                    GLib.Idle.Add(() => { thrown++; throw new InvalidOperationException("from a handler"); });
                    GLib.Idle.Add(() => { afterwards++; return false; });

                    Drain();

                    Assert.Equal(1, thrown);
                    Assert.Equal(1, afterwards);
                }
                finally
                {
                    GLib.ExceptionManager.UnhandledException -= Swallow;
                }

                static void Swallow(GLib.UnhandledExceptionArgs args) => args.ExitApplication = false;
            });
        }

        // ------------------------------------------------------- MainContext

        [Fact]
        public void The_default_context_reports_whether_work_is_waiting()
        {
            Run(() =>
            {
                Quiesce();

                Assert.False(GLib.MainContext.Pending());

                GLib.Idle.Add(() => false);

                Assert.True(GLib.MainContext.Pending());

                Drain();

                Assert.False(GLib.MainContext.Pending());
            });
        }

        [Fact]
        public void Iterating_the_context_dispatches_one_pending_source()
        {
            Run(() =>
            {
                Quiesce();

                var calls = 0;
                GLib.Idle.Add(() => { calls++; return false; });

                Assert.True(GLib.MainContext.Iteration(false));

                Assert.Equal(1, calls);
            });
        }

        // ------------------------------------------------------ GLib.Source

        [Fact]
        public void A_source_can_be_found_by_the_id_it_was_given()
        {
            // Idle.Add and Timeout.Add return an id and GLib.Source has the
            // properties, but nothing turned one into the other, so almost none
            // of Source was reachable from a source this library created.
            //
            // MainContext.FindSourceById was added for that.
            Run(() =>
            {
                var id = GLib.Idle.Add(() => true);
                try
                {
                    var source = GLib.MainContext.Default.FindSourceById(id);

                    Assert.NotNull(source);
                    Assert.False(source.IsDestroyed);
                }
                finally
                {
                    GLib.Source.Remove(id);
                }
            });
        }

        [Fact]
        public void A_source_reports_the_priority_and_name_it_was_given()
        {
            Run(() =>
            {
                var id = GLib.Idle.Add(GLib.Priority.HighIdle, () => true);
                try
                {
                    var source = GLib.MainContext.Default.FindSourceById(id);

                    Assert.Equal((int) GLib.Priority.HighIdle, source.Priority);

                    source.Name = "a named source";
                    Assert.Equal("a named source", source.Name);

                    source.Priority = (int) GLib.Priority.Low;
                    Assert.Equal((int) GLib.Priority.Low, source.Priority);
                }
                finally
                {
                    GLib.Source.Remove(id);
                }
            });
        }

        [Fact]
        public void Destroying_a_source_marks_it_destroyed_and_stops_it_running()
        {
            Run(() =>
            {
                var calls = 0;
                var id = GLib.Idle.Add(() => { calls++; return true; });

                var source = GLib.MainContext.Default.FindSourceById(id);
                Assert.False(source.IsDestroyed);

                source.Destroy();

                Assert.True(source.IsDestroyed);

                Drain();

                Assert.Equal(0, calls);
            });
        }

        [Fact]
        public void A_source_reports_the_context_it_is_attached_to()
        {
            Run(() =>
            {
                var id = GLib.Idle.Add(() => true);
                try
                {
                    var source = GLib.MainContext.Default.FindSourceById(id);

                    Assert.NotNull(source.Context);
                    Assert.Equal(GLib.MainContext.Default.Handle, source.Context.Handle);
                }
                finally
                {
                    GLib.Source.Remove(id);
                }
            });
        }

        [Fact]
        public void CanRecurse_is_reported_back_as_it_was_set()
        {
            Run(() =>
            {
                var id = GLib.Idle.Add(() => true);
                try
                {
                    var source = GLib.MainContext.Default.FindSourceById(id);

                    Assert.False(source.CanRecurse);

                    source.CanRecurse = true;

                    Assert.True(source.CanRecurse);
                }
                finally
                {
                    GLib.Source.Remove(id);
                }
            });
        }

        // -------------------------------------------------------- MainLoop

        [Fact]
        public void A_main_loop_runs_until_something_quits_it()
        {
            // Application.Run is built on this, and it is what replaced gtk_main
            // in the Gtk 4 port -- so it is worth a test that actually enters
            // and leaves a loop rather than trusting the shape of the code.
            Run(() =>
            {
                var loop = new GLib.MainLoop();
                var ran = false;

                GLib.Idle.Add(() =>
                {
                    ran = true;
                    loop.Quit();
                    return false;
                });

                Assert.False(loop.IsRunning);

                loop.Run();

                Assert.True(ran);
                Assert.False(loop.IsRunning);
            });
        }

        [Fact]
        public void A_main_loop_reports_that_it_is_running_from_inside_itself()
        {
            Run(() =>
            {
                var loop = new GLib.MainLoop();
                bool? runningWhileInside = null;

                GLib.Idle.Add(() =>
                {
                    runningWhileInside = loop.IsRunning;
                    loop.Quit();
                    return false;
                });

                loop.Run();

                Assert.True(runningWhileInside);
            });
        }
    }
}
