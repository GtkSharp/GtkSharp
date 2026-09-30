using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The parts of GdkSharp with an answer that does not depend on a display:
    /// incremental decoding (feed a PNG in pieces and it must agree with the
    /// whole file), texture download (the bytes that went in must come back),
    /// the save options a format claims to support, colour parsing, rectangle
    /// arithmetic, and the Gtk 4 clipboard data model — <c>ContentFormats</c>
    /// and <c>ContentProvider</c> — which the port introduced wholesale and
    /// nothing had ever called.
    /// </summary>
    public class GdkDeepTests : GtkTestBase
    {
        public GdkDeepTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------ helpers

        private static Gdk.Pixbuf Painted(int width, int height, uint rgba)
        {
            var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, width, height);
            pixbuf.Fill(rgba);
            return pixbuf;
        }

        private static (byte r, byte g, byte b, byte a) PixelAt(Gdk.Pixbuf pixbuf, int x, int y)
        {
            var data = pixbuf.PixelBytes.Data;
            var offset = y * pixbuf.Rowstride + x * pixbuf.NChannels;
            return (data[offset], data[offset + 1], data[offset + 2],
                    pixbuf.HasAlpha ? data[offset + 3] : (byte) 255);
        }

        /// <summary>A pixbuf whose every pixel differs, so a stride or row-order
        /// mistake cannot survive a comparison of two of them.</summary>
        private static Gdk.Pixbuf Gradient(int width, int height) => Gradient(width, height, true);

        /// <summary>The same, with the alpha channel made optional: a JPEG
        /// encoder has no alpha to write, and glycin's refuses an RGBA source
        /// outright rather than dropping the channel the way the classic
        /// gdk-pixbuf loaders do.</summary>
        private static Gdk.Pixbuf Gradient(int width, int height, bool hasAlpha)
        {
            var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, hasAlpha, 8, width, height);
            pixbuf.Fill(0x000000FFu);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    using (var dot = pixbuf.NewSubpixbuf(x, y, 1, 1))
                        dot.Fill((uint) ((x * 8) << 24 | (y * 8) << 16 | ((x + y) * 4) << 8 | 0xFF));
            return pixbuf;
        }

        private static void AssertSamePixels(Gdk.Pixbuf expected, Gdk.Pixbuf actual)
        {
            Assert.Equal(expected.Width, actual.Width);
            Assert.Equal(expected.Height, actual.Height);
            for (int y = 0; y < expected.Height; y++)
                for (int x = 0; x < expected.Width; x++)
                    Assert.Equal(PixelAt(expected, x, y), PixelAt(actual, x, y));
        }

        private static Gdk.PixbufFormat FormatNamed(string name) =>
            Gdk.Pixbuf.Formats.FirstOrDefault(f => f.Name == name);

        // -------------------------------------------- PixbufLoader: chunked IO

        [Fact]
        public void A_png_fed_in_small_chunks_decodes_to_the_same_pixels_as_the_whole_file()
        {
            // The oracle is outside the binding: PNG is lossless and its
            // decoder is incremental, so splitting the byte stream cannot
            // change the answer. A loader that dropped or duplicated a chunk,
            // or wrote the whole array instead of the count it was given,
            // fails here and nowhere else.
            Run(() =>
            {
                byte[] encoded;
                using (var original = Gradient(9, 7))
                    encoded = original.SaveToBuffer("png");

                using var whole = new Gdk.Pixbuf(encoded);

                var loader = new Gdk.PixbufLoader();
                for (int offset = 0; offset < encoded.Length; offset += 7)
                {
                    var chunk = new byte[Math.Min(7, encoded.Length - offset)];
                    Array.Copy(encoded, offset, chunk, 0, chunk.Length);
                    Assert.True(loader.Write(chunk));
                }
                Assert.True(loader.Close());

                AssertSamePixels(whole, loader.Pixbuf);
            });
        }

        [Fact]
        public void The_loader_names_the_format_it_recognised_from_the_bytes()
        {
            Run(() =>
            {
                byte[] encoded;
                using (var original = Painted(4, 4, 0x3366FFFFu))
                    encoded = original.SaveToBuffer("png");

                var loader = new Gdk.PixbufLoader();
                loader.Write(encoded);
                loader.Close();

                Assert.Equal("png", loader.Format.Name);
                Assert.Contains("image/png", loader.Format.MimeTypes);
            });
        }

        [Fact]
        public void Size_prepared_arrives_with_the_encoded_size_before_the_pixels_do()
        {
            // gdk-pixbuf emits size-prepared as soon as it has read the header,
            // which is what makes SetSize during decode possible at all. The
            // assertion is that the size reported is the one that was encoded,
            // not the one the loader ends up producing.
            Run(() =>
            {
                byte[] encoded;
                using (var original = Painted(24, 12, 0x3366FFFFu))
                    encoded = original.SaveToBuffer("png");

                int seenWidth = 0, seenHeight = 0, sizeEvents = 0;

                var loader = new Gdk.PixbufLoader();
                loader.SizePrepared += (o, args) =>
                {
                    sizeEvents++;
                    seenWidth = args.Width;
                    seenHeight = args.Height;
                };

                loader.Write(encoded);
                loader.Close();

                Assert.Equal(1, sizeEvents);
                Assert.Equal(24, seenWidth);
                Assert.Equal(12, seenHeight);
            });
        }

        [Fact]
        public void Setting_the_size_from_the_size_prepared_handler_scales_during_the_decode()
        {
            // SetSize is only meaningful between size-prepared and the first
            // row: this is the whole reason the signal exists. A binding that
            // delivered the signal too late would still decode, just at the
            // wrong size, which no non-signal test would notice.
            Run(() =>
            {
                byte[] encoded;
                using (var original = Painted(40, 20, 0x3366FFFFu))
                    encoded = original.SaveToBuffer("png");

                var loader = new Gdk.PixbufLoader();
                loader.SizePrepared += (o, args) => ((Gdk.PixbufLoader) o).SetSize(10, 5);

                loader.Write(encoded);
                loader.Close();

                Assert.Equal(10, loader.Pixbuf.Width);
                Assert.Equal(5, loader.Pixbuf.Height);
                Assert.Equal((0x33, 0x66, 0xFF, 0xFF), PixelAt(loader.Pixbuf, 5, 2));
            });
        }

        [Fact]
        public void The_loader_signals_arrive_in_the_order_a_decoder_produces_them()
        {
            // area-prepared means "the pixbuf exists"; area-updated means "these
            // rows are now valid"; closed is last. Anything that reads pixels
            // from area-updated depends on that ordering.
            Run(() =>
            {
                byte[] encoded;
                using (var original = Gradient(16, 16))
                    encoded = original.SaveToBuffer("png");

                var order = new List<string>();

                var loader = new Gdk.PixbufLoader();
                loader.SizePrepared += (o, a) => order.Add("size");
                loader.AreaPrepared += (o, a) => order.Add("prepared");
                loader.AreaUpdated += (o, a) =>
                {
                    if (order.Count == 0 || order[order.Count - 1] != "updated")
                        order.Add("updated");
                };
                loader.Closed += (o, a) => order.Add("closed");

                for (int offset = 0; offset < encoded.Length; offset += 32)
                {
                    var chunk = new byte[Math.Min(32, encoded.Length - offset)];
                    Array.Copy(encoded, offset, chunk, 0, chunk.Length);
                    loader.Write(chunk);
                }
                loader.Close();

                Assert.Equal(new[] { "size", "prepared", "updated", "closed" }, order);
            });
        }

        [Fact]
        public void The_area_updated_rectangles_between_them_cover_every_row()
        {
            // Each area-updated names rows that are now decoded. Their union has
            // to be the whole image by the time the loader closes, or an
            // application redrawing only the reported areas would leave bands
            // stale.
            Run(() =>
            {
                byte[] encoded;
                using (var original = Gradient(16, 24))
                    encoded = original.SaveToBuffer("png");

                var rows = new HashSet<int>();

                var loader = new Gdk.PixbufLoader();
                loader.AreaUpdated += (o, args) =>
                {
                    for (int y = args.Y; y < args.Y + args.Height; y++)
                        rows.Add(y);
                    Assert.Equal(0, args.X);
                    Assert.Equal(16, args.Width);
                };

                for (int offset = 0; offset < encoded.Length; offset += 16)
                {
                    var chunk = new byte[Math.Min(16, encoded.Length - offset)];
                    Array.Copy(encoded, offset, chunk, 0, chunk.Length);
                    loader.Write(chunk);
                }
                loader.Close();

                Assert.Equal(Enumerable.Range(0, 24).ToArray(), rows.OrderBy(r => r).ToArray());
            });
        }

        [Fact]
        public void A_closed_loader_refuses_further_bytes_without_raising()
        {
            // Worth pinning because the plausible assumption is wrong and fails
            // quietly. Writing to a closed loader is a g_return_val_if_fail in
            // gdk-pixbuf, not a GError: it logs a CRITICAL and returns FALSE,
            // leaving the error pointer NULL. So Write's bool return is the only
            // thing that says the bytes went nowhere, and every caller that
            // ignores it -- including PixbufLoader's own LoadFromStream -- would
            // silently drop them.
            Run(() =>
            {
                byte[] encoded;
                using (var original = Painted(4, 4, 0x3366FFFFu))
                    encoded = original.SaveToBuffer("png");

                var loader = new Gdk.PixbufLoader();
                loader.Write(encoded);
                loader.Close();

                var complaints = new List<string>();
                var id = GLib.Log.SetLogHandler("GdkPixbuf", GLib.LogLevelFlags.Critical,
                                                (domain, level, message) => complaints.Add(message));
                bool accepted;
                try
                {
                    accepted = loader.Write(encoded);
                }
                finally
                {
                    GLib.Log.RemoveLogHandler("GdkPixbuf", id);
                }

                Assert.False(accepted);
                Assert.Contains(complaints, m => m.Contains("closed"));
            });
        }

        [Fact]
        public void Closing_a_loader_that_never_saw_a_complete_image_reports_the_truncation()
        {
            // Truncation is only detectable at close: every individual write
            // succeeded. A caller that ignores Close's error therefore ships an
            // empty pixbuf, which is why Close has to raise.
            //
            // The second half of this test is where the two runtimes part, and
            // the disagreement is worth pinning rather than papering over: given
            // HALF a PNG, gdk-pixbuf's classic libpng loader closes successfully
            // and hands back a full-size pixbuf with the missing rows blank,
            // while Debian forky's glycin loader raises
            // org.gnome.glycin.Error.LoadingError ("unexpected end of file").
            // Both are asserted, because what a caller must not conclude is the
            // same on either: Close's return value says nothing about how much
            // of the image arrived, so it is not a completeness check.
            Run(() =>
            {
                byte[] encoded;
                using (var original = Gradient(32, 32))
                    encoded = original.SaveToBuffer("png");

                var header = new byte[24];   // signature plus most of IHDR
                Array.Copy(encoded, header, header.Length);

                var truncated = new Gdk.PixbufLoader();
                Assert.True(truncated.Write(header));
                Assert.ThrowsAny<GLib.GException>(() => truncated.Close());
                Assert.Null(truncated.Pixbuf);

                var half = new byte[encoded.Length / 2];
                Array.Copy(encoded, half, half.Length);

                var partial = new Gdk.PixbufLoader();
                Assert.True(partial.Write(half));

                bool closed;
                try
                {
                    closed = partial.Close();
                }
                catch (GLib.GException)
                {
                    closed = false;
                }

                if (closed)
                {
                    // The trap: success, and a pixbuf of the full encoded size
                    // whose lower rows were never decoded.
                    Assert.Equal(32, partial.Pixbuf.Width);
                    Assert.Equal(32, partial.Pixbuf.Height);
                }
                else
                {
                    // The other loader refuses instead, and then there is no
                    // pixbuf at all rather than a partly-filled one.
                    Assert.Null(partial.Pixbuf);
                }
            });
        }

        // ---------------------------------------------- save options / formats

        [Fact]
        public void A_text_chunk_written_as_a_save_option_comes_back_off_the_reloaded_pixbuf()
        {
            // The keys/values arrays are marshalled by hand in Pixbuf.cs
            // (NullTerm/ReleaseArray). Nothing else proves they arrive: an empty
            // options array is the path every other save test takes.
            Run(() =>
            {
                using var original = Painted(4, 4, 0x3366FFFFu);

                var encoded = original.SaveToBuffer("png",
                                                    new[] { "tEXt::Title", "tEXt::Author" },
                                                    new[] { "round trip", "the test" });

                using var reloaded = new Gdk.Pixbuf(encoded);

                Assert.Equal("round trip", reloaded.GetOption("tEXt::Title"));
                Assert.Equal("the test", reloaded.GetOption("tEXt::Author"));
                Assert.Null(reloaded.GetOption("tEXt::Missing"));
            });
        }

        [Fact]
        public void Png_has_a_compression_option_and_no_quality_one()
        {
            // A fact about the format rather than about the binding: PNG is
            // lossless, so it has a compression level and no quality knob.
            Run(() =>
            {
                var png = FormatNamed("png");
                Assert.NotNull(png);
                Assert.True(png.IsWritable);
                Assert.False(png.IsSaveOptionSupported("quality"));
                Assert.True(png.IsSaveOptionSupported("compression"));
            });
        }

        [SkippableFact]
        public void Jpeg_has_a_quality_option_and_no_compression_one()
        {
            // The mirror of the PNG case, and skippable rather than folded into
            // it because the jpeg loader is a separate module: an early `return`
            // would report green having asserted nothing.
            Skip.If(Run(() => FormatNamed("jpeg") == null), "no jpeg loader in this runtime");

            Run(() =>
            {
                var jpeg = FormatNamed("jpeg");
                Assert.True(jpeg.IsSaveOptionSupported("quality"));
                Assert.False(jpeg.IsSaveOptionSupported("compression"));
            });
        }

        [SkippableFact]
        public void A_worse_jpeg_quality_produces_a_smaller_file()
        {
            // Arithmetic on file sizes: whatever else changes, quality 10 must
            // not encode a photograph-like gradient larger than quality 95.
            //
            // The source deliberately has no alpha channel. JPEG cannot carry
            // one, and the two implementations disagree about what to do with
            // it: the classic gdk-pixbuf encoder drops it, glycin's refuses the
            // colour type outright with
            // "the encoder or decoder for Jpeg does not support the color type
            // Rgba8". An RGB source is the case both agree on, and the one an
            // application saving a photograph actually has.
            Skip.If(Run(() => FormatNamed("jpeg") == null), "no jpeg loader in this runtime");

            Run(() =>
            {
                using var original = Gradient(64, 64, false);
                Assert.False(original.HasAlpha);

                var coarse = original.SaveToBuffer("jpeg", new[] { "quality" }, new[] { "10" });
                var fine = original.SaveToBuffer("jpeg", new[] { "quality" }, new[] { "95" });

                Assert.True(coarse.Length < fine.Length,
                            $"quality 10 produced {coarse.Length} bytes, quality 95 produced {fine.Length}");
            });
        }

        [Fact]
        public void The_png_format_names_its_extension_and_mime_type()
        {
            // The two string-array fields of GdkPixbufFormat are marshalled by
            // the generated struct reader from a null-terminated char**; a
            // truncation there shows up as an empty array, not as an error.
            //
            // Scoped to png on purpose. Asserting this of every installed format
            // is a loop of NotEmpty with no oracle behind it, and it is not even
            // true: forky's glycin-backed loaders include a format that names no
            // extension at all, which is a fact about that runtime's module set
            // rather than about this binding.
            Run(() =>
            {
                Assert.NotEmpty(Gdk.Pixbuf.Formats);

                var png = FormatNamed("png");
                Assert.Equal("png", png.Name);
                Assert.Contains("png", png.Extensions);
                Assert.Contains("image/png", png.MimeTypes);
                Assert.False(png.IsScalable);
                Assert.True(png.IsWritable);
            });
        }

        // ------------------------------------------------------------ textures

        [Fact]
        public void A_texture_downloads_the_pixels_of_the_pixbuf_it_was_made_from()
        {
            // gdk_texture_download writes Height * stride bytes into storage the
            // CALLER provides. Codegen bound that guchar* as `out byte`, so the
            // generated Download(stride) handed Gdk the address of one stack
            // byte -- a guaranteed overwrite of the frame, not a wrong answer.
            // The download format is GDK_MEMORY_DEFAULT, i.e. cairo ARGB32,
            // which is B,G,R,A in memory on a little-endian machine.
            Run(() =>
            {
                using var pixbuf = Painted(4, 3, 0x3366FFFFu);   // r=33 g=66 b=FF a=FF
                var texture = new Gdk.Texture(pixbuf);

                var data = texture.Download();

                Assert.Equal(4 * 3 * 4, data.Length);
                Assert.Equal(new byte[] { 0xFF, 0x66, 0x33, 0xFF }, data.Take(4).ToArray());

                // Every pixel, so a stride mistake cannot hide in row 0.
                for (int i = 0; i < data.Length; i += 4)
                    Assert.Equal(new byte[] { 0xFF, 0x66, 0x33, 0xFF },
                                 new[] { data[i], data[i + 1], data[i + 2], data[i + 3] });
            });
        }

        [Fact]
        public void Downloading_into_a_buffer_smaller_than_the_texture_is_refused()
        {
            // The guard is the whole point of the fix: without it the call is a
            // buffer overrun that no assertion could survive to report.
            Run(() =>
            {
                using var pixbuf = Painted(8, 8, 0x3366FFFFu);
                var texture = new Gdk.Texture(pixbuf);

                Assert.Throws<ArgumentException>(() => texture.Download(new byte[8 * 8 * 4 - 1], 8 * 4));
                Assert.Throws<ArgumentOutOfRangeException>(() => texture.Download(new byte[8 * 8 * 4], 8 * 4 - 1));
                Assert.Throws<ArgumentNullException>(() => texture.Download(null, 8 * 4));
            });
        }

        [Fact]
        public void A_memory_texture_hands_back_exactly_the_bytes_it_was_built_from()
        {
            // The strongest oracle available here: the test chose the bytes, so
            // an identity round-trip through Gdk's texture machinery is checkable
            // without knowing anything about how Gdk stores them.
            Run(() =>
            {
                const int width = 5, height = 4, stride = width * 4;

                var source = new byte[stride * height];
                for (int i = 0; i < source.Length; i += 4)
                {
                    source[i + 0] = (byte) (i & 0xFF);          // blue
                    source[i + 1] = (byte) ((i * 3) & 0xFF);    // green
                    source[i + 2] = (byte) ((i * 7) & 0xFF);    // red
                    source[i + 3] = 0xFF;                       // opaque, so premultiply is identity
                }

                var texture = new Gdk.MemoryTexture(width, height,
                                                    Gdk.MemoryFormat.B8g8r8a8Premultiplied,
                                                    new GLib.Bytes(source), stride);

                Assert.Equal(width, texture.Width);
                Assert.Equal(height, texture.Height);
                Assert.Equal(source, texture.Download());
            });
        }

        [Fact]
        public void The_texture_downloader_and_the_texture_itself_produce_the_same_bytes()
        {
            // Two independent entry points into the same operation:
            // gdk_texture_download and gdk_texture_downloader_download_bytes.
            // Only the second was usable before, so this pins the first against
            // an implementation that was already working.
            Run(() =>
            {
                using var pixbuf = Gradient(6, 5);
                var texture = new Gdk.Texture(pixbuf);

                using var downloader = new Gdk.TextureDownloader(texture);
                Assert.Equal(Gdk.MemoryFormat.B8g8r8a8Premultiplied, downloader.Format);

                using var bytes = downloader.DownloadBytes(out var stride);

                Assert.Equal((ulong) 6 * 4, stride);
                Assert.Equal(bytes.Data, texture.Download());

                // And the caller-allocated form agrees with both.
                var into = new byte[stride * 5];
                downloader.DownloadInto(into, stride);
                Assert.Equal(bytes.Data, into);
            });
        }

        [Fact]
        public void A_texture_saved_as_png_reloads_through_the_pixbuf_decoder_unchanged()
        {
            // Crosses the two halves of the library: Gdk's own PNG writer and
            // gdk-pixbuf's reader, with a gradient so that a channel swap
            // between them cannot cancel out.
            Run(() =>
            {
                using var original = Gradient(8, 6);
                var texture = new Gdk.Texture(original);

                using var png = texture.SaveToPngBytes();
                using var reloaded = new Gdk.Pixbuf(png.Data);

                AssertSamePixels(original, reloaded);
            });
        }

        // ---------------------------------------------------------------- RGBA

        [Fact]
        public void The_css_spellings_of_one_colour_all_parse_to_the_same_value()
        {
            // Facts about CSS colour syntax, not about Gdk: #f00, #ff0000,
            // rgb(255,0,0) and the keyword "red" name one colour.
            Run(() =>
            {
                var forms = new[] { "#f00", "#ff0000", "rgb(255,0,0)", "red", "rgb(100%,0%,0%)" };

                foreach (var form in forms)
                {
                    var colour = new Gdk.RGBA();
                    Assert.True(colour.Parse(form), form + " should parse");
                    Assert.Equal(1f, colour.Red);
                    Assert.Equal(0f, colour.Green);
                    Assert.Equal(0f, colour.Blue);
                    Assert.Equal(1f, colour.Alpha);
                    Assert.True(colour.IsOpaque);
                    Assert.False(colour.IsClear);
                }
            });
        }

        [Fact]
        public void A_failed_parse_leaves_the_colour_it_was_asked_to_fill_alone()
        {
            // The generated Parse copies the struct out to unmanaged memory and
            // reads it back unconditionally, so a failing parse could easily
            // return a zeroed colour. It does not -- and code that ignores the
            // bool would otherwise silently get transparent black.
            Run(() =>
            {
                var colour = new Gdk.RGBA();
                Assert.True(colour.Parse("#336699"));
                var before = colour;

                Assert.False(colour.Parse("not a colour at all"));

                Assert.Equal(before, colour);
            });
        }

        [Fact]
        public void Equal_colours_hash_alike_and_different_ones_do_not()
        {
            // The rule a hash has to keep, checked the way that a constant hash
            // cannot satisfy: distinct values must mostly differ, and an equal
            // key must be findable in a dictionary.
            Run(() =>
            {
                var red = new Gdk.RGBA();
                red.Parse("rgb(255,0,0)");
                var alsoRed = new Gdk.RGBA();
                alsoRed.Parse("#ff0000");
                var green = new Gdk.RGBA();
                green.Parse("rgb(0,255,0)");

                Assert.Equal(red, alsoRed);
                Assert.Equal(red.GetHashCode(), alsoRed.GetHashCode());
                Assert.NotEqual(red.GetHashCode(), green.GetHashCode());

                // Gdk's own hash agrees with equality too.
                Assert.Equal(red.Hash(), alsoRed.Hash());
                Assert.NotEqual(red.Hash(), green.Hash());

                var table = new Dictionary<Gdk.RGBA, string> { { red, "red" } };
                Assert.Equal("red", table[alsoRed]);
            });
        }

        [Fact]
        public void A_half_transparent_colour_is_neither_clear_nor_opaque_and_says_so_in_its_string()
        {
            Run(() =>
            {
                var half = new Gdk.RGBA { Red = 1f, Green = 0f, Blue = 0f, Alpha = 0.5f };

                Assert.False(half.IsClear);
                Assert.False(half.IsOpaque);
                Assert.StartsWith("rgba(", half.ToString());

                var reparsed = new Gdk.RGBA();
                Assert.True(reparsed.Parse(half.ToString()));
                Assert.Equal(0.5f, reparsed.Alpha, 2);

                var clear = new Gdk.RGBA { Red = 1f, Alpha = 0f };
                Assert.True(clear.IsClear);

                var opaque = new Gdk.RGBA { Red = 1f, Alpha = 1f };
                Assert.True(opaque.IsOpaque);
                Assert.StartsWith("rgb(", opaque.ToString());
            });
        }

        // ----------------------------------------------------------- Rectangle

        [Fact]
        public void The_managed_and_native_hit_tests_agree_on_every_edge()
        {
            // Rectangle.Contains is hand-written in terms of Right = X+Width-1,
            // while gdk_rectangle_contains_point is Gdk's own. Two independent
            // implementations of one predicate: a disagreement anywhere on the
            // boundary is a defect in the hand-written half.
            Run(() =>
            {
                var rect = new Gdk.Rectangle(10, 20, 4, 3);

                for (int y = 18; y <= 24; y++)
                    for (int x = 8; x <= 15; x++)
                        Assert.Equal(rect.ContainsPoint(x, y), rect.Contains(x, y));

                Assert.True(rect.Contains(13, 22));    // the last pixel inside
                Assert.False(rect.Contains(14, 22));   // one past the right edge
                Assert.False(rect.Contains(13, 23));   // one past the bottom
            });
        }

        [Fact]
        public void Overlapping_rectangles_intersect_to_the_common_area_and_disjoint_ones_to_nothing()
        {
            Run(() =>
            {
                var a = new Gdk.Rectangle(0, 0, 10, 10);
                var b = new Gdk.Rectangle(6, 4, 10, 10);

                Assert.True(a.Intersect(b, out var overlap));
                Assert.Equal(new Gdk.Rectangle(6, 4, 4, 6), overlap);
                Assert.Equal(overlap, Gdk.Rectangle.Intersect(a, b));
                Assert.True(a.IntersectsWith(b));

                var far = new Gdk.Rectangle(100, 100, 5, 5);
                Assert.False(a.Intersect(far, out var nothing));
                Assert.True(nothing.IsEmpty);
                Assert.False(a.IntersectsWith(far));

                // Intersection is the area both cover, so it is contained in both.
                Assert.True(a.Contains(overlap));
                Assert.True(b.Contains(overlap));
            });
        }

        [Fact]
        public void Inflating_then_deflating_a_rectangle_returns_it_to_where_it_started()
        {
            // Inflate moves two edges by the same amount, so it is its own
            // inverse under negation -- arithmetic, independent of Gdk.
            Run(() =>
            {
                var original = new Gdk.Rectangle(3, 7, 11, 5);

                var grown = Gdk.Rectangle.Inflate(original, 2, 4);
                Assert.Equal(new Gdk.Rectangle(1, 3, 15, 13), grown);

                Assert.Equal(original, Gdk.Rectangle.Inflate(grown, -2, -4));
                Assert.Equal(original, Gdk.Rectangle.Offset(Gdk.Rectangle.Offset(original, 6, -9), -6, 9));

                // Union with a rectangle it already covers changes nothing.
                Assert.Equal(grown, grown.Union(original));
            });
        }

        // --------------------------------------- ContentFormats / ContentProvider

        [Fact]
        public void Content_formats_built_from_several_mime_types_contain_all_of_them()
        {
            // gdk_content_formats_new takes an ARRAY of mime types. Codegen had
            // no rule for "const char** plus an explicit count" and emitted
            // ContentFormats(string, uint), which handed Gdk one strdup'd string
            // to read as an array of pointers -- the characters of the string
            // used as addresses.
            Run(() =>
            {
                using var formats = new Gdk.ContentFormats(new[] { "text/plain", "image/png", "text/uri-list" });

                Assert.False(formats.IsEmpty);
                Assert.True(formats.ContainMimeType("text/plain"));
                Assert.True(formats.ContainMimeType("image/png"));
                Assert.True(formats.ContainMimeType("text/uri-list"));
                Assert.False(formats.ContainMimeType("application/pdf"));

                var listed = formats.GetMimeTypes(out var count);
                Assert.Equal(3ul, count);
                Assert.Equal(new[] { "text/plain", "image/png", "text/uri-list" }, listed);

                // And the parser is the inverse of ToString.
                using var reparsed = Gdk.ContentFormats.Parse(formats.ToString());
                Assert.True(reparsed.Match(formats));
            });
        }

        [Fact]
        public void Content_formats_for_gtypes_list_the_types_rather_than_the_address_of_the_list()
        {
            // gdk_content_formats_get_gtypes returns a GType array and its
            // length; the generated binding wrapped the ARRAY POINTER in a
            // GLib.GType, producing a "type" whose value was an address.
            Run(() =>
            {
                using var strings = new Gdk.ContentFormats(GLib.GType.String);
                using var textures = new Gdk.ContentFormats(Gdk.Texture.GType);
                using var both = strings.Union(textures);

                var types = both.GetGtypes();

                Assert.Equal(2, types.Length);
                Assert.Contains(GLib.GType.String, types);
                Assert.Contains(Gdk.Texture.GType, types);

                Assert.True(both.ContainGtype(GLib.GType.String));
                Assert.False(both.ContainGtype(GLib.GType.Int));

                // A formats object built only from mime types has no gtypes at
                // all -- and that is an empty array, not a null one.
                using var mimeOnly = new Gdk.ContentFormats(new[] { "text/plain" });
                Assert.Empty(mimeOnly.GetGtypes());
            });
        }

        [Fact]
        public void Two_content_formats_match_on_what_they_share_and_not_on_what_they_do_not()
        {
            Run(() =>
            {
                using var offered = new Gdk.ContentFormats(new[] { "text/plain", "image/png" });
                using var wanted = new Gdk.ContentFormats(new[] { "application/pdf", "image/png" });
                using var unrelated = new Gdk.ContentFormats(new[] { "application/pdf" });

                Assert.True(offered.Match(wanted));
                Assert.Equal("image/png", offered.MatchMimeType(wanted));

                Assert.False(offered.Match(unrelated));
                Assert.Null(offered.MatchMimeType(unrelated));

                using var union = offered.Union(unrelated);
                Assert.True(union.Match(unrelated));
                Assert.True(union.ContainMimeType("application/pdf"));
                Assert.True(union.ContainMimeType("text/plain"));
            });
        }

        [Fact]
        public void A_content_provider_gives_back_the_value_it_was_created_around()
        {
            // The Gtk 4 clipboard and drag-and-drop data model. A provider built
            // from a GValue must advertise that value's type and hand the value
            // back; both halves cross the GValue boundary in opposite directions.
            //
            // The type is an INPUT: gdk_content_provider_get_value answers a
            // GValue the caller has already initialised, and refuses any type it
            // does not hold. Codegen made it an out-parameter over uninitialised
            // memory, so Gdk read a GType out of whatever was on the heap.
            Run(() =>
            {
                using var text = new GLib.Value("carried across");
                var provider = new Gdk.ContentProvider(text);

                using var formats = provider.RefFormats();
                Assert.True(formats.ContainGtype(GLib.GType.String));
                Assert.False(formats.ContainGtype(GLib.GType.Int));

                Assert.True(provider.GetValue(GLib.GType.String, out var got));
                Assert.Equal("carried across", (string) got.Val);
                got.Dispose();

                // A type the provider does not offer is refused, not answered
                // with a default.
                Assert.ThrowsAny<GLib.GException>(() => provider.GetValue(GLib.GType.Int, out _));
            });
        }

        [Fact]
        public void A_provider_built_from_bytes_advertises_the_mime_type_it_was_given()
        {
            Run(() =>
            {
                var payload = new byte[] { (byte) 'h', (byte) 'i' };
                var provider = new Gdk.ContentProvider("application/x-gtksharp-test", new GLib.Bytes(payload));

                using var formats = provider.RefFormats();
                Assert.True(formats.ContainMimeType("application/x-gtksharp-test"));
                Assert.False(formats.ContainMimeType("text/plain"));

                // A bytes provider offers a mime type, not a GValue type, so
                // asking for one raises rather than handing back a zeroed value.
                Assert.ThrowsAny<GLib.GException>(() => provider.GetValue(GLib.GType.String, out _));
            });
        }

        [Fact]
        public void The_formats_property_and_ref_formats_describe_the_same_provider()
        {
            // Two routes to one answer: the GObject property, which arrives as a
            // boxed GValue, and gdk_content_provider_ref_formats, which is
            // transfer-full. They have separate ownership handling, and only the
            // second is what the C API expects a caller to use.
            Run(() =>
            {
                using var text = new GLib.Value("either way");
                var provider = new Gdk.ContentProvider(text);

                using var refd = provider.RefFormats();
                var property = provider.Formats;

                Assert.Equal(refd.ToString(), property.ToString());
                Assert.True(property.ContainGtype(GLib.GType.String));
            });
        }

        [Fact]
        public void A_content_provider_raises_content_changed_when_asked_to()
        {
            Run(() =>
            {
                using var text = new GLib.Value("first");
                var provider = new Gdk.ContentProvider(text);

                var changes = 0;
                provider.ContentChanged += (o, a) => changes++;

                provider.EmitContentChanged();
                provider.EmitContentChanged();

                Assert.Equal(2, changes);
            });
        }

        [Fact]
        public void The_union_operations_do_not_free_the_formats_they_were_called_on()
        {
            // Every gdk_content_formats_union* takes its RECEIVER as
            // (transfer full): the callee eats a reference. An api.xml <method>
            // describes its parameters' ownership and never the instance's, so
            // the generated wrappers passed Handle and went on owning it. The
            // formats were freed underneath a live wrapper, which unreffed them
            // a second time when it was collected -- and the crash landed on
            // whatever touched the reused address next, roughly one full-suite
            // run in three.
            //
            // Two oracles, because the first alone survived the defect for a
            // while: the receiver still describes what it did, and Gdk itself
            // reports no refcount trouble once every wrapper has been finalized
            // and its deferred unref has run on the main loop.
            Run(() =>
            {
                var complaints = new List<string>();
                var id = GLib.Log.SetLogHandler("Gdk", GLib.LogLevelFlags.Critical | GLib.LogLevelFlags.Warning,
                                                (domain, level, message) => complaints.Add(message));
                try
                {
                    for (int i = 0; i < 50; i++)
                    {
                        using var text = new Gdk.ContentFormats(new[] { "text/plain" });
                        using var strings = new Gdk.ContentFormats(GLib.GType.String);

                        using var united = text.Union(strings);
                        using var serialisable = text.UnionSerializeMimeTypes();
                        using var deserialisable = text.UnionDeserializeGtypes();

                        // Four calls that each ate a reference in C, and the
                        // receiver is still the formats it was.
                        Assert.Equal("text/plain", text.ToString());
                        Assert.True(united.ContainGtype(GLib.GType.String));
                        Assert.True(united.ContainMimeType("text/plain"));
                        Assert.True(serialisable.ContainMimeType("text/plain"));
                        Assert.True(deserialisable.ContainMimeType("text/plain"));
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    // The generated finalizer does not unref: it queues the
                    // unref onto the main loop with a 50 ms timeout, so a double
                    // free only surfaces once the loop has run.
                    var deadline = System.Diagnostics.Stopwatch.StartNew();
                    while (deadline.ElapsedMilliseconds < 400)
                        if (Gtk.Application.EventsPending())
                            Gtk.Application.RunIteration(false);
                }
                finally
                {
                    GLib.Log.RemoveLogHandler("Gdk", id);
                }

                Assert.Empty(complaints);
            });
        }

        [Fact]
        public void A_formats_builder_accumulates_what_it_is_given_and_survives_being_drained()
        {
            // The builder is the only way to mix mime types and GTypes into one
            // ContentFormats, so it is what a real drag source builds its
            // offer with.
            Run(() =>
            {
                using var builder = new Gdk.ContentFormatsBuilder();
                builder.AddMimeType("text/plain");
                builder.AddGtype(GLib.GType.String);
                builder.AddFormats(new Gdk.ContentFormats(new[] { "image/png" }));

                using var built = builder.ToFormats();
                Assert.True(built.ContainMimeType("text/plain"));
                Assert.True(built.ContainMimeType("image/png"));
                Assert.True(built.ContainGtype(GLib.GType.String));

                // Worth pinning, because it is the opposite of what the name
                // suggests and it fails silently: ToFormats RESETS the builder.
                // A second call yields empty formats rather than the same set
                // again, so code that reads the offer twice gets nothing the
                // second time.
                using var again = builder.ToFormats();
                Assert.True(again.IsEmpty);

                // FreeToFormats frees the builder in C. The generated wrapper
                // went on owning the freed pointer and unreffed it a second
                // time when this scope ended, which is the line that used to
                // corrupt the heap.
                using var second = new Gdk.ContentFormatsBuilder();
                second.AddMimeType("application/pdf");

                using var drained = second.FreeToFormats();
                Assert.True(drained.ContainMimeType("application/pdf"));
            });
        }

        // -------------------------------------------------------------- Cursor

        [Fact]
        public void A_named_cursor_keeps_its_name_and_the_fallback_it_was_given()
        {
            // Cursors are display-independent objects in Gtk 4: constructing one
            // needs no screen, which is what makes this testable headless.
            Run(() =>
            {
                var fallback = new Gdk.Cursor("default", null);
                var cursor = new Gdk.Cursor("pointer", fallback);

                Assert.Equal("pointer", cursor.Name);
                Assert.Same(fallback, cursor.Fallback);
                Assert.Null(cursor.Texture);
                Assert.Null(fallback.Fallback);
            });
        }

        [Fact]
        public void A_texture_cursor_keeps_its_hotspot_and_has_no_name()
        {
            Run(() =>
            {
                using var pixbuf = Painted(16, 16, 0x3366FFFFu);
                var texture = new Gdk.Texture(pixbuf);

                var cursor = new Gdk.Cursor(texture, 3, 11, null);

                Assert.Equal(3, cursor.HotspotX);
                Assert.Equal(11, cursor.HotspotY);
                Assert.Null(cursor.Name);
                Assert.Same(texture, cursor.Texture);
            });
        }

        // ----------------------------------------------------------- Paintable

        [Fact]
        public void A_texture_as_a_paintable_reports_its_own_size_and_aspect_ratio()
        {
            // GdkPaintable is the interface everything drawable in Gtk 4 arrives
            // through, and Texture is the implementation every application meets
            // first. ComputeConcreteSize is pure arithmetic on those numbers.
            Run(() =>
            {
                using var pixbuf = Painted(40, 20, 0x3366FFFFu);
                Gdk.IPaintable paintable = new Gdk.Texture(pixbuf);

                Assert.Equal(40, paintable.IntrinsicWidth);
                Assert.Equal(20, paintable.IntrinsicHeight);
                Assert.Equal(2.0, paintable.IntrinsicAspectRatio, 6);

                // Neither dimension specified: the paintable's own size.
                paintable.ComputeConcreteSize(0, 0, 100, 100, out var w, out var h);
                Assert.Equal(40.0, w, 6);
                Assert.Equal(20.0, h, 6);

                // One dimension specified: the other follows the aspect ratio.
                paintable.ComputeConcreteSize(80, 0, 100, 100, out w, out h);
                Assert.Equal(80.0, w, 6);
                Assert.Equal(40.0, h, 6);
            });
        }
    }
}
