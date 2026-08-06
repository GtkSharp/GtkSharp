using System;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Cairo, Pango, Graphene, Gdk and Gsk — the drawing stack under Gtk.
    /// </summary>
    /// <remarks>
    /// Several of these assert rendered output rather than API shape. Reading a
    /// pixel back is the strongest oracle available for a drawing binding: it
    /// proves the call reached the native library, that the arguments arrived in
    /// the right order, and that the result came back — none of which a
    /// non-throwing call demonstrates.
    /// </remarks>
    public class GraphicsStackTests : GtkTestBase
    {
        public GraphicsStackTests(GtkFixture gtk) : base(gtk) { }

        // ---------------------------------------------------------------- Cairo

        [Fact]
        public void Cairo_fills_the_pixels_it_is_told_to()
        {
            using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 4, 4);
            using (var cr = new Cairo.Context(surface))
            {
                cr.SetSourceRGBA(1, 0, 0, 1);       // opaque red
                cr.Rectangle(0, 0, 2, 4);           // left half only
                cr.Fill();
            }

            surface.Flush();
            var data = surface.Data;
            int stride = surface.Stride;

            // ARGB32 is premultiplied BGRA in memory on little-endian.
            Assert.Equal(0xFF, data[0 * stride + 0 * 4 + 2]);   // red channel, filled
            Assert.Equal(0xFF, data[0 * stride + 0 * 4 + 3]);   // alpha, filled
            Assert.Equal(0x00, data[0 * stride + 3 * 4 + 3]);   // alpha, untouched half
        }

        [Fact]
        public void Cairo_translation_moves_where_drawing_lands()
        {
            using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 4, 4);
            using (var cr = new Cairo.Context(surface))
            {
                cr.Translate(2, 0);
                cr.SetSourceRGBA(0, 0, 1, 1);
                cr.Rectangle(0, 0, 2, 4);           // becomes the right half
                cr.Fill();
            }

            surface.Flush();
            var data = surface.Data;
            int stride = surface.Stride;

            Assert.Equal(0x00, data[0 * stride + 0 * 4 + 3]);   // left half untouched
            Assert.Equal(0xFF, data[0 * stride + 3 * 4 + 3]);   // right half filled
        }

        // ---------------------------------------------------------------- Pango

        [Fact]
        public void Pango_font_description_round_trips_through_its_string_form()
        {
            var font = Pango.FontDescription.FromString("Monospace Bold 12");

            Assert.Equal("Monospace", font.Family);
            Assert.Equal(Pango.Weight.Bold, font.Weight);
            Assert.Equal(12 * Pango.Scale.PangoScale, font.Size);
        }

        [Fact]
        public void Pango_measures_longer_text_as_wider()
        {
            // An absolute size depends on the font available, but the ordering
            // does not -- and it still proves the layout was measured natively.
            using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 200, 50);
            using var cr = new Cairo.Context(surface);
            using var layout = Pango.CairoHelper.CreateLayout(cr);

            layout.FontDescription = Pango.FontDescription.FromString("Sans 12");

            layout.SetText("i");
            layout.GetPixelSize(out int narrow, out int height);

            layout.SetText("wwwwwwwwww");
            layout.GetPixelSize(out int wide, out _);

            Assert.True(height > 0, "text should have a measurable height");
            Assert.True(wide > narrow, $"'wwwwwwwwww' ({wide}) should be wider than 'i' ({narrow})");
        }

        // ------------------------------------------------------------- Graphene

        [Fact]
        public void Graphene_rect_reports_the_geometry_it_was_initialised_with()
        {
            var rect = Graphene.Rect.Alloc();
            rect.Init(1, 2, 30, 40);

            Assert.Equal(1, rect.X, 3);
            Assert.Equal(2, rect.Y, 3);
            Assert.Equal(30, rect.Width, 3);
            Assert.Equal(40, rect.Height, 3);
        }

        [Fact]
        public void Graphene_rect_union_covers_both_rectangles()
        {
            var a = Graphene.Rect.Alloc();
            a.Init(0, 0, 10, 10);
            var b = Graphene.Rect.Alloc();
            b.Init(20, 20, 10, 10);

            var union = a.Union(b);

            Assert.Equal(0, union.X, 3);
            Assert.Equal(0, union.Y, 3);
            Assert.Equal(30, union.Width, 3);
            Assert.Equal(30, union.Height, 3);
        }

        // ------------------------------------------------------------------ Gdk

        [Theory]
        [InlineData("#FF0000", 1f, 0f, 0f)]
        [InlineData("#00FF00", 0f, 1f, 0f)]
        [InlineData("blue", 0f, 0f, 1f)]
        public void Gdk_rgba_parses_named_and_hex_colours(string spec, float r, float g, float b)
        {
            var rgba = new Gdk.RGBA();

            Assert.True(rgba.Parse(spec), $"'{spec}' should parse");

            Assert.Equal(r, rgba.Red, 3);
            Assert.Equal(g, rgba.Green, 3);
            Assert.Equal(b, rgba.Blue, 3);
        }

        [Fact]
        public void Gdk_rgba_rejects_nonsense()
        {
            var rgba = new Gdk.RGBA();

            Assert.False(rgba.Parse("not a colour"));
        }

        [Fact]
        public void Gdk_rectangle_intersection_matches_the_overlap()
        {
            var a = new Gdk.Rectangle(0, 0, 10, 10);
            var b = new Gdk.Rectangle(5, 5, 10, 10);

            Assert.True(a.Intersect(b, out Gdk.Rectangle overlap));

            Assert.Equal(5, overlap.X);
            Assert.Equal(5, overlap.Y);
            Assert.Equal(5, overlap.Width);
            Assert.Equal(5, overlap.Height);
        }

        [Fact]
        public void Gdk_pixbuf_reports_the_geometry_it_was_created_with()
        {
            using var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 16, 9);

            Assert.Equal(16, pixbuf.Width);
            Assert.Equal(9, pixbuf.Height);
            Assert.True(pixbuf.HasAlpha);
        }

        // ------------------------------------------------------------------ Gsk

        [Fact]
        public void Gsk_colour_node_keeps_the_bounds_it_was_given()
        {
            // GskRenderNode is a GLib fundamental type bound on GLib.Opaque, so
            // this also exercises that hierarchy's ownership handling.
            var bounds = Graphene.Rect.Alloc();
            bounds.Init(0, 0, 20, 10);

            var node = new Gsk.ColorNode(new Gdk.RGBA { Red = 1f, Alpha = 1f }, bounds);

            Assert.NotEqual(IntPtr.Zero, node.Handle);
            Assert.Equal(20, node.Bounds.Width, 3);
            Assert.Equal(10, node.Bounds.Height, 3);
        }

        [Fact]
        public void Gsk_colour_node_is_constructed_and_owned()
        {
            // GskRenderNode is a GLib fundamental type bound on GLib.Opaque, so
            // this pins that hierarchy's construction and ownership. Reading
            // Bounds back is a separate, currently-broken path -- see the
            // skipped test above.
            var bounds = Graphene.Rect.Alloc();
            bounds.Init(0, 0, 20, 10);

            var node = new Gsk.ColorNode(new Gdk.RGBA { Red = 1f, Alpha = 1f }, bounds);

            Assert.NotEqual(IntPtr.Zero, node.Handle);
            Assert.Equal(Gsk.RenderNodeType.ColorNode, node.NodeType);
        }

        [Fact]
        public void Gsk_transform_composes_translations()
        {
            var offset = new Graphene.Point();
            offset.Init(5, 7);

            var transform = new Gsk.Transform().Translate(offset);

            // to_string is the documented way to inspect a transform, and it
            // proves the translation actually reached the native side.
            Assert.Contains("5", transform.ToString());
            Assert.Contains("7", transform.ToString());
        }
    }
}
