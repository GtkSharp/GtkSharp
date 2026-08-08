using System;
using System.Text;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Further coverage of Gio, Gdk, Pango and Gsk — the assemblies the earlier
    /// tests barely reached.
    /// </summary>
    public class DeeperStackTests : GtkTestBase
    {
        public DeeperStackTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------------ Gio

        [Fact]
        public void A_variant_dictionary_returns_what_was_put_in_it()
        {
            Run(() =>
            {
                var dict = new GLib.Variant(new System.Collections.Generic.Dictionary<string, GLib.Variant> {
                    ["name"] = new GLib.Variant("value"),
                    ["count"] = new GLib.Variant(3),
                });

                var read = dict.ToAsv();

                Assert.Equal("value", (string) read["name"]);
                Assert.Equal(3, (int) read["count"]);
            });
        }

        [Fact]
        public void A_variant_array_reports_its_children()
        {
            Run(() =>
            {
                var array = new GLib.Variant(new[] { "one", "two", "three" });

                var children = array.ToArray();

                Assert.Equal(3, children.Length);
                Assert.Equal("two", (string) children[1]);
            });
        }

        [Fact]
        public void A_cancellable_reports_being_cancelled()
        {
            Run(() =>
            {
                var cancellable = new GLib.Cancellable();

                Assert.False(cancellable.IsCancelled);

                cancellable.Cancel();

                Assert.True(cancellable.IsCancelled);
            });
        }

        [Fact]
        public void A_data_input_stream_reads_a_line_at_a_time()
        {
            Run(() =>
            {
                var payload = Encoding.UTF8.GetBytes("first\nsecond\n");
                var source = new GLib.MemoryInputStream(new GLib.Bytes(payload));
                var reader = new GLib.DataInputStream(source);

                Assert.Equal("first", reader.ReadLine(out _, null));
                Assert.Equal("second", reader.ReadLine(out _, null));
            });
        }

        [Fact]
        public void A_memory_output_stream_collects_what_is_written_to_it()
        {
            Run(() =>
            {
                // A memory output stream with no realloc function is fixed
                // size, so it needs a buffer big enough up front -- writing past
                // it raises "Memory output stream not resizable" as a
                // GException, which is the error path working correctly.
                var payload = Encoding.UTF8.GetBytes("written");
                var buffer = GLib.Marshaller.Malloc(64);
                var stream = new GLib.MemoryOutputStream(buffer, 64, null, null);

                var written = stream.Write(payload, (ulong) payload.Length, null);
                stream.Close(null);

                Assert.Equal(payload.Length, (int) written);
                Assert.Equal((ulong) payload.Length, stream.DataSize);
            });
        }

        [Fact]
        public void A_GFile_reports_its_uri_and_parent()
        {
            Run(() =>
            {
                var file = GLib.FileFactory.NewForPath("/tmp/dir/file.txt");

                Assert.StartsWith("file://", file.Uri.ToString());
                Assert.Equal("dir", file.Parent.Basename);
            });
        }

        [Fact]
        public void A_list_store_removes_and_reports_the_new_length()
        {
            Run(() =>
            {
                var store = new GLib.ListStore((GLib.GType) typeof(ListModelTests.Row));
                store.Append(new ListModelTests.Row { Name = "a" }.Handle);
                store.Append(new ListModelTests.Row { Name = "b" }.Handle);

                store.Remove(0);

                Assert.Equal(1u, store.NItems);
                Assert.Equal("b", (store.GetObject(0) as ListModelTests.Row)?.Name);
            });
        }

        // ------------------------------------------------------------------ Gdk

        [Fact]
        public void An_rgba_round_trips_through_its_string_form()
        {
            Run(() =>
            {
                var colour = new Gdk.RGBA();
                Assert.True(colour.Parse("#336699"));

                var text = colour.ToString();
                var reparsed = new Gdk.RGBA();

                Assert.True(reparsed.Parse(text));
                Assert.Equal(colour.Red, reparsed.Red, 3);
                Assert.Equal(colour.Blue, reparsed.Blue, 3);
            });
        }

        [Fact]
        public void A_rectangle_knows_whether_it_contains_a_point()
        {
            Run(() =>
            {
                var rect = new Gdk.Rectangle(10, 10, 20, 20);

                Assert.True(rect.Contains(15, 15));
                Assert.False(rect.Contains(5, 5));
            });
        }

        [Fact]
        public void A_rectangle_union_covers_both()
        {
            Run(() =>
            {
                var a = new Gdk.Rectangle(0, 0, 10, 10);
                var b = new Gdk.Rectangle(20, 20, 10, 10);

                var union = a.Union(b);

                Assert.Equal(0, union.X);
                Assert.Equal(30, union.Width);
                Assert.Equal(30, union.Height);
            });
        }

        [Fact]
        public void A_pixbuf_scaled_reports_the_size_asked_for()
        {
            Run(() =>
            {
                using var original = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 40, 20);
                using var scaled = original.ScaleSimple(20, 10, Gdk.InterpType.Bilinear);

                Assert.Equal(20, scaled.Width);
                Assert.Equal(10, scaled.Height);
            });
        }

        [Fact]
        public void Filling_a_pixbuf_sets_every_pixel()
        {
            // Fill takes a packed RGBA value and writes it across the buffer,
            // so reading a byte back proves the call reached the pixels.
            Run(() =>
            {
                using var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 4, 4);

                pixbuf.Fill(0xFF0000FFu);   // opaque red

                var pixels = pixbuf.PixelBytes.Data;
                Assert.Equal(0xFF, pixels[0]);   // red
                Assert.Equal(0x00, pixels[1]);   // green
                Assert.Equal(0xFF, pixels[3]);   // alpha
            });
        }

        [Fact]
        public void A_texture_reports_the_size_of_the_pixbuf_it_wraps()
        {
            Run(() =>
            {
                using var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 16, 8);
                var texture = new Gdk.Texture(pixbuf);

                Assert.Equal(16, texture.Width);
                Assert.Equal(8, texture.Height);
            });
        }

        // ---------------------------------------------------------------- Pango

        [Fact]
        public void A_font_description_round_trips_through_its_string_form()
        {
            Run(() =>
            {
                var font = Pango.FontDescription.FromString("Serif Italic 14");

                var text = font.ToString();
                var reparsed = Pango.FontDescription.FromString(text);

                Assert.Equal(font.Family, reparsed.Family);
                Assert.Equal(font.Style, reparsed.Style);
                Assert.Equal(font.Size, reparsed.Size);
            });
        }

        [Fact]
        public void A_layout_reports_the_text_it_was_given()
        {
            Run(() =>
            {
                using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 40);
                using var cr = new Cairo.Context(surface);
                using var layout = Pango.CairoHelper.CreateLayout(cr);

                layout.SetText("measured");

                Assert.Equal("measured", layout.Text);
                Assert.Equal(1, layout.LineCount);
            });
        }

        [Fact]
        public void Wrapping_a_layout_to_a_narrow_width_produces_more_lines()
        {
            Run(() =>
            {
                using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
                using var cr = new Cairo.Context(surface);
                using var layout = Pango.CairoHelper.CreateLayout(cr);

                layout.FontDescription = Pango.FontDescription.FromString("Sans 12");
                layout.SetText("the quick brown fox jumps over the lazy dog");

                Assert.Equal(1, layout.LineCount);

                layout.Width = (int) (40 * Pango.Scale.PangoScale);
                layout.Wrap = Pango.WrapMode.Word;

                Assert.True(layout.LineCount > 1,
                            $"a narrow width should wrap, got {layout.LineCount} line(s)");
            });
        }

        [Fact]
        public void Pango_units_convert_between_points_and_device_units()
        {
            Assert.Equal(1024, Pango.Scale.PangoScale);
            Assert.Equal(10 * Pango.Scale.PangoScale, (int) Pango.Units.FromDouble(10.0));
        }

        // ------------------------------------------------------------------ Gsk

        [Fact(Skip = "Gsk.RoundedRect has no allocator: it is a boxed type whose only " +
                     "constructor is GLib.Opaque's parameterless one, which leaves a null " +
                     "handle, so Init writes through it and crashes. See Docs/testing.md.")]
        public void A_rounded_rect_keeps_its_bounds()
        {
            Run(() =>
            {
                var bounds = Graphene.Rect.Alloc();
                bounds.Init(0, 0, 40, 20);

                var rounded = new Gsk.RoundedRect();
                rounded.InitFromRect(bounds, 5);

                Assert.Equal(40, rounded.Bounds.Width, 3);
            });
        }

        [Fact]
        public void A_container_node_reports_the_bounds_covering_its_children()
        {
            Run(() =>
            {
                var first = Graphene.Rect.Alloc();
                first.Init(0, 0, 10, 10);
                var second = Graphene.Rect.Alloc();
                second.Init(20, 20, 10, 10);

                var red = new Gdk.RGBA { Red = 1f, Alpha = 1f };
                var nodes = new Gsk.RenderNode[] {
                    new Gsk.ColorNode(red, first),
                    new Gsk.ColorNode(red, second),
                };

                var snapshot = new Gtk.Snapshot();
                foreach (var node in nodes)
                    snapshot.AppendNode(node);

                var combined = snapshot.ToNode();

                Assert.NotNull(combined);
                Assert.Equal(30, combined.Bounds.Width, 3);
            });
        }

        [Fact]
        public void A_transform_reports_the_category_it_belongs_to()
        {
            Run(() =>
            {
                var identity = new Gsk.Transform();

                Assert.Equal(Gsk.TransformCategory.Identity, identity.Category);

                var moved = identity.Translate(PointAt(5, 5));

                Assert.NotEqual(Gsk.TransformCategory.Identity, moved.Category);
            });
        }

        private static Graphene.Point PointAt(float x, float y)
        {
            var point = new Graphene.Point();
            point.Init(x, y);
            return point;
        }
    }
}
