using System;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Cairo's text and glyph API: the toy text calls, scaled fonts, and drawing
    /// by glyph index.
    /// </summary>
    /// <remarks>
    /// <c>CairoSharp</c> has no <c>.metadata</c> and nothing generated — every
    /// line of it is hand-written, so nothing about it is checked by compiling.
    /// <c>FontFace</c>, <c>Glyph</c> and <c>ShowGlyphs</c> had no mention in the
    /// suite at all, and the glyph path carries its own marshalling: a managed
    /// <c>Glyph[]</c> copied into unmanaged memory by hand.
    ///
    /// Nothing here asserts an absolute measurement. Which fonts exist is a fact
    /// about the machine, so every test compares two results from the same font
    /// or pairs a drawing with a control that must leave no ink.
    /// </remarks>
    public class CairoTextTests : GtkTestBase
    {
        public CairoTextTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>True when <paramref name="draw"/> put anything on the surface.</summary>
        static bool LeavesInk(Action<Cairo.Context> draw, int size = 40)
        {
            using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, size, size);
            using (var cr = new Cairo.Context(surface))
                draw(cr);

            surface.Flush();

            var data = surface.Data;
            int stride = surface.Stride;

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    if (data[y * stride + x * 4 + 3] != 0)
                        return true;

            return false;
        }

        /// <summary>The x of the leftmost painted pixel, or -1.</summary>
        static int LeftmostInk(Action<Cairo.Context> draw, int width = 80, int height = 40)
        {
            using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, width, height);
            using (var cr = new Cairo.Context(surface))
                draw(cr);

            surface.Flush();

            var data = surface.Data;
            int stride = surface.Stride;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (data[y * stride + x * 4 + 3] != 0)
                        return x;

            return -1;
        }

        static void UseFont(Cairo.Context cr, double size = 20)
        {
            cr.SelectFontFace("sans", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
            cr.SetFontSize(size);
            cr.SetSourceRGBA(0, 0, 0, 1);
        }

        // -------------------------------------------------------- the Glyph struct

        [Fact]
        public void A_glyph_keeps_the_index_and_position_it_was_built_with()
        {
            var glyph = new Cairo.Glyph(42, 1.5, 2.5);

            Assert.Equal(42L, glyph.Index);
            Assert.Equal(1.5, glyph.X);
            Assert.Equal(2.5, glyph.Y);

            glyph.X = 9.5;
            Assert.Equal(9.5, glyph.X);
        }

        [Fact]
        public void Two_glyphs_are_equal_only_when_all_three_fields_match()
        {
            var glyph = new Cairo.Glyph(1, 2, 3);

            Assert.True(glyph == new Cairo.Glyph(1, 2, 3));
            Assert.True(glyph.Equals(new Cairo.Glyph(1, 2, 3)));

            // One test per field, because an equality that ignores a field passes
            // any check that only varies one of the others.
            Assert.True(glyph != new Cairo.Glyph(9, 2, 3));
            Assert.True(glyph != new Cairo.Glyph(1, 9, 3));
            Assert.True(glyph != new Cairo.Glyph(1, 2, 9));

            Assert.False(glyph.Equals("not a glyph"));
        }

        [Fact]
        public void Glyphs_that_differ_only_in_the_order_of_their_numbers_hash_differently()
        {
            // GetHashCode folded the three fields with XOR, which is commutative:
            // every permutation of the same numbers came out with one hash. Same
            // defect StructBase.GenHashCode was fixed for, and the same reason it
            // matters -- a glyph run is mostly permutations of small numbers, so
            // the collisions are not the rare case but the usual one.
            var glyph = new Cairo.Glyph(1, 2, 3);

            Assert.NotEqual(glyph.GetHashCode(), new Cairo.Glyph(3, 2, 1).GetHashCode());
            Assert.NotEqual(glyph.GetHashCode(), new Cairo.Glyph(2, 1, 3).GetHashCode());
            Assert.NotEqual(glyph.GetHashCode(), new Cairo.Glyph(1, 3, 2).GetHashCode());

            // ...and equal glyphs still agree, which is the part that must not break.
            Assert.Equal(glyph.GetHashCode(), new Cairo.Glyph(1, 2, 3).GetHashCode());
        }

        [Fact]
        public void A_glyphs_fractional_position_survives_hashing()
        {
            // The old hash cast X and Y to int, so every glyph between two whole
            // numbers hashed alike -- and sub-pixel positioning is exactly what a
            // glyph run uses those fields for.
            var whole = new Cairo.Glyph(1, 2.0, 3.0);
            var fractional = new Cairo.Glyph(1, 2.5, 3.0);

            Assert.True(whole != fractional);
            Assert.NotEqual(whole.GetHashCode(), fractional.GetHashCode());
        }

        // ------------------------------------------------------- the toy text API

        [Fact]
        public void Showing_text_leaves_ink_and_showing_nothing_does_not()
        {
            Run(() =>
            {
                Assert.True(LeavesInk(cr =>
                {
                    UseFont(cr);
                    cr.MoveTo(2, 30);
                    cr.ShowText("Hg");
                }), "text should paint");

                Assert.False(LeavesInk(cr =>
                {
                    UseFont(cr);
                    cr.MoveTo(2, 30);
                    cr.ShowText("");
                }), "an empty string should paint nothing");
            });
        }

        [Fact]
        public void Text_extents_grow_with_the_text()
        {
            // Absolute widths depend on the font the machine has; the ordering
            // does not, and it still proves the extents came back from Cairo.
            Run(() =>
            {
                using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 200, 50);
                using var cr = new Cairo.Context(surface);
                UseFont(cr);

                var one = cr.TextExtents("W");
                var four = cr.TextExtents("WWWW");
                var none = cr.TextExtents("");

                Assert.True(one.Width > 0, "a glyph should have width");
                Assert.True(four.Width > one.Width, $"'WWWW' ({four.Width}) should be wider than 'W' ({one.Width})");
                Assert.True(four.XAdvance > one.XAdvance, "and should advance further");

                Assert.Equal(0, none.Width);
                Assert.Equal(0, none.XAdvance);
            });
        }

        [Fact]
        public void Font_extents_describe_the_font_rather_than_any_string()
        {
            Run(() =>
            {
                using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 200, 50);
                using var cr = new Cairo.Context(surface);

                cr.SelectFontFace("sans", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
                cr.SetFontSize(10);
                var small = cr.FontExtents;

                cr.SetFontSize(40);
                var large = cr.FontExtents;

                Assert.True(small.Height > 0 && small.Ascent > 0, "a font has a height and an ascent");
                Assert.True(large.Height > small.Height,
                            $"40pt ({large.Height}) should be taller than 10pt ({small.Height})");
            });
        }

        [Fact]
        public void A_text_path_paints_only_once_something_fills_it()
        {
            // TextPath adds to the current path; it does not draw. The pair is the
            // point -- "ink appeared" would otherwise be reporting ShowText.
            Run(() =>
            {
                Assert.False(LeavesInk(cr =>
                {
                    UseFont(cr);
                    cr.MoveTo(2, 30);
                    cr.TextPath("Hg");
                }), "a path on its own paints nothing");

                Assert.True(LeavesInk(cr =>
                {
                    UseFont(cr);
                    cr.MoveTo(2, 30);
                    cr.TextPath("Hg");
                    cr.Fill();
                }), "filling the path paints it");
            });
        }

        [Fact]
        public void Where_the_current_point_is_decides_where_the_text_lands()
        {
            Run(() =>
            {
                int near = LeftmostInk(cr =>
                {
                    UseFont(cr);
                    cr.MoveTo(2, 30);
                    cr.ShowText("H");
                });

                int far = LeftmostInk(cr =>
                {
                    UseFont(cr);
                    cr.MoveTo(40, 30);
                    cr.ShowText("H");
                });

                Assert.True(near >= 0 && far >= 0, "both should paint");
                Assert.True(far > near, $"text at x=40 ({far}) should be right of text at x=2 ({near})");
            });
        }

        // ------------------------------------------------- scaled fonts and faces

        [Fact]
        public void A_context_hands_out_the_font_face_and_scaled_font_it_is_using()
        {
            Run(() =>
            {
                using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 50);
                using var cr = new Cairo.Context(surface);
                UseFont(cr);

                var face = cr.GetContextFontFace();
                var scaled = cr.GetScaledFont();

                Assert.Equal(Cairo.Status.Success, face.Status);
                Assert.Equal(Cairo.Status.Success, scaled.Status);

                // The toy API selects by family name, so the face is a toy one.
                Assert.Equal(Cairo.FontType.Toy, face.FontType);

                // A scaled font knows the metrics of the size it was scaled to.
                Assert.True(scaled.FontExtents.Height > 0);
            });
        }

        [Fact]
        public void A_scaled_font_and_its_context_agree_about_a_glyphs_extents()
        {
            // Two independent paths to the same answer -- cairo_scaled_font_glyph_
            // extents and cairo_glyph_extents -- each with its own hand-written
            // Glyph[] marshalling. Disagreement would mean one of them is copying
            // the array wrongly.
            Run(() =>
            {
                using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 50);
                using var cr = new Cairo.Context(surface);
                UseFont(cr);

                var glyphs = new[] { new Cairo.Glyph(DrawableGlyph(cr), 0, 0) };

                var fromContext = cr.GlyphExtents(glyphs);
                var fromFont = cr.GetScaledFont().GlyphExtents(glyphs);

                Assert.Equal(fromContext.Width, fromFont.Width, 3);
                Assert.Equal(fromContext.Height, fromFont.Height, 3);
                Assert.Equal(fromContext.XAdvance, fromFont.XAdvance, 3);
            });
        }

        // ------------------------------------------------------- drawing by glyph

        /// <summary>
        /// A glyph index this font actually draws something for.
        /// </summary>
        /// <remarks>
        /// Glyph indices are a property of the font file, so no particular number
        /// can be assumed on a machine whose fonts the test did not choose.
        /// Scanning for one with non-empty extents makes the test independent of
        /// which font "sans" resolves to, and fails loudly if none is found rather
        /// than quietly drawing nothing.
        /// </remarks>
        static long DrawableGlyph(Cairo.Context cr)
        {
            for (long index = 1; index < 300; index++)
            {
                var extents = cr.GlyphExtents(new[] { new Cairo.Glyph(index, 0, 0) });
                if (extents.Width > 0 && extents.Height > 0)
                    return index;
            }

            throw new InvalidOperationException("no drawable glyph found in the first 300 indices");
        }

        [Fact]
        public void Showing_a_glyph_by_index_paints_it()
        {
            Run(() =>
            {
                using var probe = new Cairo.ImageSurface(Cairo.Format.Argb32, 80, 40);
                using var probeContext = new Cairo.Context(probe);
                UseFont(probeContext);
                long index = DrawableGlyph(probeContext);

                Assert.True(LeavesInk(cr =>
                {
                    UseFont(cr);
                    cr.ShowGlyphs(new[] { new Cairo.Glyph(index, 2, 30) });
                }), "a glyph with extents should leave ink");
            });
        }

        [Fact]
        public void A_glyphs_own_position_decides_where_it_lands()
        {
            // The strongest available check on the hand-written Glyph[] copy into
            // unmanaged memory: index, x and y all have to survive it, and moving
            // only x has to move only the ink. A struct laid out wrongly would put
            // the glyph somewhere else, or draw nothing.
            Run(() =>
            {
                using var probe = new Cairo.ImageSurface(Cairo.Format.Argb32, 80, 40);
                using var probeContext = new Cairo.Context(probe);
                UseFont(probeContext);
                long index = DrawableGlyph(probeContext);

                int near = LeftmostInk(cr =>
                {
                    UseFont(cr);
                    cr.ShowGlyphs(new[] { new Cairo.Glyph(index, 2, 30) });
                });

                int far = LeftmostInk(cr =>
                {
                    UseFont(cr);
                    cr.ShowGlyphs(new[] { new Cairo.Glyph(index, 40, 30) });
                });

                Assert.True(near >= 0 && far >= 0, "both should paint");
                Assert.True(far > near, $"the glyph at x=40 ({far}) should be right of the one at x=2 ({near})");
            });
        }

        [Fact]
        public void A_run_of_several_glyphs_paints_wider_than_one()
        {
            // Proves the whole array reaches Cairo rather than only its first
            // element -- the failure mode this repository has hit repeatedly with
            // array-plus-count parameters.
            Run(() =>
            {
                using var probe = new Cairo.ImageSurface(Cairo.Format.Argb32, 80, 40);
                using var probeContext = new Cairo.Context(probe);
                UseFont(probeContext);
                long index = DrawableGlyph(probeContext);

                var one = new[] { new Cairo.Glyph(index, 2, 30) };
                var three = new[]
                {
                    new Cairo.Glyph(index, 2, 30),
                    new Cairo.Glyph(index, 22, 30),
                    new Cairo.Glyph(index, 42, 30),
                };

                Assert.True(RightmostInk(cr => { UseFont(cr); cr.ShowGlyphs(three); })
                            > RightmostInk(cr => { UseFont(cr); cr.ShowGlyphs(one); }),
                            "three glyphs should reach further right than one");
            });
        }

        [Fact]
        public void A_glyph_path_paints_only_once_something_fills_it()
        {
            Run(() =>
            {
                using var probe = new Cairo.ImageSurface(Cairo.Format.Argb32, 80, 40);
                using var probeContext = new Cairo.Context(probe);
                UseFont(probeContext);
                long index = DrawableGlyph(probeContext);

                var glyphs = new[] { new Cairo.Glyph(index, 2, 30) };

                Assert.False(LeavesInk(cr => { UseFont(cr); cr.GlyphPath(glyphs); }),
                             "a glyph path on its own paints nothing");

                Assert.True(LeavesInk(cr => { UseFont(cr); cr.GlyphPath(glyphs); cr.Fill(); }),
                            "filling it paints it");
            });
        }

        /// <summary>The x of the rightmost painted pixel, or -1.</summary>
        static int RightmostInk(Action<Cairo.Context> draw, int width = 80, int height = 40)
        {
            using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, width, height);
            using (var cr = new Cairo.Context(surface))
                draw(cr);

            surface.Flush();

            var data = surface.Data;
            int stride = surface.Stride;

            for (int x = width - 1; x >= 0; x--)
                for (int y = 0; y < height; y++)
                    if (data[y * stride + x * 4 + 3] != 0)
                        return x;

            return -1;
        }
    }
}
