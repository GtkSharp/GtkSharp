using System;
using System.Collections.Generic;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Connecting and disconnecting handlers: what <c>SignalClosure</c> does with
    /// the handler ids GObject hands back.
    /// </summary>
    /// <remarks>
    /// <c>SignalTests</c> covers that a handler runs and that its arguments
    /// arrive. This covers the bookkeeping around it — several handlers on one
    /// signal, removing one of them, removing one twice, and the order they run
    /// in — because that is what <c>SignalClosure</c> is for and none of it was
    /// exercised.
    ///
    /// None of these can demonstrate the defect that prompted them: the handler
    /// id is a <c>gulong</c>, which is 64 bits on Linux and macOS, and the
    /// delegates declared <c>uint</c>. Ids are small sequential counters, so the
    /// value has always fitted. The declared ABI was still wrong, and it
    /// contradicted GapiCodegen, which has mapped <c>gulong</c> to
    /// <c>UIntPtr</c> since the mono era. These tests pin the behaviour the fix
    /// had to preserve.
    /// </remarks>
    public class SignalLifetimeTests : GtkTestBase
    {
        public SignalLifetimeTests(GtkFixture fixture) : base(fixture) { }

        [Fact]
        public void Removing_one_handler_leaves_the_others_connected()
        {
            // Each += makes its own closure with its own id, so removing one has
            // to find that one. Getting the id wrong would either remove nothing
            // or remove somebody else's handler.
            Run(() =>
            {
                var button = new Gtk.Button();
                var fired = new List<string>();

                EventHandler first = (o, e) => fired.Add("first");
                EventHandler second = (o, e) => fired.Add("second");
                EventHandler third = (o, e) => fired.Add("third");

                button.Clicked += first;
                button.Clicked += second;
                button.Clicked += third;

                button.Clicked -= second;

                GLib.Signal.Emit(button, "clicked");

                Assert.Equal(new[] { "first", "third" }, fired);
            });
        }

        [Fact]
        public void Handlers_run_in_the_order_they_were_connected()
        {
            Run(() =>
            {
                var button = new Gtk.Button();
                var fired = new List<int>();

                for (int i = 0; i < 5; i++)
                {
                    int n = i;
                    button.Clicked += (o, e) => fired.Add(n);
                }

                GLib.Signal.Emit(button, "clicked");

                Assert.Equal(new[] { 0, 1, 2, 3, 4 }, fired);
            });
        }

        [Fact]
        public void Removing_a_handler_twice_is_harmless()
        {
            // The second removal has no closure to disconnect. Disconnect() guards
            // on the id being set and on the handler still being connected, and
            // both halves of that guard matter: g_signal_handler_disconnect on a
            // stale id warns loudly and may hit an unrelated handler.
            Run(() =>
            {
                var button = new Gtk.Button();
                int calls = 0;

                EventHandler handler = (o, e) => calls++;

                button.Clicked += handler;
                button.Clicked -= handler;
                button.Clicked -= handler;

                GLib.Signal.Emit(button, "clicked");

                Assert.Equal(0, calls);
            });
        }

        [Fact]
        public void Removing_a_handler_that_was_never_connected_is_harmless()
        {
            Run(() =>
            {
                var button = new Gtk.Button();
                int calls = 0;

                button.Clicked -= (o, e) => calls++;

                GLib.Signal.Emit(button, "clicked");

                Assert.Equal(0, calls);
            });
        }

        [Fact]
        public void The_same_handler_added_twice_runs_twice_and_comes_off_one_at_a_time()
        {
            Run(() =>
            {
                var button = new Gtk.Button();
                int calls = 0;

                EventHandler handler = (o, e) => calls++;

                button.Clicked += handler;
                button.Clicked += handler;

                GLib.Signal.Emit(button, "clicked");
                Assert.Equal(2, calls);

                calls = 0;
                button.Clicked -= handler;

                GLib.Signal.Emit(button, "clicked");
                Assert.Equal(1, calls);
            });
        }

        [Fact]
        public void Connecting_and_disconnecting_many_handlers_keeps_them_distinct()
        {
            // Ids come from a counter that never repeats within a process, so a
            // hundred connect/disconnect cycles walk it forward. If an id were
            // being truncated or reused, the wrong handler would come off and the
            // survivors would not match.
            Run(() =>
            {
                var button = new Gtk.Button();
                var handlers = new List<EventHandler>();
                var fired = new List<int>();

                for (int i = 0; i < 100; i++)
                {
                    int n = i;
                    EventHandler handler = (o, e) => fired.Add(n);
                    handlers.Add(handler);
                    button.Clicked += handler;
                }

                // Take off every even-numbered one.
                for (int i = 0; i < 100; i += 2)
                    button.Clicked -= handlers[i];

                GLib.Signal.Emit(button, "clicked");

                var expected = new List<int>();
                for (int i = 1; i < 100; i += 2)
                    expected.Add(i);

                Assert.Equal(expected, fired);
            });
        }

        [Fact]
        public void Handlers_on_one_object_do_not_fire_for_another()
        {
            Run(() =>
            {
                var first = new Gtk.Button();
                var second = new Gtk.Button();
                int firstCalls = 0, secondCalls = 0;

                first.Clicked += (o, e) => firstCalls++;
                second.Clicked += (o, e) => secondCalls++;

                GLib.Signal.Emit(first, "clicked");

                Assert.Equal(1, firstCalls);
                Assert.Equal(0, secondCalls);
            });
        }

        [Fact]
        public void Two_signals_on_one_object_are_kept_apart()
        {
            // One object, two closures, two ids. Disconnecting one must not touch
            // the other.
            Run(() =>
            {
                var entry = new Gtk.Entry();
                int changed = 0, activated = 0;

                EventHandler onChanged = (o, e) => changed++;
                EventHandler onActivated = (o, e) => activated++;

                entry.Changed += onChanged;
                entry.Activated += onActivated;

                entry.Changed -= onChanged;

                entry.Text = "typed";
                GLib.Signal.Emit(entry, "activate");

                Assert.Equal(0, changed);
                Assert.Equal(1, activated);
            });
        }

        [Fact]
        public void A_handler_survives_a_collection_while_the_object_is_alive()
        {
            // The closure holds a GCHandle; if it were collectible the signal
            // would arrive at freed memory rather than simply not firing.
            Run(() =>
            {
                var button = new Gtk.Button();
                int calls = 0;

                button.Clicked += (o, e) => calls++;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                GLib.Signal.Emit(button, "clicked");

                Assert.Equal(1, calls);
                GC.KeepAlive(button);
            });
        }

        [Fact]
        public void A_reconnected_handler_fires_again()
        {
            // Disconnect clears the id; connecting again has to get a fresh one
            // rather than reusing the cleared value.
            Run(() =>
            {
                var button = new Gtk.Button();
                int calls = 0;

                EventHandler handler = (o, e) => calls++;

                button.Clicked += handler;
                button.Clicked -= handler;
                button.Clicked += handler;

                GLib.Signal.Emit(button, "clicked");

                Assert.Equal(1, calls);
            });
        }
    }
}
