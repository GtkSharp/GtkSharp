using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cairo;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The third pass over <c>CairoSharp</c>, after <c>CairoTests</c> and
    /// <c>CairoTextAndPathTests</c>: the path data union, joins and caps, the
    /// compositing operators, groups with a content type, scaled fonts and font
    /// faces, the pattern subclasses and the region operations neither of the
    /// earlier files reached.
    /// </summary>
    /// <remarks>
    /// Same discipline throughout — render to an <c>ImageSurface</c> and read the
    /// pixels back, so the assertion is what landed in the buffer. Where a fact
    /// can be settled outside the library it is: which pixels a miter join covers
    /// is trigonometry, what the <c>Add</c> operator produces is arithmetic, and
    /// the bytes in test-owned memory are the bytes the test put there.
    ///
    /// Nothing here touches Gtk, so nothing needs the fixture's thread; Cairo has
    /// no thread affinity of its own.
    /// </remarks>
    public class CairoDeepTests : GtkTestBase
    {
        public CairoDeepTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>Reads a pixel as (b, g, r, a) — ARGB32 is premultiplied and
        /// stored little-endian, so byte 0 is blue.</summary>
        private static (byte b, byte g, byte r, byte a) PixelAt(ImageSurface surface, int x, int y)
        {
            surface.Flush();
            var data = surface.Data;
            var offset = y * surface.Stride + x * 4;
            return (data[offset], data[offset + 1], data[offset + 2], data[offset + 3]);
        }

        // ------------------------------------------------------------ path data

        /// <summary>
        /// Walks a copied path into (operation, points) pairs. Cairo hands the
        /// path over as a flat array of a union: element 0 of a run is the
        /// header saying which operation it is and how many array slots it
        /// occupies, and the slots after it are the points.
        /// </summary>
        private static List<(PathDataType Type, PointD[] Points)> Walk(Path path)
        {
            var result = new List<(PathDataType, PointD[])>();
            var data = path.Data;

            for (var i = 0; i < data.Length; )
            {
                var header = data[i].Header;
                var points = new PointD[header.Length - 1];

                for (var p = 0; p < points.Length; p++)
                    points[p] = new PointD(data[i + 1 + p].Point.X, data[i + 1 + p].Point.Y);

                result.Add((header.Type, points));
                i += header.Length;
            }

            return result;
        }

        [Fact]
        public void A_copied_path_spells_out_the_operations_that_built_it()
        {
            // PathData is a LayoutKind.Explicit union of a {type,length} header
            // and an {x,y} point at the same offset. If that overlay is wrong
            // the coordinates come back as garbage rather than as an error, so
            // the assertion has to be the exact numbers this test chose.
            using var surface = new ImageSurface(Format.Argb32, 40, 40);
            using var cr = new Context(surface);

            cr.MoveTo(10, 20);
            cr.LineTo(30, 20);
            cr.ClosePath();

            using var path = cr.CopyPath();

            Assert.Equal(Status.Success, path.Status);
            Assert.Equal(path.Data.Length, path.NumData);

            var elements = Walk(path);

            Assert.Equal(PathDataType.MoveTo, elements[0].Type);
            Assert.Equal(10, elements[0].Points[0].X, 6);
            Assert.Equal(20, elements[0].Points[0].Y, 6);

            Assert.Equal(PathDataType.LineTo, elements[1].Type);
            Assert.Equal(30, elements[1].Points[0].X, 6);
            Assert.Equal(20, elements[1].Points[0].Y, 6);

            Assert.Equal(PathDataType.ClosePath, elements[2].Type);
            Assert.Empty(elements[2].Points);

            // The trap: closing a subpath is not the last thing in the array.
            // Cairo appends an explicit MoveTo back to where the subpath began,
            // so code that assumes ClosePath ends the path misses an element.
            Assert.Equal(4, elements.Count);
            Assert.Equal(PathDataType.MoveTo, elements[3].Type);
            Assert.Equal(10, elements[3].Points[0].X, 6);
            Assert.Equal(20, elements[3].Points[0].Y, 6);
        }

        [Fact]
        public void A_curve_is_one_element_of_three_points_and_flattening_replaces_it()
        {
            using var surface = new ImageSurface(Format.Argb32, 60, 60);
            using var cr = new Context(surface);

            cr.MoveTo(5, 5);
            cr.CurveTo(15, 0, 35, 20, 45, 15);

            using var curved = cr.CopyPath();
            var elements = Walk(curved);

            Assert.Equal(2, elements.Count);
            Assert.Equal(PathDataType.CurveTo, elements[1].Type);

            // Three points in one element: the two control points and the end.
            Assert.Equal(3, elements[1].Points.Length);
            Assert.Equal(15, elements[1].Points[0].X, 6);
            Assert.Equal(35, elements[1].Points[1].X, 6);
            Assert.Equal(45, elements[1].Points[2].X, 6);
            Assert.Equal(15, elements[1].Points[2].Y, 6);

            using var flat = cr.CopyPathFlat();
            var flattened = Walk(flat);

            Assert.DoesNotContain(flattened, e => e.Type == PathDataType.CurveTo);
            Assert.True(flattened.Count > 2, "flattening a curve must produce several line segments");
            Assert.All(flattened.GetRange(1, flattened.Count - 1),
                       e => Assert.Equal(PathDataType.LineTo, e.Type));

            // The segments still end where the curve did.
            var last = flattened[flattened.Count - 1].Points[0];
            Assert.Equal(45, last.X, 3);
            Assert.Equal(15, last.Y, 3);
        }

        // ------------------------------------------------------- joins and caps

        /// <summary>
        /// Strokes a right-angled corner at (60,60) with a 20-wide line, and
        /// reports the alpha at (68,68) — a point in the outer corner that only
        /// a miter join reaches.
        /// </summary>
        private static byte CornerAlpha(LineJoin join, double miterLimit)
        {
            using var surface = new ImageSurface(Format.Argb32, 80, 80);
            using (var cr = new Context(surface))
            {
                cr.Antialias = Antialias.None;
                cr.LineWidth = 20;
                cr.LineJoin = join;
                cr.MiterLimit = miterLimit;
                cr.SetSourceRGB(0, 0, 0);
                cr.MoveTo(20, 60);
                cr.LineTo(60, 60);
                cr.LineTo(60, 20);
                cr.Stroke();
            }

            // Sanity: the middle of the vertical arm is painted whatever the
            // join is, so a zero at (68,68) means the join and not a blank run.
            Assert.Equal(255, PixelAt(surface, 65, 55).a);

            return PixelAt(surface, 68, 68).a;
        }

        [Fact]
        public void A_miter_join_fills_the_outer_corner_that_bevel_and_round_cut_away()
        {
            // Half the line width is 10, so the two arms cover everything except
            // the 10x10 square outside the corner. A miter fills that square; a
            // bevel fills only the triangle under its diagonal, and a round join
            // only the quarter disc of radius 10. The sample at (68,68) is
            // 12.0 from the joint and 7 past the bevel's diagonal, so all three
            // answers are decided by geometry rather than by tolerance.
            Assert.Equal(255, CornerAlpha(LineJoin.Miter, 10));
            Assert.Equal(0, CornerAlpha(LineJoin.Bevel, 10));
            Assert.Equal(0, CornerAlpha(LineJoin.Round, 10));
        }

        [Fact]
        public void The_miter_limit_turns_a_join_into_a_bevel_when_the_spike_grows_too_long()
        {
            // The miter ratio for an angle t is 1/sin(t/2); a right angle gives
            // 1/sin(45) = 1.4142. The limit is compared against exactly that, so
            // bracketing it decides the outcome without measuring anything.
            Assert.Equal(1.4142135623730951, 1 / Math.Sin(Math.PI / 4), 12);

            Assert.Equal(255, CornerAlpha(LineJoin.Miter, 1.5));
            Assert.Equal(0, CornerAlpha(LineJoin.Miter, 1.3));
        }

        [Fact]
        public void The_line_cap_decides_what_lies_beyond_the_end_of_a_line()
        {
            static byte AlphaAt(LineCap cap, int x, int y)
            {
                using var surface = new ImageSurface(Format.Argb32, 60, 40);
                using (var cr = new Context(surface))
                {
                    cr.Antialias = Antialias.None;
                    cr.LineWidth = 10;
                    cr.LineCap = cap;
                    cr.SetSourceRGB(0, 0, 0);
                    cr.MoveTo(20, 20);
                    cr.LineTo(50, 20);
                    cr.Stroke();
                }
                return PixelAt(surface, x, y).a;
            }

            // Butt stops dead at the endpoint; the other two overhang it by half
            // the line width.
            Assert.Equal(0, AlphaAt(LineCap.Butt, 17, 20));
            Assert.Equal(255, AlphaAt(LineCap.Square, 17, 20));
            Assert.Equal(255, AlphaAt(LineCap.Round, 17, 20));

            // (16,24) separates the other two: it is inside the square cap's
            // corner but 5.7 from the endpoint, so outside the round one.
            Assert.Equal(255, AlphaAt(LineCap.Square, 16, 24));
            Assert.Equal(0, AlphaAt(LineCap.Round, 16, 24));
        }

        // ---------------------------------------------------------- compositing

        [Fact]
        public void The_Source_operator_replaces_the_destination_where_Over_blends_with_it()
        {
            static (byte b, byte g, byte r, byte a) Composite(Operator op)
            {
                using var surface = new ImageSurface(Format.Argb32, 4, 4);
                using (var cr = new Context(surface))
                {
                    cr.SetSourceRGB(1, 0, 0);
                    cr.Paint();

                    cr.Operator = op;
                    cr.SetSourceRGBA(0, 0, 1, 0.5);
                    cr.Paint();
                }
                return PixelAt(surface, 2, 2);
            }

            // Over: half of the blue on top of half of the red still showing.
            var over = Composite(Operator.Over);
            Assert.InRange(over.r, 126, 130);
            Assert.InRange(over.b, 126, 130);
            Assert.Equal(255, over.a);

            // Source: the destination is discarded, alpha included, so what is
            // left is a half-transparent blue and no red at all.
            var source = Composite(Operator.Source);
            Assert.Equal(0, source.r);
            Assert.InRange(source.b, 126, 130);
            Assert.InRange(source.a, 126, 130);
        }

        [Fact]
        public void The_Add_operator_sums_the_channels_and_saturates_at_one()
        {
            using var surface = new ImageSurface(Format.Argb32, 4, 4);
            using (var cr = new Context(surface))
            {
                cr.SetSourceRGB(0.25, 0, 0);
                cr.Paint();

                Assert.InRange(PixelAt(surface, 2, 2).r, 62, 66);

                cr.Operator = Operator.Add;
                cr.Paint();
            }

            var (_, _, r, a) = PixelAt(surface, 2, 2);

            Assert.InRange(r, 126, 130);   // 0.25 + 0.25
            Assert.Equal(255, a);          // 1 + 1, clamped
        }

        [Fact]
        public void The_In_operator_keeps_the_source_only_where_the_destination_was()
        {
            using var surface = new ImageSurface(Format.Argb32, 40, 40);
            using (var cr = new Context(surface))
            {
                cr.SetSourceRGB(0, 0, 0);
                cr.Rectangle(0, 0, 20, 40);   // the left half only
                cr.Fill();

                // "In" is not a clip: it rewrites the whole target, keeping the
                // source scaled by the destination's alpha. So painting red over
                // everything erases the right half rather than filling it.
                cr.Operator = Operator.In;
                cr.SetSourceRGB(1, 0, 0);
                cr.Paint();
            }

            Assert.Equal((0, 0, 255, 255), PixelAt(surface, 10, 20));
            Assert.Equal(0, PixelAt(surface, 30, 20).a);
        }

        // ---------------------------------------------------------------- groups

        [Fact]
        public void A_group_pushed_for_colour_alone_has_no_alpha_to_be_transparent_with()
        {
            static (Content content, byte alpha) DrawNothingIntoAGroup(Content? requested)
            {
                using var surface = new ImageSurface(Format.Argb32, 20, 20);
                Content groupContent;

                using (var cr = new Context(surface))
                {
                    if (requested.HasValue)
                        cr.PushGroup(requested.Value);
                    else
                        cr.PushGroup();

                    using (var target = cr.GetGroupTarget())
                        groupContent = target.Content;

                    using var group = cr.PopGroup();
                    cr.SetSource(group);
                    cr.Paint();
                }

                return (groupContent, PixelAt(surface, 10, 10).a);
            }

            // The default group carries alpha, so an empty one paints nothing.
            var normal = DrawNothingIntoAGroup(null);
            Assert.Equal(Content.ColorAlpha, normal.content);
            Assert.Equal(0, normal.alpha);

            // A colour-only group has nowhere to record "nothing here", so the
            // same empty group paints opaque black over the whole surface.
            var opaque = DrawNothingIntoAGroup(Content.Color);
            Assert.Equal(Content.Color, opaque.content);
            Assert.Equal(255, opaque.alpha);
        }

        [Fact]
        public void Masking_with_a_gradient_fades_the_paint_across_it()
        {
            using var surface = new ImageSurface(Format.Argb32, 100, 8);
            using (var cr = new Context(surface))
            using (var mask = new LinearGradient(0, 0, 100, 0))
            {
                // Only the alpha of the mask matters; its colour is ignored.
                mask.AddColorStop(0, new Color(0, 1, 0, 0));
                mask.AddColorStop(1, new Color(0, 1, 0, 1));

                cr.SetSourceRGB(1, 0, 0);
                cr.Mask(mask);
            }

            var left = PixelAt(surface, 2, 4);
            var middle = PixelAt(surface, 50, 4);
            var right = PixelAt(surface, 97, 4);

            Assert.True(left.a < middle.a && middle.a < right.a,
                        $"the mask should fade in: {left.a}, {middle.a}, {right.a}");
            Assert.InRange(middle.a, 120, 136);

            // The mask took the source's colour, not its own: what is on the
            // surface is red, premultiplied, so red equals alpha everywhere.
            Assert.Equal(middle.a, middle.r);
            Assert.Equal(0, middle.g);
        }

        // -------------------------------------------------------------- patterns

        [Fact]
        public void The_source_read_back_off_a_context_is_the_pattern_that_was_set()
        {
            using var surface = new ImageSurface(Format.Argb32, 8, 8);
            using var cr = new Context(surface);

            cr.SetSourceRGBA(0.25, 0.5, 0.75, 0.5);

            using (var source = cr.GetSource())
            {
                // Pattern.Lookup picks the subclass from the native pattern type,
                // so a plain SetSourceRGBA has to come back as a SolidPattern
                // with the colour still readable off it.
                var solid = Assert.IsType<SolidPattern>(source);
                Assert.Equal(PatternType.Solid, solid.PatternType);
                Assert.Equal(0.25, solid.Color.R, 6);
                Assert.Equal(0.5, solid.Color.G, 6);
                Assert.Equal(0.75, solid.Color.B, 6);
                Assert.Equal(0.5, solid.Color.A, 6);
            }

            using (var gradient = new RadialGradient(1, 2, 3, 4, 5, 6))
                cr.SetSource(gradient);

            using (var source = cr.GetSource())
            {
                Assert.IsType<RadialGradient>(source);
                Assert.Equal(PatternType.Radial, source.PatternType);
            }
        }

        [Fact]
        public void A_radial_gradient_is_symmetric_about_its_centre_and_falls_off_outwards()
        {
            using var surface = new ImageSurface(Format.Argb32, 100, 100);
            using (var cr = new Context(surface))
            using (var gradient = new RadialGradient(50, 50, 0, 50, 50, 50))
            {
                gradient.AddColorStop(0, new Color(1, 1, 1));
                gradient.AddColorStopRgb(1, new Color(0, 0, 0));

                Assert.Equal(2, gradient.ColorStopCount);

                cr.SetSource(gradient);
                cr.Paint();
            }

            Assert.True(PixelAt(surface, 50, 50).r > PixelAt(surface, 75, 50).r);
            Assert.True(PixelAt(surface, 75, 50).r > PixelAt(surface, 98, 50).r);

            // Pixel centres sit on half-integers, so x and 99-x are the same
            // distance from a centre at 50: a correct radial gradient has to
            // give them the same value, in either axis and on the diagonal.
            Assert.Equal(PixelAt(surface, 25, 50).r, PixelAt(surface, 74, 50).r);
            Assert.Equal(PixelAt(surface, 50, 25).r, PixelAt(surface, 50, 74).r);
            Assert.Equal(PixelAt(surface, 30, 30).r, PixelAt(surface, 69, 69).r);
        }

        [Fact]
        public void A_pattern_matrix_transforms_the_pattern_and_not_the_drawing()
        {
            static byte[] Row(Matrix matrix)
            {
                using var surface = new ImageSurface(Format.Argb32, 100, 4);
                using (var cr = new Context(surface))
                using (var gradient = new LinearGradient(0, 0, 100, 0))
                {
                    gradient.AddColorStop(0, new Color(0, 0, 0));
                    gradient.AddColorStop(1, new Color(1, 1, 1));

                    if (matrix != null)
                    {
                        gradient.Matrix = matrix;
                        Assert.Equal(matrix.Xx, gradient.Matrix.Xx, 6);
                    }

                    cr.SetSource(gradient);
                    cr.Paint();
                }

                var row = new byte[100];
                for (var x = 0; x < 100; x++)
                    row[x] = PixelAt(surface, x, 2).r;
                return row;
            }

            var plain = Row(null);

            var doubled = new Matrix();
            doubled.InitScale(2, 1);
            var compressed = Row(doubled);

            // The matrix maps user space into pattern space, so scaling it by
            // two makes the gradient finish in half the distance -- the opposite
            // of what scaling the drawing would do. x=25 now shows what x=50
            // showed before.
            Assert.InRange(compressed[25], plain[50] - 2, plain[50] + 2);

            // Past the halfway point the gradient has run out and pads.
            Assert.Equal(255, compressed[80]);
            Assert.True(plain[80] < 255);
        }

        [Fact]
        public void Nearest_filtering_keeps_a_tile_edge_hard_where_bilinear_blends_it()
        {
            static bool HasIntermediateValues(Filter filter)
            {
                using var tile = new ImageSurface(Format.Argb32, 2, 1);
                using (var cr = new Context(tile))
                {
                    cr.SetSourceRGB(0, 0, 0);
                    cr.Rectangle(0, 0, 1, 1);
                    cr.Fill();
                    cr.SetSourceRGB(1, 1, 1);
                    cr.Rectangle(1, 0, 1, 1);
                    cr.Fill();
                }

                using var surface = new ImageSurface(Format.Argb32, 40, 4);
                using (var cr = new Context(surface))
                using (var pattern = new SurfacePattern(tile) { Filter = filter, Extend = Extend.Pad })
                {
                    Assert.Equal(filter, pattern.Filter);

                    cr.Scale(20, 4);
                    cr.SetSource(pattern);
                    cr.Paint();
                }

                for (var x = 0; x < 40; x++)
                {
                    var r = PixelAt(surface, x, 2).r;
                    if (r != 0 && r != 255)
                        return true;
                }
                return false;
            }

            Assert.False(HasIntermediateValues(Filter.Nearest),
                         "nearest-neighbour sampling can only ever copy a source pixel");
            Assert.True(HasIntermediateValues(Filter.Bilinear),
                        "bilinear sampling must interpolate across the tile boundary");
        }

        // -------------------------------------------------------------- surfaces

        [Fact]
        public void The_device_scale_multiplies_the_size_of_what_is_drawn()
        {
            using var surface = new ImageSurface(Format.Argb32, 40, 40);

            // Must be set before the context is created: a context captures the
            // surface's device transform when it is made.
            surface.DeviceScale = new PointD(2, 2);

            Assert.Equal(2, surface.DeviceScale.X, 6);
            Assert.Equal(2, surface.DeviceScale.Y, 6);

            using (var cr = new Context(surface))
            {
                cr.SetSourceRGB(0, 0, 0);
                cr.Rectangle(0, 0, 5, 5);
                cr.Fill();
            }

            // Five user units became ten device pixels.
            Assert.Equal(255, PixelAt(surface, 9, 9).a);
            Assert.Equal(0, PixelAt(surface, 11, 11).a);
        }

        [Fact]
        public void The_context_target_is_the_very_surface_it_was_created_for()
        {
            using var surface = new ImageSurface(Format.Argb32, 32, 16);
            using var cr = new Context(surface);

            using var target = cr.GetTarget();

            // Surface.Lookup dispatches on the native surface type, so an image
            // surface has to come back as an ImageSurface and not as the base
            // class -- otherwise Width, Height and Data are all unreachable.
            var image = Assert.IsType<ImageSurface>(target);

            Assert.Equal(SurfaceType.Image, image.SurfaceType);
            Assert.Equal(surface.Handle, image.Handle);
            Assert.Equal(32, image.Width);
            Assert.Equal(16, image.Height);

            // The second wrapper is borrowed, not a copy: drawing through it
            // shows up on the original, and disposing it must not free the one
            // this test still holds.
            using (var other = new Context(image))
            {
                other.SetSourceRGB(0, 0, 1);
                other.Paint();
            }

            Assert.Equal(255, PixelAt(surface, 5, 5).b);
        }

        [Fact]
        public void Bytes_written_straight_into_a_surface_and_marked_dirty_are_what_cairo_composites()
        {
            using var surface = new ImageSurface(Format.Argb32, 8, 8);
            using (var cr = new Context(surface))
            {
                cr.SetSourceRGB(0, 0, 0);
                cr.Paint();
            }

            // Flush first, or cairo may still be holding drawing of its own that
            // would land on top of what is written here.
            surface.Flush();

            var offset = 3 * surface.Stride + 2 * 4;
            Marshal.WriteByte(surface.DataPtr, offset, 255);       // b
            Marshal.WriteByte(surface.DataPtr, offset + 1, 0);     // g
            Marshal.WriteByte(surface.DataPtr, offset + 2, 0);     // r
            Marshal.WriteByte(surface.DataPtr, offset + 3, 255);   // a

            surface.MarkDirty(new Rectangle(2, 3, 1, 1));

            Assert.Equal((255, 0, 0, 255), PixelAt(surface, 2, 3));

            // And cairo agrees: compositing the surface elsewhere carries the
            // byte this test wrote, rather than a cached copy of what it drew.
            using var copy = new ImageSurface(Format.Argb32, 8, 8);
            using (var cr = new Context(copy))
            {
                cr.SetSource(surface);
                cr.Paint();
            }

            Assert.Equal(255, PixelAt(copy, 2, 3).b);
            Assert.Equal(0, PixelAt(copy, 0, 0).b);
        }

        [Fact]
        public void An_image_surface_over_caller_owned_memory_draws_into_that_memory()
        {
            const int width = 8, height = 8, stride = width * 4;

            var buffer = Marshal.AllocHGlobal(stride * height);
            try
            {
                for (var i = 0; i < stride * height; i++)
                    Marshal.WriteByte(buffer, i, 0);

                using (var surface = new ImageSurface(buffer, Format.Argb32, width, height, stride))
                {
                    Assert.Equal(Status.Success, surface.Status);
                    Assert.Equal(stride, surface.Stride);

                    using (var cr = new Context(surface))
                    {
                        cr.SetSourceRGB(0, 1, 0);
                        cr.Rectangle(0, 0, 4, 4);
                        cr.Fill();
                    }

                    surface.Flush();
                }

                // The oracle is memory this test allocated and can still read
                // after the surface is gone: cairo must have drawn into it
                // rather than into a copy of its own.
                Assert.Equal(255, Marshal.ReadByte(buffer, 1 * stride + 1 * 4 + 1));   // green
                Assert.Equal(255, Marshal.ReadByte(buffer, 1 * stride + 1 * 4 + 3));   // alpha
                Assert.Equal(0, Marshal.ReadByte(buffer, 6 * stride + 6 * 4 + 3));     // untouched
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        // --------------------------------------------------------------- regions

        [Fact]
        public void Xor_leaves_only_the_ground_that_one_region_covered()
        {
            using var region = new Region(new RectangleInt { X = 0, Y = 0, Width = 10, Height = 10 });
            using var other = new Region(new RectangleInt { X = 5, Y = 0, Width = 10, Height = 10 });

            Assert.Equal(Status.Success, region.Xor(other));

            // The middle strip belonged to both, so it cancelled, leaving two
            // separate rectangles under one span of extents.
            Assert.Equal(2, region.NumRectangles);
            Assert.Equal(0, region.Extents.X);
            Assert.Equal(15, region.Extents.Width);

            Assert.True(region.ContainsPoint(2, 5));
            Assert.False(region.ContainsPoint(7, 5));
            Assert.True(region.ContainsPoint(12, 5));

            var rectangles = new[] { region.GetRectangle(0), region.GetRectangle(1) };
            Assert.Contains(rectangles, r => r.X == 0 && r.Width == 5);
            Assert.Contains(rectangles, r => r.X == 10 && r.Width == 5);
        }

        [Fact]
        public void Rectangle_containment_answers_In_Out_or_Part()
        {
            using var region = new Region(new RectangleInt { X = 0, Y = 0, Width = 10, Height = 10 });

            // Named ContainsPoint but overloaded on a rectangle, and it does not
            // answer a bool: it is cairo_region_contains_rectangle, and the
            // three-way answer is the whole point of it.
            Assert.Equal(RegionOverlap.In,
                         region.ContainsPoint(new RectangleInt { X = 2, Y = 2, Width = 3, Height = 3 }));
            Assert.Equal(RegionOverlap.Out,
                         region.ContainsPoint(new RectangleInt { X = 20, Y = 20, Width = 3, Height = 3 }));
            Assert.Equal(RegionOverlap.Part,
                         region.ContainsPoint(new RectangleInt { X = 5, Y = 5, Width = 20, Height = 20 }));
        }

        [Fact]
        public void The_rectangle_forms_of_the_region_operations_change_what_it_covers()
        {
            var rectangles = new[]
            {
                new RectangleInt { X = 0, Y = 0, Width = 5, Height = 5 },
                new RectangleInt { X = 20, Y = 0, Width = 5, Height = 5 },
            };

            using var region = new Region(rectangles);

            Assert.Equal(Status.Success, region.Status);
            Assert.Equal(2, region.NumRectangles);
            Assert.Equal(25, region.Extents.Width);

            // Filling the gap coalesces the two into one.
            Assert.Equal(Status.Success,
                         region.UnionRectangle(new RectangleInt { X = 5, Y = 0, Width = 15, Height = 5 }));
            Assert.Equal(1, region.NumRectangles);

            // Cutting a hole in the middle splits it again.
            Assert.Equal(Status.Success,
                         region.SubtractRectangle(new RectangleInt { X = 10, Y = 0, Width = 5, Height = 5 }));
            Assert.Equal(2, region.NumRectangles);

            Assert.Equal(Status.Success,
                         region.IntersectRectangle(new RectangleInt { X = 0, Y = 0, Width = 10, Height = 5 }));
            Assert.Equal(1, region.NumRectangles);
            Assert.Equal(10, region.Extents.Width);

            // Xor against exactly what is left leaves nothing.
            Assert.Equal(Status.Success,
                         region.XorRectangle(new RectangleInt { X = 0, Y = 0, Width = 10, Height = 5 }));
            Assert.True(region.IsEmpty);
        }

        [Fact]
        public void Equal_regions_hash_alike_and_can_be_used_as_dictionary_keys()
        {
            using var region = new Region(new RectangleInt { X = 1, Y = 2, Width = 3, Height = 4 });
            using var same = new Region(new RectangleInt { X = 1, Y = 2, Width = 3, Height = 4 });
            using var different = new Region(new RectangleInt { X = 9, Y = 2, Width = 3, Height = 4 });

            Assert.True(region.Equals(same));
            Assert.False(region.Equals(different));

            // Equals compares the regions, so the hash has to as well. It used
            // to be the handle's, which meant two equal regions hashed apart and
            // a lookup by an equal key missed -- the classic broken-hash bug,
            // invisible to any test that only compares a value with its copy.
            Assert.Equal(region.GetHashCode(), same.GetHashCode());
            Assert.NotEqual(region.GetHashCode(), different.GetHashCode());

            var map = new Dictionary<Region, string> { { region, "found" } };

            Assert.Equal("found", map[same]);
            Assert.False(map.ContainsKey(different));
        }

        // ---------------------------------------------------------------- matrix

        [Fact]
        public void Multiplying_matrices_applies_the_left_one_first()
        {
            var translate = new Matrix();
            translate.InitTranslate(10, 0);

            var scale = new Matrix();
            scale.InitScale(2, 2);

            // cairo_matrix_multiply(result, a, b) means "a, then b". Getting the
            // order backwards produces a transform that is still a valid matrix,
            // so nothing complains -- the drawing just lands somewhere else.
            var product = Matrix.Multiply(translate, scale);

            double x = 1, y = 1;
            product.TransformPoint(ref x, ref y);
            Assert.Equal(22, x, 6);   // (1+10) * 2
            Assert.Equal(2, y, 6);

            var reversed = Matrix.Multiply(scale, translate);

            double rx = 1, ry = 1;
            reversed.TransformPoint(ref rx, ref ry);
            Assert.Equal(12, rx, 6);  // 1*2 + 10

            // The instance form multiplies in place, on the same side.
            translate.Multiply(scale);
            Assert.True(translate == product);
        }

        [Fact]
        public void Rotate_and_Translate_compose_onto_the_matrix_instead_of_replacing_it()
        {
            var matrix = new Matrix();
            matrix.InitScale(2, 2);
            matrix.Rotate(Math.PI / 2);

            // The new rotation happens first and the existing scale second, so
            // (1,0) turns to (0,1) and is then doubled. A matrix that had been
            // replaced rather than composed would give (0,1).
            double x = 1, y = 0;
            matrix.TransformPoint(ref x, ref y);
            Assert.Equal(0, x, 6);
            Assert.Equal(2, y, 6);

            var translated = new Matrix();
            translated.InitScale(2, 2);
            translated.Translate(3, 0);

            double tx = 0, ty = 0;
            translated.TransformPoint(ref tx, ref ty);
            Assert.Equal(6, tx, 6);   // moved by three, then scaled
            Assert.Equal(0, ty, 6);
        }

        [Fact]
        public void Distinct_matrices_do_not_all_hash_to_the_same_value()
        {
            var matrices = new[]
            {
                new Matrix(1, 0, 0, 1, 0, 0),
                new Matrix(2, 0, 0, 1, 0, 0),
                new Matrix(1, 2, 0, 1, 0, 0),
                new Matrix(1, 0, 3, 1, 0, 0),
                new Matrix(1, 0, 0, 4, 0, 0),
                new Matrix(1, 0, 0, 1, 5, 0),
                new Matrix(1, 0, 0, 1, 0, 6),
                new Matrix(1.5, 0, 0, 1, 0, 0),
                new Matrix(1.9, 0, 0, 1, 0, 0),
            };

            var hashes = new HashSet<int>();
            foreach (var matrix in matrices)
                hashes.Add(matrix.GetHashCode());

            // Every one of these hashed to zero: the old implementation xored
            // each field against itself, and truncated to int besides, so the
            // last two were indistinguishable even in principle. A dictionary
            // of matrices degenerated into a linear scan.
            Assert.Equal(matrices.Length, hashes.Count);

            // Still consistent with equality, which is the point of a hash.
            Assert.Equal(new Matrix(1, 2, 3, 4, 5, 6).GetHashCode(),
                         new Matrix(1, 2, 3, 4, 5, 6).GetHashCode());
        }

        [Fact]
        public void A_matrix_can_be_compared_against_null()
        {
            var matrix = new Matrix();
            Matrix missing = null;

            // Matrix is a class, so this is the first thing any caller writes --
            // and the operator used to dereference both sides unconditionally.
            Assert.False(matrix == null);
            Assert.True(matrix != null);
            Assert.True(missing == null);
            Assert.False(missing != null);
            Assert.False(matrix.Equals(null));
        }

        // ---------------------------------------------------- fonts and faces

        [Fact]
        public void A_scaled_font_carries_the_matrix_it_was_built_with_and_measures_by_it()
        {
            using var surface = new ImageSurface(Format.Argb32, 10, 10);
            using var cr = new Context(surface);

            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);

            using var face = cr.GetContextFontFace();
            Assert.Equal(Status.Success, face.Status);
            Assert.True(face.ReferenceCount >= 1);

            var identity = new Matrix();
            // Hint metrics round the extents to whole pixels, which would stop
            // them scaling linearly with the matrix.
            using var options = new FontOptions { HintMetrics = HintMetrics.Off };

            var small = new Matrix();
            small.InitScale(10, 10);
            var large = new Matrix();
            large.InitScale(40, 40);

            using var tenth = new ScaledFont(face, small, identity, options);
            using var fortieth = new ScaledFont(face, large, identity, options);

            Assert.Equal(Status.Success, fortieth.Status);

            // cairo_scaled_font_get_font_matrix writes through a pointer the
            // caller provides. Binding it as an `out` parameter on a class made
            // it a double pointer and corrupted the stack, so reading it back is
            // a regression pin as much as a round-trip.
            Assert.Equal(10, tenth.FontMatrix.Xx, 6);
            Assert.Equal(10, tenth.FontMatrix.Yy, 6);
            Assert.Equal(40, fortieth.FontMatrix.Xx, 6);
            Assert.Equal(0, fortieth.FontMatrix.X0, 6);

            var ascent = tenth.FontExtents.Ascent;
            Assert.True(ascent > 0, "a scalable font must have a positive ascent");
            Assert.InRange(fortieth.FontExtents.Ascent, 3.9 * ascent, 4.1 * ascent);
        }

        [Fact]
        public void A_scaled_font_measures_the_same_on_whatever_context_it_is_installed()
        {
            const string sample = "Hamburgefonstiv";

            using var surfaceA = new ImageSurface(Format.Argb32, 10, 10);
            using var a = new Context(surfaceA);
            a.SelectFontFace("Serif", FontSlant.Italic, FontWeight.Bold);
            a.SetFontSize(24);

            using var font = a.GetScaledFont();

            Assert.Equal(24, font.FontMatrix.Xx, 6);
            Assert.Equal(a.FontExtents.Ascent, font.FontExtents.Ascent, 6);

            using var surfaceB = new ImageSurface(Format.Argb32, 10, 10);
            using var b = new Context(surfaceB);
            b.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
            b.SetFontSize(8);

            var before = b.TextExtents(sample).Width;

            b.SetScaledFont(font);

            // A scaled font fixes the face, the size and the transform together,
            // so the second context now measures exactly what the first does.
            Assert.Equal(a.TextExtents(sample).Width, b.TextExtents(sample).Width, 6);
            Assert.True(b.TextExtents(sample).Width > before,
                        "24pt text is wider than 8pt whatever family the machine resolves");
        }

        [Fact]
        public void A_font_face_taken_from_one_context_can_be_installed_on_another()
        {
            const string sample = "Hamburgefonstiv";

            using var surfaceA = new ImageSurface(Format.Argb32, 10, 10);
            using var a = new Context(surfaceA);
            a.SelectFontFace("Serif", FontSlant.Italic, FontWeight.Bold);
            a.SetFontSize(20);

            using var face = a.GetContextFontFace();

            using var surfaceB = new ImageSurface(Format.Argb32, 10, 10);
            using var b = new Context(surfaceB);
            b.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
            b.SetFontSize(20);

            b.SetContextFontFace(face);

            // The face carries the family, slant and weight but not the size, so
            // at a matching size the two contexts must agree exactly. That holds
            // whether or not this machine resolves the two families differently,
            // which a width comparison alone would depend on.
            Assert.Equal(a.TextExtents(sample).Width, b.TextExtents(sample).Width, 6);

            using (var installed = b.GetContextFontFace())
                Assert.Equal(face.Handle, installed.Handle);

            // Null is the documented way back to cairo's default face, and it is
            // the one argument the binding has to special-case.
            b.SetContextFontFace(null);

            using var restored = b.GetContextFontFace();
            Assert.NotEqual(face.Handle, restored.Handle);
            Assert.Equal(Status.Success, restored.Status);
        }
    }
}
