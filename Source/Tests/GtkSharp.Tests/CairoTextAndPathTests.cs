using System;
using Cairo;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The parts of <c>Cairo.Context</c> the first pass did not reach: text, path
    /// inspection, groups, masks and surface operations. Same discipline as
    /// <c>CairoTests</c> — render and read the pixels back, so the assertion is
    /// what landed in the buffer.
    /// </summary>
    public class CairoTextAndPathTests : GtkTestBase
    {
        public CairoTextAndPathTests(GtkFixture fixture) : base(fixture) { }

        private static (byte b, byte g, byte r, byte a) PixelAt(ImageSurface surface, int x, int y)
        {
            surface.Flush();
            var data = surface.Data;
            var offset = y * surface.Stride + x * 4;
            return (data[offset], data[offset + 1], data[offset + 2], data[offset + 3]);
        }

        /// <summary>How many pixels on the surface were painted at all.</summary>
        private static int PaintedPixels(ImageSurface surface)
        {
            surface.Flush();
            var data = surface.Data;
            var count = 0;

            for (var y = 0; y < surface.Height; y++)
                for (var x = 0; x < surface.Width; x++)
                    if (data[y * surface.Stride + x * 4 + 3] != 0)
                        count++;

            return count;
        }

        // ---------------------------------------------------------------- text

        [Fact]
        public void Measuring_text_reports_a_width_that_grows_with_the_string()
        {
            using var surface = new ImageSurface(Format.Argb32, 200, 60);
            using var cr = new Context(surface);

            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
            cr.SetFontSize(20);

            var one = cr.TextExtents("M");
            var many = cr.TextExtents("MMMM");

            Assert.True(one.Width > 0, "a glyph must have a width");
            Assert.True(many.Width > one.Width * 3,
                        $"four Ms ({many.Width}) should be about four times one ({one.Width})");
        }

        [Fact]
        public void A_larger_font_size_measures_larger()
        {
            using var surface = new ImageSurface(Format.Argb32, 200, 60);
            using var cr = new Context(surface);

            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);

            cr.SetFontSize(10);
            var small = cr.TextExtents("text").Width;

            cr.SetFontSize(30);
            var large = cr.TextExtents("text").Width;

            Assert.True(large > small, $"30pt ({large}) should exceed 10pt ({small})");
        }

        [Fact]
        public void Font_extents_describe_a_font_with_sensible_proportions()
        {
            using var surface = new ImageSurface(Format.Argb32, 100, 60);
            using var cr = new Context(surface);

            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
            cr.SetFontSize(16);

            var extents = cr.FontExtents;

            Assert.True(extents.Ascent > 0);
            Assert.True(extents.Descent >= 0);
            Assert.True(extents.Height >= extents.Ascent);
        }

        [Fact]
        public void Showing_text_puts_ink_on_the_surface()
        {
            using var surface = new ImageSurface(Format.Argb32, 120, 40);
            using (var cr = new Context(surface))
            {
                cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
                cr.SetFontSize(24);
                cr.SetSourceRGB(0, 0, 0);
                cr.MoveTo(5, 30);
                cr.ShowText("Hg");
            }

            Assert.True(PaintedPixels(surface) > 20,
                        "drawing two glyphs at 24pt should mark more than a handful of pixels");
        }

        [Fact]
        public void Showing_text_advances_the_current_point_by_its_width()
        {
            using var surface = new ImageSurface(Format.Argb32, 200, 40);
            using var cr = new Context(surface);

            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
            cr.SetFontSize(20);
            cr.MoveTo(10, 30);

            var advance = cr.TextExtents("hello").XAdvance;

            cr.ShowText("hello");

            // Cairo keeps the current point in 24.8 fixed point, so it lands on
            // the nearest 1/256 rather than exactly where the double says.
            Assert.Equal(10 + advance, cr.CurrentPoint.X, 1);
            Assert.True(Math.Abs(10 + advance - cr.CurrentPoint.X) < 1.0 / 256,
                        $"expected {10 + advance} within one fixed-point step of {cr.CurrentPoint.X}");
            Assert.Equal(30, cr.CurrentPoint.Y, 3);
        }

        [Fact]
        public void Text_added_as_a_path_can_be_filled_like_any_other_path()
        {
            // TextPath appends glyph outlines to the current path rather than
            // painting, so the extents afterwards describe the glyphs.
            using var surface = new ImageSurface(Format.Argb32, 200, 60);
            using var cr = new Context(surface);

            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
            cr.SetFontSize(30);
            cr.MoveTo(10, 40);
            cr.TextPath("Ay");

            var extents = cr.FillExtents();

            Assert.True(extents.Width > 0 && extents.Height > 0,
                        "the glyph outlines should give the path a real size");

            cr.Fill();

            Assert.True(PaintedPixels(surface) > 20);
        }

        [Fact]
        public void The_font_matrix_round_trips_and_scaling_it_changes_the_measurement()
        {
            using var surface = new ImageSurface(Format.Argb32, 200, 60);
            using var cr = new Context(surface);

            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
            cr.SetFontSize(10);

            var narrow = cr.TextExtents("wide").Width;

            var matrix = new Matrix();
            matrix.InitScale(30, 10);
            cr.FontMatrix = matrix;

            Assert.Equal(30, cr.FontMatrix.Xx, 3);
            Assert.Equal(10, cr.FontMatrix.Yy, 3);

            Assert.True(cr.TextExtents("wide").Width > narrow);
        }

        // ---------------------------------------------------------------- path

        [Fact]
        public void A_copied_path_can_be_appended_to_another_context()
        {
            using var source = new ImageSurface(Format.Argb32, 40, 40);
            Path path;
            using (var cr = new Context(source))
            {
                cr.Rectangle(5, 5, 10, 10);
                path = cr.CopyPath();
            }

            using var target = new ImageSurface(Format.Argb32, 40, 40);
            using (var cr = new Context(target))
            {
                cr.AppendPath(path);

                var extents = cr.FillExtents();
                Assert.Equal(5, extents.X, 3);
                Assert.Equal(10, extents.Width, 3);

                cr.SetSourceRGB(0, 0, 0);
                cr.Fill();
            }

            Assert.Equal(255, PixelAt(target, 10, 10).a);
        }

        [Fact]
        public void A_flattened_path_covers_the_same_ground_as_the_curve()
        {
            using var surface = new ImageSurface(Format.Argb32, 60, 60);
            using var cr = new Context(surface);

            cr.MoveTo(10, 10);
            cr.CurveTo(20, 0, 40, 20, 50, 10);

            var curved = cr.StrokeExtents();

            using var flat = cr.CopyPathFlat();
            cr.NewPath();
            cr.AppendPath(flat);

            var flattened = cr.StrokeExtents();

            // Flattening replaces curves with line segments within Tolerance,
            // so the bounds must agree to about that much rather than exactly.
            Assert.Equal(curved.X, flattened.X, 0);
            Assert.Equal(curved.Width, flattened.Width, 0);
        }

        [Fact]
        public void Closing_a_path_joins_the_last_point_back_to_the_first()
        {
            using var surface = new ImageSurface(Format.Argb32, 40, 40);
            using var cr = new Context(surface);

            cr.MoveTo(10, 10);
            cr.LineTo(30, 10);
            cr.LineTo(30, 30);

            Assert.Equal(30, cr.CurrentPoint.X, 3);

            cr.ClosePath();

            // Closing returns the current point to where the subpath began.
            Assert.Equal(10, cr.CurrentPoint.X, 3);
            Assert.Equal(10, cr.CurrentPoint.Y, 3);
        }

        [Fact]
        public void An_arc_sweeps_the_way_its_name_says()
        {
            static Rectangle ExtentsOf(bool negative)
            {
                using var surface = new ImageSurface(Format.Argb32, 100, 100);
                using var cr = new Context(surface);

                if (negative)
                    cr.ArcNegative(50, 50, 20, 0, Math.PI / 2);
                else
                    cr.Arc(50, 50, 20, 0, Math.PI / 2);

                return cr.StrokeExtents();
            }

            // A quarter turn one way stays in one quadrant; the other way goes
            // three quarters of the circle round, so it is much wider.
            Assert.True(ExtentsOf(true).Width > ExtentsOf(false).Width);
        }

        [Fact]
        public void InStroke_answers_for_points_on_and_off_the_line()
        {
            using var surface = new ImageSurface(Format.Argb32, 60, 60);
            using var cr = new Context(surface);

            cr.LineWidth = 10;
            cr.MoveTo(10, 30);
            cr.LineTo(50, 30);

            Assert.True(cr.InStroke(30, 30));
            Assert.False(cr.InStroke(30, 50));
        }

        [Fact]
        public void StrokePreserve_and_FillPreserve_keep_the_path_for_a_second_pass()
        {
            using var surface = new ImageSurface(Format.Argb32, 40, 40);
            using var cr = new Context(surface);

            cr.Rectangle(10, 10, 20, 20);

            cr.SetSourceRGB(0, 0, 1);
            cr.FillPreserve();

            // The path is still there, so it can be stroked without rebuilding.
            Assert.Equal(20, cr.FillExtents().Width, 3);

            cr.SetSourceRGB(1, 0, 0);
            cr.LineWidth = 4;
            cr.Stroke();

            Assert.Equal(255, PixelAt(surface, 20, 20).b);   // blue fill inside
            Assert.Equal(255, PixelAt(surface, 10, 20).r);   // red stroke on the edge
        }

        // ------------------------------------------------------ groups & masks

        [Fact]
        public void A_group_can_be_drawn_into_and_then_painted_as_one()
        {
            using var surface = new ImageSurface(Format.Argb32, 40, 40);
            using (var cr = new Context(surface))
            {
                cr.PushGroup();

                cr.SetSourceRGB(0, 1, 0);
                cr.Rectangle(10, 10, 20, 20);
                cr.Fill();

                using var group = cr.PopGroup();

                // Nothing has reached the surface yet: the drawing went to the
                // group. Painting the group at half alpha proves both halves.
                Assert.Equal(0, PaintedPixels(surface));

                cr.SetSource(group);
                cr.PaintWithAlpha(0.5);
            }

            var (_, g, _, a) = PixelAt(surface, 20, 20);
            Assert.InRange(a, 126, 130);
            Assert.InRange(g, 126, 130);
        }

        [Fact]
        public void A_mask_limits_paint_to_where_the_mask_is_opaque()
        {
            using var stencil = new ImageSurface(Format.Argb32, 40, 40);
            using (var cr = new Context(stencil))
            {
                cr.SetSourceRGBA(0, 0, 0, 1);
                cr.Rectangle(0, 0, 20, 40);   // the left half only
                cr.Fill();
            }

            using var surface = new ImageSurface(Format.Argb32, 40, 40);
            using (var cr = new Context(surface))
            {
                cr.SetSourceRGB(1, 0, 0);
                cr.MaskSurface(stencil, 0, 0);
            }

            Assert.Equal(255, PixelAt(surface, 10, 20).a);   // under the mask
            Assert.Equal(0, PixelAt(surface, 30, 20).a);     // outside it
        }

        [Fact]
        public void The_Clear_operator_takes_paint_away_again()
        {
            using var surface = new ImageSurface(Format.Argb32, 20, 20);
            using (var cr = new Context(surface))
            {
                cr.SetSourceRGB(0, 0, 0);
                cr.Paint();

                Assert.Equal(400, PaintedPixels(surface));

                cr.Operator = Operator.Clear;
                cr.Rectangle(5, 5, 10, 10);
                cr.Fill();
            }

            Assert.Equal(0, PixelAt(surface, 10, 10).a);     // cleared
            Assert.Equal(255, PixelAt(surface, 1, 1).a);     // left alone
        }

        [Fact]
        public void The_clip_extents_describe_the_region_that_was_clipped_to()
        {
            using var surface = new ImageSurface(Format.Argb32, 100, 100);
            using var cr = new Context(surface);

            var whole = cr.ClipExtents();
            Assert.Equal(100, whole.Width, 3);

            cr.Rectangle(20, 30, 40, 50);
            cr.Clip();

            var clipped = cr.ClipExtents();

            Assert.Equal(20, clipped.X, 3);
            Assert.Equal(40, clipped.Width, 3);

            cr.ResetClip();

            Assert.Equal(100, cr.ClipExtents().Width, 3);
        }

        // ------------------------------------------------------------ surfaces

        [Fact]
        public void A_similar_surface_matches_the_size_it_was_asked_for()
        {
            using var surface = new ImageSurface(Format.Argb32, 40, 40);

            using var similar = surface.CreateSimilar(Content.ColorAlpha, 20, 10);

            Assert.Equal(Status.Success, similar.Status);
        }

        [Fact]
        public void One_surface_painted_onto_another_carries_its_pixels_over()
        {
            using var source = new ImageSurface(Format.Argb32, 10, 10);
            using (var cr = new Context(source))
            {
                cr.SetSourceRGB(0, 0, 1);
                cr.Paint();
            }

            using var target = new ImageSurface(Format.Argb32, 40, 40);
            using (var cr = new Context(target))
            {
                cr.SetSourceSurface(source, 15, 15);
                cr.Paint();
            }

            Assert.Equal(255, PixelAt(target, 20, 20).b);   // where it was placed
            Assert.Equal(0, PixelAt(target, 5, 5).a);       // outside it
        }

        [Fact]
        public void A_surface_written_to_a_png_reads_back_with_the_same_pixels()
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                                              "gtksharp-cairo-" + Guid.NewGuid().ToString("N") + ".png");
            try
            {
                using (var surface = new ImageSurface(Format.Argb32, 8, 8))
                {
                    using (var cr = new Context(surface))
                    {
                        cr.SetSourceRGB(0, 0, 1);
                        cr.Paint();
                    }

                    surface.WriteToPng(path);
                    Assert.Equal(Status.Success, surface.Status);
                }

                Assert.True(System.IO.File.Exists(path));

                using var reloaded = new ImageSurface(path);

                Assert.Equal(8, reloaded.Width);
                Assert.Equal(255, PixelAt(reloaded, 4, 4).b);
            }
            finally
            {
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
        }

        [Fact]
        public void A_surface_can_be_given_an_offset_that_moves_where_drawing_lands()
        {
            using var surface = new ImageSurface(Format.Argb32, 40, 40);

            surface.DeviceOffset = new PointD(10, 10);

            using (var cr = new Context(surface))
            {
                cr.SetSourceRGB(0, 0, 0);
                cr.Rectangle(0, 0, 5, 5);
                cr.Fill();
            }

            Assert.Equal(10, surface.DeviceOffset.X, 3);

            // The rectangle was drawn at the origin, but the device offset moved
            // it ten pixels down and across.
            Assert.Equal(255, PixelAt(surface, 12, 12).a);
            Assert.Equal(0, PixelAt(surface, 2, 2).a);
        }
    }
}
