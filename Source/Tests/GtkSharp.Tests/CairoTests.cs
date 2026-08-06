using System;
using Cairo;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// CairoSharp is entirely hand-written and was the least covered assembly with
    /// any real logic in it — <c>Context.cs</c> alone had 754 untouched lines.
    /// Drawing is testable without a screen: render to an <c>ImageSurface</c> and
    /// read the pixels back, and the assertion is what actually landed in the
    /// buffer rather than that a call returned.
    /// </summary>
    public class CairoTests : GtkTestBase
    {
        public CairoTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>Reads a pixel as (b, g, r, a) — ARGB32 is premultiplied and
        /// stored little-endian, so byte 0 is blue.</summary>
        private static (byte b, byte g, byte r, byte a) PixelAt(ImageSurface surface, int x, int y)
        {
            surface.Flush();
            var data = surface.Data;
            var offset = y * surface.Stride + x * 4;
            return (data[offset], data[offset + 1], data[offset + 2], data[offset + 3]);
        }

        // ------------------------------------------------------------- surfaces

        [Fact]
        public void An_image_surface_reports_the_geometry_it_was_created_with()
        {
            using var surface = new ImageSurface(Format.Argb32, 64, 32);

            Assert.Equal(64, surface.Width);
            Assert.Equal(32, surface.Height);
            Assert.Equal(Format.Argb32, surface.Format);
            Assert.True(surface.Stride >= 64 * 4, "stride must cover four bytes per pixel");
            Assert.Equal(Status.Success, surface.Status);
        }

        [Fact]
        public void A_new_surface_starts_out_transparent()
        {
            using var surface = new ImageSurface(Format.Argb32, 8, 8);

            Assert.Equal((0, 0, 0, 0), PixelAt(surface, 4, 4));
        }

        // -------------------------------------------------------------- filling

        [Fact]
        public void Painting_a_colour_reaches_every_pixel()
        {
            using var surface = new ImageSurface(Format.Argb32, 8, 8);
            using (var cr = new Context(surface))
            {
                cr.SetSourceRGB(1, 0, 0);
                cr.Paint();
            }

            var (b, g, r, a) = PixelAt(surface, 0, 0);
            Assert.Equal(255, r);
            Assert.Equal(0, g);
            Assert.Equal(0, b);
            Assert.Equal(255, a);

            Assert.Equal((0, 0, 255, 255), PixelAt(surface, 7, 7));
        }

        [Fact]
        public void Filling_a_rectangle_paints_inside_it_and_not_outside()
        {
            using var surface = new ImageSurface(Format.Argb32, 20, 20);
            using (var cr = new Context(surface))
            {
                cr.SetSourceRGB(0, 0, 1);
                cr.Rectangle(5, 5, 10, 10);
                cr.Fill();
            }

            Assert.Equal(255, PixelAt(surface, 10, 10).b);   // inside
            Assert.Equal(0, PixelAt(surface, 2, 2).a);       // outside, untouched
            Assert.Equal(0, PixelAt(surface, 17, 17).a);
        }

        [Fact]
        public void A_half_transparent_source_leaves_premultiplied_pixels()
        {
            // ARGB32 stores colour premultiplied by alpha, so red at 50% alpha
            // is stored as roughly (128, 0, 0, 128) rather than (255, 0, 0, 128).
            // Getting this wrong is how a binding produces washed-out drawing.
            using var surface = new ImageSurface(Format.Argb32, 4, 4);
            using (var cr = new Context(surface))
            {
                cr.SetSourceRGBA(1, 0, 0, 0.5);
                cr.Paint();
            }

            var (_, _, r, a) = PixelAt(surface, 1, 1);
            Assert.InRange(a, 126, 130);
            Assert.InRange(r, 126, 130);
        }

        [Fact]
        public void The_fill_rule_decides_whether_the_middle_of_a_ring_is_filled()
        {
            // Two concentric squares wound the same way: EvenOdd leaves the
            // middle empty, Winding fills it.
            static byte MiddleAlpha(FillRule rule)
            {
                using var surface = new ImageSurface(Format.Argb32, 40, 40);
                using (var cr = new Context(surface))
                {
                    cr.FillRule = rule;
                    cr.SetSourceRGB(0, 0, 0);
                    cr.Rectangle(5, 5, 30, 30);
                    cr.Rectangle(15, 15, 10, 10);
                    cr.Fill();
                }
                return PixelAt(surface, 20, 20).a;
            }

            Assert.Equal(0, MiddleAlpha(FillRule.EvenOdd));
            Assert.Equal(255, MiddleAlpha(FillRule.Winding));
        }

        // ----------------------------------------------------------- path state

        [Fact]
        public void The_current_point_follows_the_path_being_built()
        {
            using var surface = new ImageSurface(Format.Argb32, 50, 50);
            using var cr = new Context(surface);

            Assert.False(cr.HasCurrentPoint);

            cr.MoveTo(10, 20);

            Assert.True(cr.HasCurrentPoint);
            Assert.Equal(10, cr.CurrentPoint.X, 3);
            Assert.Equal(20, cr.CurrentPoint.Y, 3);

            cr.RelLineTo(5, -5);

            Assert.Equal(15, cr.CurrentPoint.X, 3);
            Assert.Equal(15, cr.CurrentPoint.Y, 3);

            cr.NewPath();

            Assert.False(cr.HasCurrentPoint);
        }

        [Fact]
        public void Fill_extents_describe_the_path_that_was_built()
        {
            using var surface = new ImageSurface(Format.Argb32, 100, 100);
            using var cr = new Context(surface);

            cr.Rectangle(10, 20, 30, 40);
            var extents = cr.FillExtents();

            Assert.Equal(10, extents.X, 3);
            Assert.Equal(20, extents.Y, 3);
            Assert.Equal(30, extents.Width, 3);
            Assert.Equal(40, extents.Height, 3);
        }

        [Fact]
        public void Stroke_extents_are_wider_than_fill_extents_by_half_the_line_width()
        {
            using var surface = new ImageSurface(Format.Argb32, 100, 100);
            using var cr = new Context(surface);

            cr.LineWidth = 10;
            cr.Rectangle(20, 20, 40, 40);

            var stroke = cr.StrokeExtents();

            // The stroke straddles the path, so it reaches five units either side.
            Assert.Equal(15, stroke.X, 3);
            Assert.Equal(50, stroke.Width, 3);
        }

        [Fact]
        public void InFill_answers_for_points_inside_and_outside_the_path()
        {
            using var surface = new ImageSurface(Format.Argb32, 100, 100);
            using var cr = new Context(surface);

            cr.Rectangle(10, 10, 20, 20);

            Assert.True(cr.InFill(15, 15));
            Assert.False(cr.InFill(50, 50));
        }

        // ------------------------------------------------------- graphics state

        [Fact]
        public void Save_and_Restore_undo_a_state_change()
        {
            using var surface = new ImageSurface(Format.Argb32, 10, 10);
            using var cr = new Context(surface);

            cr.LineWidth = 2;
            cr.Save();

            cr.LineWidth = 17;
            Assert.Equal(17, cr.LineWidth, 3);

            cr.Restore();

            Assert.Equal(2, cr.LineWidth, 3);
        }

        [Fact]
        public void Every_scalar_state_property_reports_back_what_was_set()
        {
            using var surface = new ImageSurface(Format.Argb32, 10, 10);
            using var cr = new Context(surface);

            cr.LineWidth = 4.5;
            cr.LineCap = LineCap.Round;
            cr.LineJoin = LineJoin.Bevel;
            cr.MiterLimit = 3;
            cr.Antialias = Antialias.None;
            cr.FillRule = FillRule.EvenOdd;
            cr.Tolerance = 0.5;
            cr.Operator = Operator.Add;

            Assert.Equal(4.5, cr.LineWidth, 3);
            Assert.Equal(LineCap.Round, cr.LineCap);
            Assert.Equal(LineJoin.Bevel, cr.LineJoin);
            Assert.Equal(3, cr.MiterLimit, 3);
            Assert.Equal(Antialias.None, cr.Antialias);
            Assert.Equal(FillRule.EvenOdd, cr.FillRule);
            Assert.Equal(0.5, cr.Tolerance, 3);
            Assert.Equal(Operator.Add, cr.Operator);
            Assert.Equal(Status.Success, cr.Status);
        }

        [Fact]
        public void A_dash_pattern_leaves_gaps_a_solid_stroke_does_not()
        {
            // Context exposes no getter for the dash array, so the oracle has to
            // be the drawing: with 4-on 4-off, the pixel at x=6 falls in a gap.
            static byte AlphaAt(int x, double[] dashes)
            {
                using var surface = new ImageSurface(Format.Argb32, 20, 10);
                using (var cr = new Context(surface))
                {
                    cr.SetSourceRGB(0, 0, 0);
                    cr.LineWidth = 4;
                    cr.Antialias = Antialias.None;
                    if (dashes != null)
                        cr.SetDash(dashes, 0);
                    cr.MoveTo(0, 5);
                    cr.LineTo(20, 5);
                    cr.Stroke();
                }
                return PixelAt(surface, x, 5).a;
            }

            Assert.Equal(255, AlphaAt(2, null));                        // solid: painted
            Assert.Equal(255, AlphaAt(2, new double[] { 4, 4 }));       // dashed: on
            Assert.Equal(0, AlphaAt(6, new double[] { 4, 4 }));         // dashed: gap
        }

        // ----------------------------------------------------------- transforms

        [Fact]
        public void Translating_moves_where_a_rectangle_lands()
        {
            using var surface = new ImageSurface(Format.Argb32, 40, 40);
            using (var cr = new Context(surface))
            {
                cr.Translate(20, 20);
                cr.SetSourceRGB(0, 1, 0);
                cr.Rectangle(0, 0, 10, 10);
                cr.Fill();
            }

            Assert.Equal(255, PixelAt(surface, 25, 25).g);   // where it moved to
            Assert.Equal(0, PixelAt(surface, 5, 5).a);       // where it would have been
        }

        [Fact]
        public void Scaling_stretches_what_is_drawn()
        {
            using var surface = new ImageSurface(Format.Argb32, 40, 40);
            using (var cr = new Context(surface))
            {
                cr.Scale(4, 4);
                cr.SetSourceRGB(0, 0, 0);
                cr.Rectangle(0, 0, 5, 5);       // becomes 20x20 on the surface
                cr.Fill();
            }

            Assert.Equal(255, PixelAt(surface, 18, 18).a);
            Assert.Equal(0, PixelAt(surface, 22, 22).a);
        }

        [Fact]
        public void User_and_device_coordinates_convert_back_and_forth()
        {
            using var surface = new ImageSurface(Format.Argb32, 100, 100);
            using var cr = new Context(surface);

            cr.Translate(10, 5);
            cr.Scale(2, 2);

            double x = 3, y = 4;
            cr.UserToDevice(ref x, ref y);

            Assert.Equal(16, x, 3);   // 10 + 3*2
            Assert.Equal(13, y, 3);   //  5 + 4*2

            cr.DeviceToUser(ref x, ref y);

            Assert.Equal(3, x, 3);
            Assert.Equal(4, y, 3);
        }

        [Fact]
        public void The_identity_matrix_undoes_a_transform()
        {
            using var surface = new ImageSurface(Format.Argb32, 100, 100);
            using var cr = new Context(surface);

            cr.Translate(30, 30);
            cr.IdentityMatrix();

            double x = 1, y = 1;
            cr.UserToDevice(ref x, ref y);

            Assert.Equal(1, x, 3);
            Assert.Equal(1, y, 3);
        }

        // -------------------------------------------------------- Cairo.Matrix

        [Fact]
        public void A_fresh_matrix_is_the_identity_and_leaves_points_alone()
        {
            var matrix = new Matrix();

            Assert.True(matrix.IsIdentity());

            double x = 7, y = 9;
            matrix.TransformPoint(ref x, ref y);

            Assert.Equal(7, x, 6);
            Assert.Equal(9, y, 6);
        }

        [Fact]
        public void A_translation_matrix_moves_points_but_not_distances()
        {
            var matrix = new Matrix();
            matrix.InitTranslate(5, -3);

            double x = 1, y = 1;
            matrix.TransformPoint(ref x, ref y);
            Assert.Equal(6, x, 6);
            Assert.Equal(-2, y, 6);

            // A distance is a vector: translation must not affect it.
            double dx = 1, dy = 1;
            matrix.TransformDistance(ref dx, ref dy);
            Assert.Equal(1, dx, 6);
            Assert.Equal(1, dy, 6);
        }

        [Fact]
        public void Inverting_a_matrix_undoes_it()
        {
            var matrix = new Matrix();
            matrix.InitScale(2, 4);
            matrix.Translate(3, 1);

            double x = 5, y = 5;
            matrix.TransformPoint(ref x, ref y);

            Assert.Equal(Status.Success, matrix.Invert());
            matrix.TransformPoint(ref x, ref y);

            Assert.Equal(5, x, 6);
            Assert.Equal(5, y, 6);
        }

        [Fact]
        public void Multiplying_by_the_inverse_gives_the_identity()
        {
            var matrix = new Matrix();
            matrix.InitRotate(Math.PI / 3);

            var inverse = (Matrix) matrix.Clone();
            Assert.Equal(Status.Success, inverse.Invert());

            var product = Matrix.Multiply(matrix, inverse);

            Assert.Equal(1, product.Xx, 6);
            Assert.Equal(0, product.Yx, 6);
            Assert.Equal(0, product.Xy, 6);
            Assert.Equal(1, product.Yy, 6);
        }

        [Fact]
        public void Matrices_compare_by_value()
        {
            var a = new Matrix(1, 2, 3, 4, 5, 6);
            var b = new Matrix(1, 2, 3, 4, 5, 6);
            var c = new Matrix(1, 2, 3, 4, 5, 7);

            Assert.True(a == b);
            Assert.False(a == c);
            Assert.True(a != c);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        // -------------------------------------------------------- Cairo.Region

        [Fact]
        public void A_region_built_from_a_rectangle_reports_it_back()
        {
            using var region = new Region(new RectangleInt { X = 5, Y = 10, Width = 20, Height = 30 });

            Assert.False(region.IsEmpty);
            Assert.Equal(1, region.NumRectangles);

            var extents = region.Extents;
            Assert.Equal(5, extents.X);
            Assert.Equal(20, extents.Width);
            Assert.Equal(30, extents.Height);
        }

        [Fact]
        public void An_empty_region_contains_nothing()
        {
            using var region = new Region();

            Assert.True(region.IsEmpty);
            Assert.Equal(0, region.NumRectangles);
            Assert.False(region.ContainsPoint(0, 0));
        }

        [Fact]
        public void A_region_answers_which_points_are_inside_it()
        {
            using var region = new Region(new RectangleInt { X = 0, Y = 0, Width = 10, Height = 10 });

            Assert.True(region.ContainsPoint(5, 5));
            Assert.False(region.ContainsPoint(15, 5));
        }

        [Fact]
        public void Union_intersect_and_subtract_change_what_the_region_covers()
        {
            var left = new RectangleInt { X = 0, Y = 0, Width = 10, Height = 10 };
            var right = new RectangleInt { X = 5, Y = 0, Width = 10, Height = 10 };

            using (var region = new Region(left))
            {
                using var other = new Region(right);
                Assert.Equal(Status.Success, region.Union(other));
                Assert.Equal(15, region.Extents.Width);
            }

            using (var region = new Region(left))
            {
                using var other = new Region(right);
                Assert.Equal(Status.Success, region.Intersect(other));
                Assert.Equal(5, region.Extents.X);
                Assert.Equal(5, region.Extents.Width);
            }

            using (var region = new Region(left))
            {
                using var other = new Region(right);
                Assert.Equal(Status.Success, region.Subtract(other));
                Assert.Equal(0, region.Extents.X);
                Assert.Equal(5, region.Extents.Width);
            }
        }

        [Fact]
        public void Translating_a_region_moves_its_extents()
        {
            using var region = new Region(new RectangleInt { X = 0, Y = 0, Width = 4, Height = 4 });

            region.Translate(7, -2);

            Assert.Equal(7, region.Extents.X);
            Assert.Equal(-2, region.Extents.Y);
        }

        [Fact]
        public void A_copied_region_equals_the_original_and_then_diverges()
        {
            using var original = new Region(new RectangleInt { X = 0, Y = 0, Width = 8, Height = 8 });
            using var copy = original.Copy();

            Assert.True(original.Equals(copy));

            copy.Translate(100, 0);

            Assert.False(original.Equals(copy));
            Assert.Equal(0, original.Extents.X);   // the original was not moved
        }

        // --------------------------------------------------- Cairo.FontOptions

        [Fact]
        public void Font_options_report_back_what_was_set_on_them()
        {
            using var options = new FontOptions
            {
                Antialias = Antialias.Gray,
                HintStyle = HintStyle.Full,
                HintMetrics = HintMetrics.Off,
                SubpixelOrder = SubpixelOrder.Bgr,
            };

            Assert.Equal(Antialias.Gray, options.Antialias);
            Assert.Equal(HintStyle.Full, options.HintStyle);
            Assert.Equal(HintMetrics.Off, options.HintMetrics);
            Assert.Equal(SubpixelOrder.Bgr, options.SubpixelOrder);
            Assert.Equal(Status.Success, options.Status);
        }

        [Fact]
        public void Font_options_compare_by_value_and_a_copy_is_equal()
        {
            using var options = new FontOptions { Antialias = Antialias.Subpixel };
            using var copy = options.Copy();

            Assert.True(options == copy);
            Assert.Equal(options.GetHashCode(), copy.GetHashCode());

            copy.Antialias = Antialias.None;

            Assert.True(options != copy);
        }

        [Fact]
        public void Merging_font_options_takes_the_settings_that_were_not_default()
        {
            using var target = new FontOptions();
            using var source = new FontOptions { HintStyle = HintStyle.Slight };

            target.Merge(source);

            Assert.Equal(HintStyle.Slight, target.HintStyle);
        }

        // ------------------------------------------------------- Cairo.Pattern

        [Fact]
        public void A_solid_pattern_paints_the_colour_it_was_built_from()
        {
            using var surface = new ImageSurface(Format.Argb32, 4, 4);
            using (var cr = new Context(surface))
            using (var pattern = new SolidPattern(new Color(0, 0, 1)))
            {
                cr.SetSource(pattern);
                cr.Paint();
            }

            Assert.Equal(255, PixelAt(surface, 2, 2).b);
        }

        [Fact]
        public void A_linear_gradient_is_darker_at_one_end_than_the_other()
        {
            using var surface = new ImageSurface(Format.Argb32, 100, 4);
            using (var cr = new Context(surface))
            using (var gradient = new LinearGradient(0, 0, 100, 0))
            {
                gradient.AddColorStop(0, new Color(0, 0, 0));
                gradient.AddColorStop(1, new Color(1, 1, 1));

                cr.SetSource(gradient);
                cr.Paint();
            }

            var left = PixelAt(surface, 2, 2).r;
            var middle = PixelAt(surface, 50, 2).r;
            var right = PixelAt(surface, 97, 2).r;

            Assert.True(left < middle && middle < right,
                        $"a left-to-right gradient should brighten: {left}, {middle}, {right}");
        }

        [Fact]
        public void A_surface_pattern_repeats_when_told_to()
        {
            // A 2x2 tile with one black pixel, extended by repetition, must put
            // that pixel down every two units across a larger surface.
            using var tile = new ImageSurface(Format.Argb32, 2, 2);
            using (var cr = new Context(tile))
            {
                cr.SetSourceRGBA(0, 0, 0, 1);
                cr.Rectangle(0, 0, 1, 1);
                cr.Fill();
            }

            using var surface = new ImageSurface(Format.Argb32, 8, 8);
            using (var cr = new Context(surface))
            using (var pattern = new SurfacePattern(tile) { Extend = Extend.Repeat })
            {
                cr.SetSource(pattern);
                cr.Paint();
            }

            Assert.Equal(255, PixelAt(surface, 0, 0).a);
            Assert.Equal(255, PixelAt(surface, 4, 4).a);   // the tile repeated
            Assert.Equal(0, PixelAt(surface, 5, 5).a);     // the empty part of it
        }

        [Fact]
        public void A_clip_stops_paint_reaching_outside_it()
        {
            using var surface = new ImageSurface(Format.Argb32, 20, 20);
            using (var cr = new Context(surface))
            {
                cr.Rectangle(5, 5, 5, 5);
                cr.Clip();

                cr.SetSourceRGB(1, 0, 0);
                cr.Paint();
            }

            Assert.Equal(255, PixelAt(surface, 7, 7).a);
            Assert.Equal(0, PixelAt(surface, 15, 15).a);
        }
    }
}
