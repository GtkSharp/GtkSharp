using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>Gdk.Pixbuf</c> is the widest hand-written surface left in GdkSharp, and
    /// image encoding is one of the few things in a GUI library with an answer
    /// that does not depend on a display: save a picture, load it back, and the
    /// pixels either match or they do not. <c>GLib.Log</c> and <c>GLib.Source</c>
    /// are here for the same reason — both are callback plumbing whose oracle is
    /// whether the callback ran and what it was handed.
    /// </summary>
    public class PixbufAndLogTests : GtkTestBase
    {
        public PixbufAndLogTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>A pixbuf with one known pixel per corner.</summary>
        private static Gdk.Pixbuf Painted(int width = 8, int height = 8)
        {
            var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, width, height);
            pixbuf.Fill(0x3366FFFFu);   // r=0x33 g=0x66 b=0xFF a=0xFF
            return pixbuf;
        }

        private static (byte r, byte g, byte b, byte a) PixelAt(Gdk.Pixbuf pixbuf, int x, int y)
        {
            var data = pixbuf.PixelBytes.Data;
            var offset = y * pixbuf.Rowstride + x * pixbuf.NChannels;
            return (data[offset], data[offset + 1], data[offset + 2],
                    pixbuf.HasAlpha ? data[offset + 3] : (byte) 255);
        }

        // ------------------------------------------------------------ geometry

        [Fact]
        public void A_pixbuf_reports_the_geometry_it_was_created_with()
        {
            Run(() =>
            {
                using var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 20, 10);

                Assert.Equal(20, pixbuf.Width);
                Assert.Equal(10, pixbuf.Height);
                Assert.Equal(8, pixbuf.BitsPerSample);
                Assert.Equal(4, pixbuf.NChannels);       // RGBA
                Assert.True(pixbuf.HasAlpha);
                Assert.True(pixbuf.Rowstride >= 20 * 4);
            });
        }

        [Fact]
        public void A_pixbuf_without_alpha_has_three_channels()
        {
            Run(() =>
            {
                using var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, 4, 4);

                Assert.False(pixbuf.HasAlpha);
                Assert.Equal(3, pixbuf.NChannels);
            });
        }

        [Fact]
        public void Adding_alpha_makes_the_substituted_colour_transparent()
        {
            Run(() =>
            {
                using var opaque = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, 4, 4);
                opaque.Fill(0xFF0000FFu);   // solid red; the alpha byte is ignored

                using var withAlpha = opaque.AddAlpha(true, 0xFF, 0x00, 0x00);

                Assert.True(withAlpha.HasAlpha);
                Assert.Equal(4, withAlpha.NChannels);
                Assert.Equal(0, PixelAt(withAlpha, 1, 1).a);   // red became see-through
            });
        }

        // -------------------------------------------------------------- pixels

        [Fact]
        public void Filling_writes_the_colour_across_every_pixel()
        {
            Run(() =>
            {
                using var pixbuf = Painted();

                Assert.Equal((0x33, 0x66, 0xFF, 0xFF), PixelAt(pixbuf, 0, 0));
                Assert.Equal((0x33, 0x66, 0xFF, 0xFF), PixelAt(pixbuf, 7, 7));
            });
        }

        [Fact]
        public void Copying_an_area_moves_the_pixels_it_names()
        {
            Run(() =>
            {
                using var source = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 4, 4);
                source.Fill(0x00FF00FFu);   // green

                using var target = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 8, 8);
                target.Fill(0x000000FFu);   // black

                source.CopyArea(0, 0, 4, 4, target, 2, 2);

                Assert.Equal((0x00, 0xFF, 0x00, 0xFF), PixelAt(target, 3, 3));   // inside
                Assert.Equal((0x00, 0x00, 0x00, 0xFF), PixelAt(target, 0, 0));   // outside
            });
        }

        [Fact]
        public void A_subpixbuf_shares_the_pixels_of_the_region_it_names()
        {
            Run(() =>
            {
                using var whole = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 8, 8);
                whole.Fill(0x000000FFu);

                using var corner = whole.NewSubpixbuf(4, 4, 4, 4);

                Assert.Equal(4, corner.Width);
                Assert.Equal(4, corner.Height);

                // Writing through the sub-pixbuf reaches the parent's pixels.
                corner.Fill(0xFFFFFFFFu);

                Assert.Equal((0xFF, 0xFF, 0xFF, 0xFF), PixelAt(whole, 5, 5));
                Assert.Equal((0x00, 0x00, 0x00, 0xFF), PixelAt(whole, 1, 1));
            });
        }

        [Fact]
        public void Scaling_produces_the_size_asked_for_and_keeps_the_colour()
        {
            Run(() =>
            {
                using var original = Painted(40, 20);

                using var scaled = original.ScaleSimple(20, 10, Gdk.InterpType.Nearest);

                Assert.Equal(20, scaled.Width);
                Assert.Equal(10, scaled.Height);
                Assert.Equal((0x33, 0x66, 0xFF, 0xFF), PixelAt(scaled, 10, 5));
            });
        }

        [Fact]
        public void Flipping_and_rotating_move_the_pixels_where_the_names_say()
        {
            Run(() =>
            {
                using var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 4, 4);
                pixbuf.Fill(0x000000FFu);

                // Mark the top-left pixel white by filling a 1x1 sub-pixbuf.
                using (var corner = pixbuf.NewSubpixbuf(0, 0, 1, 1))
                    corner.Fill(0xFFFFFFFFu);

                using var flipped = pixbuf.Flip(true);           // horizontally
                Assert.Equal((0xFF, 0xFF, 0xFF, 0xFF), PixelAt(flipped, 3, 0));

                using var rotated = pixbuf.RotateSimple(Gdk.PixbufRotation.Clockwise);
                Assert.Equal((0xFF, 0xFF, 0xFF, 0xFF), PixelAt(rotated, 3, 0));
            });
        }

        [Fact]
        public void A_clone_is_independent_of_the_pixbuf_it_came_from()
        {
            Run(() =>
            {
                using var original = Painted(4, 4);

                var clone = (Gdk.Pixbuf) original.Clone();

                Assert.Equal((0x33, 0x66, 0xFF, 0xFF), PixelAt(clone, 1, 1));

                clone.Fill(0x000000FFu);

                Assert.Equal((0x33, 0x66, 0xFF, 0xFF), PixelAt(original, 1, 1));
            });
        }

        // ----------------------------------------------------- encode / decode

        [Fact]
        public void The_supported_formats_include_png()
        {
            Run(() =>
            {
                var names = Gdk.Pixbuf.Formats.Select(f => f.Name).ToArray();

                Assert.Contains("png", names);
            });
        }

        [Fact]
        public void A_pixbuf_saved_to_a_buffer_loads_back_with_the_same_pixels()
        {
            // PNG is lossless, so this is an exact comparison rather than a
            // tolerance -- which means an off-by-one in the row stride or a
            // channel swap cannot pass.
            Run(() =>
            {
                using var original = Painted(6, 4);

                var encoded = original.SaveToBuffer("png");

                Assert.NotEmpty(encoded);
                Assert.Equal(0x89, encoded[0]);   // PNG signature
                Assert.Equal((byte) 'P', encoded[1]);

                using var reloaded = new Gdk.Pixbuf(encoded);

                Assert.Equal(original.Width, reloaded.Width);
                Assert.Equal(original.Height, reloaded.Height);
                Assert.Equal((0x33, 0x66, 0xFF, 0xFF), PixelAt(reloaded, 3, 2));
            });
        }

        [Fact]
        public void A_pixbuf_saved_to_a_file_loads_back_from_it()
        {
            Run(() =>
            {
                var path = Path.Combine(Path.GetTempPath(),
                                        "gtksharp-pixbuf-" + Guid.NewGuid().ToString("N") + ".png");
                try
                {
                    using var original = Painted(5, 5);

                    Assert.True(original.Save(path, "png"));
                    Assert.True(File.Exists(path));

                    using var reloaded = new Gdk.Pixbuf(path);

                    Assert.Equal(5, reloaded.Width);
                    Assert.Equal((0x33, 0x66, 0xFF, 0xFF), PixelAt(reloaded, 2, 2));
                }
                finally
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
            });
        }

        [Fact]
        public void Loading_from_a_file_at_a_size_scales_while_loading()
        {
            Run(() =>
            {
                var path = Path.Combine(Path.GetTempPath(),
                                        "gtksharp-pixbuf-" + Guid.NewGuid().ToString("N") + ".png");
                try
                {
                    using (var original = Painted(40, 20))
                        Assert.True(original.Save(path, "png"));

                    using var scaled = new Gdk.Pixbuf(path, 10, 5);

                    Assert.Equal(10, scaled.Width);
                    Assert.Equal(5, scaled.Height);
                }
                finally
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
            });
        }

        [Fact]
        public void A_pixbuf_reads_back_from_a_managed_stream()
        {
            Run(() =>
            {
                byte[] encoded;
                using (var original = Painted(4, 4))
                    encoded = original.SaveToBuffer("png");

                using var stream = new MemoryStream(encoded);
                using var reloaded = new Gdk.Pixbuf(stream);

                Assert.Equal(4, reloaded.Width);
                Assert.Equal((0x33, 0x66, 0xFF, 0xFF), PixelAt(reloaded, 2, 2));
            });
        }

        [Fact]
        public void Loading_something_that_is_not_an_image_raises_rather_than_returning_junk()
        {
            Run(() =>
            {
                var notAnImage = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

                Assert.ThrowsAny<GLib.GException>(() => new Gdk.Pixbuf(notAnImage));
            });
        }

        [Fact]
        public void Saving_in_an_unknown_format_raises()
        {
            Run(() =>
            {
                using var pixbuf = Painted(2, 2);

                Assert.ThrowsAny<GLib.GException>(() => pixbuf.SaveToBuffer("no-such-format"));
            });
        }

        // ----------------------------------------------------------- GLib.Log

        [Fact]
        public void A_log_handler_receives_the_domain_level_and_message()
        {
            Run(() =>
            {
                string seenDomain = null, seenMessage = null;
                GLib.LogLevelFlags seenLevel = 0;

                var id = GLib.Log.SetLogHandler("gtksharp-tests", GLib.LogLevelFlags.Info,
                                                (domain, level, message) =>
                                                {
                                                    seenDomain = domain;
                                                    seenLevel = level;
                                                    seenMessage = message;
                                                });
                try
                {
                    GLib.Log.WriteLog("gtksharp-tests", GLib.LogLevelFlags.Info, "a message");

                    Assert.Equal("gtksharp-tests", seenDomain);
                    Assert.Equal(GLib.LogLevelFlags.Info, seenLevel);
                    Assert.Equal("a message", seenMessage);
                }
                finally
                {
                    GLib.Log.RemoveLogHandler("gtksharp-tests", id);
                }
            });
        }

        [Fact]
        public void A_removed_log_handler_stops_being_called()
        {
            Run(() =>
            {
                var calls = 0;

                var id = GLib.Log.SetLogHandler("gtksharp-removed", GLib.LogLevelFlags.Info,
                                                (d, l, m) => calls++);

                GLib.Log.WriteLog("gtksharp-removed", GLib.LogLevelFlags.Info, "first");
                Assert.Equal(1, calls);

                GLib.Log.RemoveLogHandler("gtksharp-removed", id);

                GLib.Log.WriteLog("gtksharp-removed", GLib.LogLevelFlags.Info, "second");
                Assert.Equal(1, calls);
            });
        }

        [Fact]
        public void A_handler_only_hears_the_levels_it_asked_for()
        {
            Run(() =>
            {
                var levels = new System.Collections.Generic.List<GLib.LogLevelFlags>();

                var id = GLib.Log.SetLogHandler("gtksharp-levels", GLib.LogLevelFlags.Debug,
                                                (d, l, m) => levels.Add(l));
                try
                {
                    GLib.Log.WriteLog("gtksharp-levels", GLib.LogLevelFlags.Debug, "heard");
                    GLib.Log.WriteLog("gtksharp-levels", GLib.LogLevelFlags.Info, "not heard");

                    Assert.Equal(new[] { GLib.LogLevelFlags.Debug }, levels);
                }
                finally
                {
                    GLib.Log.RemoveLogHandler("gtksharp-levels", id);
                }
            });
        }

        // -------------------------------------------------------- GLib.Source

        [Fact]
        public void An_idle_handler_runs_and_stops_when_it_returns_false()
        {
            Run(() =>
            {
                var calls = 0;

                GLib.Idle.Add(() => { calls++; return false; });

                // Drain the main context: the idle is queued, not immediate.
                while (Gtk.Application.EventsPending())
                    Gtk.Application.RunIteration(false);

                Assert.Equal(1, calls);
            });
        }

        [Fact]
        public void An_idle_handler_that_returns_true_runs_again()
        {
            Run(() =>
            {
                var calls = 0;

                GLib.Idle.Add(() => { calls++; return calls < 3; });

                while (Gtk.Application.EventsPending())
                    Gtk.Application.RunIteration(false);

                Assert.Equal(3, calls);
            });
        }

        [Fact]
        public void Removing_a_source_by_its_id_stops_it_running()
        {
            Run(() =>
            {
                var calls = 0;

                var id = GLib.Idle.Add(() => { calls++; return true; });

                Assert.True(GLib.Source.Remove(id));

                while (Gtk.Application.EventsPending())
                    Gtk.Application.RunIteration(false);

                Assert.Equal(0, calls);
            });
        }

        [Fact]
        public void A_timeout_runs_once_the_main_loop_has_waited_long_enough()
        {
            Run(() =>
            {
                var calls = 0;

                GLib.Timeout.Add(1, () => { calls++; return false; });

                // Blocking iterations, so the loop waits for the timer rather
                // than spinning past it.
                var deadline = System.Diagnostics.Stopwatch.StartNew();
                while (calls == 0 && deadline.ElapsedMilliseconds < 2000)
                    Gtk.Application.RunIteration(true);

                Assert.Equal(1, calls);
            });
        }
    }
}
