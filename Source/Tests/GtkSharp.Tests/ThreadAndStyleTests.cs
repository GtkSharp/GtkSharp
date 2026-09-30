using System;
using System.Threading;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>Gtk.ThreadNotify</c> and the <c>Gtk.StyleContext</c> render helpers —
    /// two hand-written files with no coverage at all.
    /// </summary>
    /// <remarks>
    /// Both are the kind of code the null-delegate trap hides in:
    /// <c>StyleContext</c>'s helpers forward to <c>Gtk.Render</c>, several of
    /// whose C functions were removed outright in Gtk 4, and a removed one is a
    /// <c>NullReferenceException</c> at the call site rather than a link error.
    /// So they are tested by drawing and reading the pixels back, not by calling
    /// them and seeing whether anything was thrown.
    /// </remarks>
    public class ThreadAndStyleTests : GtkTestBase
    {
        public ThreadAndStyleTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------- ThreadNotify

        /// <summary>Runs the main loop until <paramref name="condition"/> holds,
        /// so a test can wait on an idle callback rather than guess a sleep.</summary>
        static bool PumpUntil(Func<bool> condition, int timeoutMs = 3000)
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

        [Fact]
        public void A_wakeup_from_another_thread_runs_the_delegate_on_the_gtk_thread()
        {
            // The whole point of ThreadNotify: Gtk may only be touched from the
            // thread that called gtk_init, so work raised elsewhere has to arrive
            // here. Comparing thread ids is the only oracle that actually shows
            // it -- a delegate that ran on the worker would satisfy any test that
            // merely counted invocations.
            Run(() =>
            {
                int gtkThread = Thread.CurrentThread.ManagedThreadId;
                int ranOn = -1;
                var done = new ManualResetEventSlim();

                using var notify = new Gtk.ThreadNotify(() =>
                {
                    ranOn = Thread.CurrentThread.ManagedThreadId;
                    done.Set();
                });

                int wokeFrom = -1;
                var worker = new Thread(() =>
                {
                    wokeFrom = Thread.CurrentThread.ManagedThreadId;
                    notify.WakeupMain();
                });

                worker.Start();
                worker.Join();

                Assert.True(PumpUntil(() => done.IsSet), "the ready delegate should have run");

                Assert.Equal(gtkThread, ranOn);
                Assert.NotEqual(gtkThread, wokeFrom);
            });
        }

        [Fact]
        public void Several_wakeups_before_the_loop_runs_collapse_into_one_call()
        {
            // The reason ThreadNotify exists rather than a bare Idle.Add: a
            // producer thread may signal far faster than the main loop drains,
            // and the delegate is meant to run once per batch, not once per
            // signal. WakeupMain is a no-op while a notification is outstanding.
            Run(() =>
            {
                int calls = 0;

                using var notify = new Gtk.ThreadNotify(() => calls++);

                for (int i = 0; i < 50; i++)
                    notify.WakeupMain();

                Assert.True(PumpUntil(() => calls > 0), "the ready delegate should have run");

                // Drain anything else the loop had queued, so a second call would
                // have shown up by now.
                PumpUntil(() => false, 50);

                Assert.Equal(1, calls);
            });
        }

        [Fact]
        public void A_wakeup_after_the_delegate_has_run_is_delivered_again()
        {
            // Coalescing must not latch: the flag is cleared as the delegate is
            // entered, so the next batch is a new notification.
            Run(() =>
            {
                int calls = 0;

                using var notify = new Gtk.ThreadNotify(() => calls++);

                notify.WakeupMain();
                Assert.True(PumpUntil(() => calls == 1), "first wakeup should arrive");

                notify.WakeupMain();
                Assert.True(PumpUntil(() => calls == 2), "second wakeup should arrive too");
            });
        }

        [Fact]
        public void Closing_a_notifier_stops_a_wakeup_already_in_flight()
        {
            // Close is what an application calls while shutting down, quite
            // possibly with a notification already queued on the main loop. The
            // delegate must not run then: it is the disposed object's.
            Run(() =>
            {
                int calls = 0;
                var notify = new Gtk.ThreadNotify(() => calls++);

                notify.WakeupMain();     // queued, but not yet dispatched
                notify.Close();

                PumpUntil(() => false, 100);

                Assert.Equal(0, calls);
            });
        }

        // -------------------------------------------------------- StyleContext

        /// <summary>A style context carrying <paramref name="css"/>, taken from a
        /// real widget because that is the only way to get one.</summary>
        static Gtk.StyleContext StyledContext(string css, out Gtk.Widget owner)
        {
            var provider = new Gtk.CssProvider();
            provider.LoadFromData(css);

            var label = new Gtk.Label("styled");
            var context = label.StyleContext;
            context.AddProvider(provider, Gtk.StyleProviderPriority.Application);

            owner = label;
            return context;
        }

        /// <summary>Rasterises <paramref name="draw"/> and reports whether any
        /// pixel was painted, plus the colour of one that was.</summary>
        static (bool painted, byte b, byte g, byte r, byte a) Rasterise(Action<Cairo.Context> draw, int size = 16)
        {
            using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, size, size);
            using (var cr = new Cairo.Context(surface))
                draw(cr);

            surface.Flush();

            var data = surface.Data;
            int stride = surface.Stride;

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    int i = y * stride + x * 4;
                    if (data[i + 3] != 0)
                        return (true, data[i], data[i + 1], data[i + 2], data[i + 3]);
                }

            return (false, 0, 0, 0, 0);
        }

        [Fact]
        public void Rendering_a_background_paints_the_colour_the_css_asked_for()
        {
            // gtk_render_background still exists in Gtk 4 (deprecated since
            // 4.10), and the CSS is the oracle: the test chose the colour, so a
            // themed default cannot be mistaken for success.
            Run(() =>
            {
                var context = StyledContext("label { background-color: rgb(255,0,0); }", out var owner);

                var result = Rasterise(cr => context.RenderBackground(cr, 0, 0, 16, 16));

                Assert.True(result.painted, "a background should have been painted");
                Assert.Equal(255, result.r);
                Assert.Equal(0, result.g);
                Assert.Equal(0, result.b);

                GC.KeepAlive(owner);
            });
        }

        [Fact]
        public void Rendering_a_background_of_nothing_paints_nothing()
        {
            // The contrast that makes the test above mean something: with a
            // transparent background the same call leaves the surface alone,
            // so "painted" is really reporting the CSS and not the call.
            Run(() =>
            {
                var context = StyledContext("label { background-color: transparent; }", out var owner);

                var result = Rasterise(cr => context.RenderBackground(cr, 0, 0, 16, 16));

                Assert.False(result.painted);

                GC.KeepAlive(owner);
            });
        }

        [Fact]
        public void Rendering_a_frame_paints_the_border_the_css_asked_for()
        {
            Run(() =>
            {
                var context = StyledContext(
                    "label { border: 4px solid rgb(0,0,255); background-color: transparent; }",
                    out var owner);

                var result = Rasterise(cr => context.RenderFrame(cr, 0, 0, 16, 16));

                Assert.True(result.painted, "a border should have been painted");
                Assert.Equal(255, result.b);
                Assert.Equal(0, result.r);

                GC.KeepAlive(owner);
            });
        }

        [Fact]
        public void A_frame_with_no_border_width_paints_nothing()
        {
            Run(() =>
            {
                var context = StyledContext(
                    "label { border: 0px solid rgb(0,0,255); background-color: transparent; }",
                    out var owner);

                Assert.False(Rasterise(cr => context.RenderFrame(cr, 0, 0, 16, 16)).painted);

                GC.KeepAlive(owner);
            });
        }

        [Fact]
        public void Rendering_a_line_paints_between_the_two_points()
        {
            Run(() =>
            {
                var context = StyledContext("label { color: rgb(0,255,0); }", out var owner);

                var result = Rasterise(cr => context.RenderLine(cr, 0, 8, 16, 8));

                Assert.True(result.painted, "a line should have been painted");

                GC.KeepAlive(owner);
            });
        }

        [Fact]
        public void Rendering_a_layout_paints_the_text_and_an_empty_one_paints_nothing()
        {
            // gtk_render_layout is how a custom widget draws text through the
            // theme. The empty layout is the control: it proves the pixels came
            // from the string rather than from the call happening at all.
            Run(() =>
            {
                var context = StyledContext("label { color: rgb(255,0,255); }", out var owner);

                using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 64, 32);
                using var measuring = new Cairo.Context(surface);

                var text = Pango.CairoHelper.CreateLayout(measuring);
                text.FontDescription = Pango.FontDescription.FromString("Sans 12");
                text.SetText("Hg");

                var empty = Pango.CairoHelper.CreateLayout(measuring);
                empty.FontDescription = Pango.FontDescription.FromString("Sans 12");
                empty.SetText("");

                Assert.True(Rasterise(cr => context.RenderLayout(cr, 0, 0, text), 64).painted,
                            "text should have been painted");
                Assert.False(Rasterise(cr => context.RenderLayout(cr, 0, 0, empty), 64).painted,
                             "an empty layout should paint nothing");

                GC.KeepAlive(owner);
            });
        }

        [Fact]
        public void A_provider_added_at_a_higher_priority_wins()
        {
            // Which of two rules applies is the question a stylesheet actually
            // raises, and it is answered by the priority the provider was added
            // with rather than by document order.
            Run(() =>
            {
                var label = new Gtk.Label("styled");
                var context = label.StyleContext;

                var weak = new Gtk.CssProvider();
                weak.LoadFromData("label { background-color: rgb(255,0,0); }");

                var strong = new Gtk.CssProvider();
                strong.LoadFromData("label { background-color: rgb(0,0,255); }");

                context.AddProvider(weak, Gtk.StyleProviderPriority.Application);
                context.AddProvider(strong, Gtk.StyleProviderPriority.User);

                var result = Rasterise(cr => context.RenderBackground(cr, 0, 0, 16, 16));

                Assert.True(result.painted);
                Assert.Equal(255, result.b);
                Assert.Equal(0, result.r);

                GC.KeepAlive(label);
            });
        }

        [Fact]
        public void Removing_a_provider_takes_its_styling_with_it()
        {
            Run(() =>
            {
                var label = new Gtk.Label("styled");
                var context = label.StyleContext;

                var provider = new Gtk.CssProvider();
                provider.LoadFromData("label { background-color: rgb(255,0,0); }");

                context.AddProvider(provider, Gtk.StyleProviderPriority.Application);
                Assert.True(Rasterise(cr => context.RenderBackground(cr, 0, 0, 16, 16)).painted);

                context.RemoveProvider(provider);
                Assert.False(Rasterise(cr => context.RenderBackground(cr, 0, 0, 16, 16)).painted);

                GC.KeepAlive(label);
            });
        }
    }
}
