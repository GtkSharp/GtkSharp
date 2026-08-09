using System;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.Source</c> beyond what <c>MainLoopTests</c> reaches: ready time,
    /// child sources, and the custom-GSource constructor that cannot work.
    /// </summary>
    /// <remarks>
    /// <c>MainLoopTests</c> covers the sources GLib makes for you — idles and
    /// timeouts — their priorities, ids and destruction. This covers the
    /// scheduling controls on a source once you hold one, which is what an
    /// application reaches for when a timeout is not quite the right shape.
    ///
    /// A source is obtained the way an application would: register a timeout and
    /// look it up by id. Constructing one directly is the broken path, and has a
    /// test of its own.
    /// </remarks>
    public class SourceLifetimeTests : GtkTestBase
    {
        public SourceLifetimeTests(GtkFixture fixture) : base(fixture) { }

        static bool PumpUntil(Func<bool> condition, int timeoutMs = 3000)
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            while (!condition() && clock.ElapsedMilliseconds < timeoutMs)
            {
                if (Gtk.Application.EventsPending())
                    Gtk.Application.RunIteration(false);
                else
                    System.Threading.Thread.Sleep(1);
            }

            return condition();
        }

        /// <summary>A live source, reached the way an application reaches one.</summary>
        static GLib.Source SourceOfTimeout(uint interval, GLib.TimeoutHandler handler, out uint id)
        {
            id = GLib.Timeout.Add(interval, handler);
            var source = GLib.MainContext.Default.FindSourceById(id);

            Assert.NotNull(source);
            return source;
        }

        // ------------------------------------------------------------ ready time

        [Fact]
        public void Setting_the_ready_time_to_now_fires_a_timeout_that_had_not_come_due()
        {
            // ReadyTime overrides the source's own scheduling: zero means "ready
            // immediately". The oracle is that a ten-second timeout runs at once,
            // which cannot happen by waiting.
            Run(() =>
            {
                bool fired = false;

                var source = SourceOfTimeout(10_000, () => { fired = true; return false; }, out _);

                Assert.False(fired, "a ten-second timeout has not come due yet");

                source.ReadyTime = 0;

                Assert.True(PumpUntil(() => fired, 1000),
                            "setting ReadyTime to 0 should make it dispatch at once");
            });
        }

        [Fact]
        public void A_ready_time_in_the_future_does_not_fire_yet()
        {
            // The control. Without it, the test above could be reporting a
            // timeout that fires regardless of ReadyTime.
            Run(() =>
            {
                bool fired = false;

                var source = SourceOfTimeout(10_000, () => { fired = true; return false; }, out uint id);

                // Monotonic microseconds, taken from the source itself, a
                // minute out: well beyond the pump below.
                source.ReadyTime = source.Time + 60_000_000;

                PumpUntil(() => false, 150);

                Assert.False(fired, "a ready time a minute out should not have arrived");

                GLib.Source.Remove(id);
            });
        }

        [Fact]
        public void The_ready_time_reads_back_as_it_was_set()
        {
            Run(() =>
            {
                var source = SourceOfTimeout(10_000, () => false, out uint id);

                source.ReadyTime = 0;
                Assert.Equal(0, source.ReadyTime);

                source.ReadyTime = -1;               // -1 means "never, on its own"
                Assert.Equal(-1, source.ReadyTime);

                GLib.Source.Remove(id);
            });
        }

        [Fact]
        public void A_sources_time_advances_with_the_loop()
        {
            // g_source_get_time is the monotonic clock the loop caches per
            // iteration, so it must move between iterations and never go
            // backwards.
            Run(() =>
            {
                var source = SourceOfTimeout(10_000, () => false, out uint id);

                long first = source.Time;
                Assert.True(first > 0, "a source attached to a context has a time");

                System.Threading.Thread.Sleep(20);
                PumpUntil(() => false, 50);

                Assert.True(source.Time >= first, "monotonic time must not go backwards");

                GLib.Source.Remove(id);
            });
        }

        // --------------------------------------------------------- child sources

        [Fact]
        public void A_child_source_cannot_be_attached_because_none_can_be_made()
        {
            // AddChildSource is bound and works in C, and is unreachable here.
            //
            // g_source_add_child_source requires a source that is not attached
            // to a context, and the only way to obtain one is g_source_new --
            // the constructor below, which cannot be used. Everything else the
            // binding offers (Idle, Timeout, IO watches) attaches on creation,
            // and detaching means Destroy, after which the source is dead.
            //
            // So this is not a gap in the tests: it is the same defect, seen
            // from the other end. Fixing GLib.SourceFuncs makes both usable.
            Run(() =>
            {
                var parent = SourceOfTimeout(10_000, () => false, out uint parentId);

                Assert.NotNull(parent);
                Assert.False(parent.IsDestroyed);

                // The only route to an unattached source refuses.
                Assert.Throws<NotSupportedException>(
                    () => new GLib.Source(GLib.SourceFuncs.Zero, 0));

                GLib.Source.Remove(parentId);
            });
        }

        // ------------------------------------------- the constructor that cannot

        [Fact]
        public void Building_a_source_from_SourceFuncs_refuses_rather_than_corrupting_the_loop()
        {
            // GLib.SourceFuncs binds only closure_callback and closure_marshal.
            // The C structure is six function pointers, of which prepare, check,
            // dispatch and finalize come first -- so the two bound fields sit in
            // the wrong slots and GLib reads dispatch from past the end of a
            // sixteen-byte allocation. That allocation was also freed on the way
            // out of the constructor, while g_source_new keeps the pointer for
            // the source's lifetime.
            //
            // Either fault alone corrupts the process, so this cannot be tested
            // by calling it and seeing what happens -- an aborted host prints
            // "Passed!" with a smaller total. It now throws, which is a thing a
            // test can assert.
            Run(() =>
            {
                var error = Assert.Throws<NotSupportedException>(
                    () => new GLib.Source(GLib.SourceFuncs.Zero, 0));

                Assert.Contains("prepare", error.Message);
                Assert.Contains("GLib.Idle", error.Message);
            });
        }

        [Fact]
        public void The_sources_GLib_makes_for_you_still_work()
        {
            // The counterpart to the refusal above: the supported route has to
            // keep working, or the message would be sending people nowhere.
            Run(() =>
            {
                int idles = 0, timeouts = 0;

                GLib.Idle.Add(() => { idles++; return false; });
                GLib.Timeout.Add(1, () => { timeouts++; return false; });

                Assert.True(PumpUntil(() => idles == 1 && timeouts == 1),
                            $"idle ran {idles} times, timeout {timeouts}");
            });
        }
    }
}
