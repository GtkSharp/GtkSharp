using System;
using System.Collections.Generic;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>Gtk.DrawingArea</c> and the custom-drawing path: a managed draw
    /// function, called by Gtk, painting through Cairo, ending up in what the
    /// widget renders.
    /// </summary>
    /// <remarks>
    /// <c>Docs/getting-started.md</c> has a whole section on this and the suite
    /// had no mention of <c>DrawingArea</c> or <c>SetDrawFunc</c> at all. It is
    /// the most common thing an application does beyond arranging widgets, and it
    /// crosses every boundary in the binding at once: a delegate marshalled into
    /// Gtk, invoked from native code, handed a wrapped <c>Cairo.Context</c> it did
    /// not create.
    ///
    /// The oracle is the rendered pixels rather than the fact that the delegate
    /// ran. A draw function that is called but whose Cairo context is wrong paints
    /// nothing, and counting invocations would call that a pass.
    /// </remarks>
    public class DrawingAreaTests : GtkTestBase
    {
        public DrawingAreaTests(GtkFixture fixture) : base(fixture) { }

        readonly struct Pixel
        {
            public Pixel(byte b, byte g, byte r, byte a) { B = b; G = g; R = r; A = a; }
            public byte B { get; }
            public byte G { get; }
            public byte R { get; }
            public byte A { get; }
            public override string ToString() => $"(b={B} g={G} r={R} a={A})";
        }

        /// <summary>
        /// Rasterises what <paramref name="widget"/> renders, by way of the same
        /// scene graph Gtk itself draws through: a WidgetPaintable snapshots the
        /// widget into a GskRenderNode, and the node is drawn onto an image
        /// surface. This is the only way to read a widget's own painting back;
        /// the draw function's Cairo context belongs to Gtk's surface.
        /// </summary>
        static Func<int, int, Pixel> Render(Gtk.Widget widget, int width, int height)
        {
            var paintable = new Gtk.WidgetPaintable(widget);
            var snapshot = new Gtk.Snapshot();

            paintable.Snapshot(snapshot, width, height);

            var node = snapshot.ToNode();

            using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, width, height);
            if (node != null)
            {
                using var cr = new Cairo.Context(surface);
                node.Draw(cr);
            }

            surface.Flush();

            var data = (byte[]) surface.Data.Clone();
            int stride = surface.Stride;

            return (x, y) =>
            {
                int i = y * stride + x * 4;
                return new Pixel(data[i], data[i + 1], data[i + 2], data[i + 3]);
            };
        }

        static bool PumpUntil(Func<bool> condition, int timeoutMs = 5000)
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

        /// <summary>A drawing area of a known size, shown, with
        /// <paramref name="draw"/> installed. The window is returned so the caller
        /// can destroy it.</summary>
        static Gtk.Window Shown(Gtk.DrawingArea area, Gtk.DrawingAreaDrawFunc draw, int size = 16)
        {
            area.ContentWidth = size;
            area.ContentHeight = size;
            area.DrawFunc = draw;

            var window = new Gtk.Window { DefaultWidth = size, DefaultHeight = size };
            window.Child = area;
            window.Present();

            return window;
        }

        // ------------------------------------------------------- the draw call

        [Fact]
        public void The_draw_function_is_handed_the_area_and_the_size_it_was_given()
        {
            // Four marshalled arguments, and three of them have been wrong in this
            // binding's history for one call or another: the instance, a wrapped
            // Cairo.Context the callee does not own, and two ints.
            Run(() =>
            {
                var area = new Gtk.DrawingArea();

                Gtk.DrawingArea seenArea = null;
                Cairo.Context seenContext = null;
                int seenWidth = -1, seenHeight = -1;

                using var window = Shown(area, (a, cr, w, h) =>
                {
                    seenArea = a;
                    seenContext = cr;
                    seenWidth = w;
                    seenHeight = h;
                });

                Assert.True(PumpUntil(() => seenWidth != -1), "the draw function should have been called");

                Assert.Same(area, seenArea);
                Assert.NotNull(seenContext);

                // The size is the *allocation*, not ContentWidth/ContentHeight.
                // Those are a natural-size request, and a window stretches its
                // child past them -- asking for 16 here and being handed 188 is
                // the widget filling the smallest window the display will make.
                // Asserting the requested size would be asserting a fact about
                // the window manager.
                Assert.Equal(area.AllocatedWidth, seenWidth);
                Assert.Equal(area.AllocatedHeight, seenHeight);
                Assert.True(seenWidth > 0 && seenHeight > 0,
                            $"the area should have real space, got {seenWidth}x{seenHeight}");

                window.Destroy();
            });
        }

        [Fact]
        public void What_the_draw_function_paints_is_what_the_widget_renders()
        {
            // The end-to-end assertion: a managed delegate, called from native
            // code, drawing through a wrapped Cairo context, into pixels read back
            // out of the widget's own render node.
            Run(() =>
            {
                var area = new Gtk.DrawingArea();

                bool drawn = false;
                using var window = Shown(area, (a, cr, w, h) =>
                {
                    cr.SetSourceRGBA(1, 0, 0, 1);
                    cr.Rectangle(0, 0, w / 2.0, h);
                    cr.Fill();
                    drawn = true;
                });

                Assert.True(PumpUntil(() => drawn), "the draw function should have been called");

                var pixel = Render(area, 16, 16);

                Assert.Equal(255, pixel(2, 8).R);       // the half it filled
                Assert.Equal(255, pixel(2, 8).A);
                Assert.Equal(0, pixel(13, 8).A);        // and the half it did not
            });
        }

        [Fact]
        public void A_draw_function_that_paints_nothing_leaves_the_widget_empty()
        {
            // The control. Without it, "the pixel is red" could be reporting the
            // theme, the window background, or anything else in the snapshot.
            Run(() =>
            {
                var area = new Gtk.DrawingArea();

                bool called = false;
                using var window = Shown(area, (a, cr, w, h) => called = true);

                Assert.True(PumpUntil(() => called), "the draw function should still be called");

                var pixel = Render(area, 16, 16);

                Assert.Equal(0, pixel(2, 8).A);
                Assert.Equal(0, pixel(13, 8).A);
            });
        }

        [Fact]
        public void Replacing_the_draw_function_replaces_what_is_drawn()
        {
            // The second delegate has to reach Gtk *and* the first has to stop
            // being called -- a destroy-notify that fired early would take the
            // second one down with it, and one that never fires leaks the first.
            Run(() =>
            {
                var area = new Gtk.DrawingArea();

                int firstCalls = 0;

                using var window = Shown(area, (a, cr, w, h) =>
                {
                    firstCalls++;
                    cr.SetSourceRGBA(1, 0, 0, 1);
                    cr.Rectangle(0, 0, w, h);
                    cr.Fill();
                });

                Assert.True(PumpUntil(() => firstCalls > 0), "the first function should run");
                Assert.Equal(255, Render(area, 16, 16)(8, 8).R);

                int callsBefore = firstCalls;
                bool secondCalled = false;

                area.DrawFunc = (a, cr, w, h) =>
                {
                    secondCalled = true;
                    cr.SetSourceRGBA(0, 0, 1, 1);
                    cr.Rectangle(0, 0, w, h);
                    cr.Fill();
                };

                area.QueueDraw();

                Assert.True(PumpUntil(() => secondCalled), "the second function should run");

                var pixel = Render(area, 16, 16);
                Assert.Equal(255, pixel(8, 8).B);
                Assert.Equal(0, pixel(8, 8).R);

                Assert.Equal(callsBefore, firstCalls);   // and the first one stopped

                window.Destroy();
            });
        }

        [Fact]
        public void Queueing_a_draw_asks_for_the_function_to_run_again()
        {
            Run(() =>
            {
                var area = new Gtk.DrawingArea();
                int calls = 0;

                using var window = Shown(area, (a, cr, w, h) => calls++);

                Assert.True(PumpUntil(() => calls > 0), "the first draw should happen");
                int before = calls;

                area.QueueDraw();

                Assert.True(PumpUntil(() => calls > before),
                            $"QueueDraw should provoke another draw; still {calls}");

                window.Destroy();
            });
        }

        [Fact]
        public void The_content_size_is_the_size_the_area_asks_for()
        {
            // ContentWidth/Height are what a drawing area reports as its natural
            // size, which is how it gets any space at all inside a container.
            Run(() =>
            {
                var area = new Gtk.DrawingArea { ContentWidth = 120, ContentHeight = 40 };

                Assert.Equal(120, area.ContentWidth);
                Assert.Equal(40, area.ContentHeight);

                area.Measure(Gtk.Orientation.Horizontal, -1, out _, out int naturalWidth, out _, out _);
                area.Measure(Gtk.Orientation.Vertical, -1, out _, out int naturalHeight, out _, out _);

                Assert.Equal(120, naturalWidth);
                Assert.Equal(40, naturalHeight);
            });
        }

        [Fact]
        public void Resizing_the_area_announces_the_new_size()
        {
            Run(() =>
            {
                var area = new Gtk.DrawingArea();

                var sizes = new List<(int, int)>();
                area.Resize += (o, args) => sizes.Add((args.Width, args.Height));

                using var window = Shown(area, (a, cr, w, h) => { }, 24);

                Assert.True(PumpUntil(() => sizes.Count > 0), "resize should be announced");

                // The signal carries the allocation the draw function will be
                // given, so those two have to agree.
                int drawWidth = -1;
                area.DrawFunc = (a, cr, w, h) => drawWidth = w;
                area.QueueDraw();

                Assert.True(PumpUntil(() => drawWidth != -1), "the area should redraw");
                Assert.Equal(sizes[sizes.Count - 1].Item1, drawWidth);

                window.Destroy();
            });
        }

        // ------------------------------------------- drawing through the context

        [Fact]
        public void The_draw_function_can_use_the_full_cairo_api_on_the_context_it_is_given()
        {
            // The context is Gtk's, wrapped rather than constructed here, so the
            // question is whether it behaves like one this binding made. Transform
            // and clip are the two pieces of state most likely to be lost in the
            // wrapping.
            Run(() =>
            {
                var area = new Gtk.DrawingArea();

                bool drawn = false;
                using var window = Shown(area, (a, cr, w, h) =>
                {
                    cr.Save();
                    cr.Translate(w / 2.0, 0);           // push the fill to the right half
                    cr.SetSourceRGBA(0, 1, 0, 1);
                    cr.Rectangle(0, 0, w / 2.0, h);
                    cr.Fill();
                    cr.Restore();
                    drawn = true;
                });

                Assert.True(PumpUntil(() => drawn), "the draw function should have been called");

                var pixel = Render(area, 16, 16);

                Assert.Equal(0, pixel(2, 8).A);         // vacated by the translation
                Assert.Equal(255, pixel(13, 8).G);      // and filled at the far side
            });
        }

        [Fact]
        public void A_clip_set_by_the_draw_function_is_honoured()
        {
            Run(() =>
            {
                var area = new Gtk.DrawingArea();

                bool drawn = false;
                using var window = Shown(area, (a, cr, w, h) =>
                {
                    cr.Rectangle(0, 0, w / 2.0, h);
                    cr.Clip();

                    cr.SetSourceRGBA(1, 0, 0, 1);
                    cr.Rectangle(0, 0, w, h);           // asks for all of it
                    cr.Fill();
                    drawn = true;
                });

                Assert.True(PumpUntil(() => drawn), "the draw function should have been called");

                var pixel = Render(area, 16, 16);

                Assert.Equal(255, pixel(2, 8).R);       // inside the clip
                Assert.Equal(0, pixel(13, 8).A);        // outside it
            });
        }

        [Fact]
        public void Text_drawn_through_pango_reaches_the_widget_too()
        {
            // The other half of custom drawing: Pango lays the text out, Cairo
            // renders it, and the whole thing has to arrive through a context the
            // draw function was handed rather than one it made.
            Run(() =>
            {
                var area = new Gtk.DrawingArea();

                bool drawn = false;
                using var window = Shown(area, (a, cr, w, h) =>
                {
                    var layout = Pango.CairoHelper.CreateLayout(cr);
                    layout.FontDescription = Pango.FontDescription.FromString("Sans 10");
                    layout.SetText("Hg");

                    cr.SetSourceRGBA(0, 0, 1, 1);
                    cr.MoveTo(0, 0);
                    Pango.CairoHelper.ShowLayout(cr, layout);
                    drawn = true;
                }, 32);

                Assert.True(PumpUntil(() => drawn), "the draw function should have been called");

                var pixel = Render(area, 32, 32);

                bool anyInk = false;
                for (int y = 0; y < 32 && !anyInk; y++)
                    for (int x = 0; x < 32 && !anyInk; x++)
                        anyInk = pixel(x, y).A != 0;

                Assert.True(anyInk, "the text should have left ink on the widget");
            });
        }

        [Fact]
        public void Drawing_nothing_but_text_that_is_empty_leaves_no_ink()
        {
            // The control for the test above.
            Run(() =>
            {
                var area = new Gtk.DrawingArea();

                bool drawn = false;
                using var window = Shown(area, (a, cr, w, h) =>
                {
                    var layout = Pango.CairoHelper.CreateLayout(cr);
                    layout.FontDescription = Pango.FontDescription.FromString("Sans 10");
                    layout.SetText("");

                    cr.SetSourceRGBA(0, 0, 1, 1);
                    cr.MoveTo(0, 0);
                    Pango.CairoHelper.ShowLayout(cr, layout);
                    drawn = true;
                }, 32);

                Assert.True(PumpUntil(() => drawn), "the draw function should have been called");

                var pixel = Render(area, 32, 32);

                for (int y = 0; y < 32; y++)
                    for (int x = 0; x < 32; x++)
                        Assert.Equal(0, pixel(x, y).A);
            });
        }
    }
}
