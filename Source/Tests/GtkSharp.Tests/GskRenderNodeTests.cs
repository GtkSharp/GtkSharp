using System;
using System.Linq;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The GskRenderNode tree: the scene graph Gtk 4 draws through.
    /// </summary>
    /// <remarks>
    /// Almost every test here ends at <see cref="Gsk.RenderNode.Draw"/> onto an
    /// image surface and reads the pixels back. That is deliberate. A render node
    /// is a description of drawing, so "did the node come out right" and "does
    /// this node describe the drawing I asked for" are the same question, and only
    /// rasterising answers it. Asserting that a constructor returned a non-null
    /// handle would pass just as happily against a node built from the wrong
    /// arguments -- and several of these bindings *were* built from the wrong
    /// arguments until this file went in.
    ///
    /// Pixels are ARGB32, which is premultiplied BGRA in memory on little-endian:
    /// byte 0 blue, 1 green, 2 red, 3 alpha.
    /// </remarks>
    public class GskRenderNodeTests : GtkTestBase
    {
        public GskRenderNodeTests(GtkFixture gtk) : base(gtk) { }

        // ------------------------------------------------------------- helpers

        static Graphene.Rect Rect(float x, float y, float w, float h)
        {
            var rect = Graphene.Rect.Alloc();
            rect.Init(x, y, w, h);
            return rect;
        }

        static Graphene.Point Point(float x, float y)
        {
            var point = new Graphene.Point();
            point.Init(x, y);
            return point;
        }

        static Gdk.RGBA Rgba(float r, float g, float b, float a = 1f)
            => new Gdk.RGBA { Red = r, Green = g, Blue = b, Alpha = a };

        static readonly Gdk.RGBA Red = Rgba(1, 0, 0);
        static readonly Gdk.RGBA Blue = Rgba(0, 0, 1);
        static readonly Gdk.RGBA Opaque = Rgba(0, 0, 0);

        static Gsk.ColorNode Color(Gdk.RGBA colour, float x, float y, float w, float h)
            => new Gsk.ColorNode(colour, Rect(x, y, w, h));

        /// <summary>One rasterised pixel, as (b, g, r, a).</summary>
        readonly struct Pixel
        {
            public Pixel(byte b, byte g, byte r, byte a) { B = b; G = g; R = r; A = a; }
            public byte B { get; }
            public byte G { get; }
            public byte R { get; }
            public byte A { get; }
            public override string ToString() => $"(b={B} g={G} r={R} a={A})";
        }

        /// <summary>Rasterises <paramref name="node"/> into a transparent
        /// <paramref name="size"/>-square surface and returns a pixel reader.</summary>
        static Func<int, int, Pixel> Render(Gsk.RenderNode node, int size = 8)
        {
            using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, size, size);
            using (var cr = new Cairo.Context(surface))
                node.Draw(cr);

            surface.Flush();

            // Copied out: the surface is disposed when this method returns, and
            // Data points into it.
            var data = (byte[])surface.Data.Clone();
            int stride = surface.Stride;

            return (x, y) =>
            {
                int i = y * stride + x * 4;
                return new Pixel(data[i], data[i + 1], data[i + 2], data[i + 3]);
            };
        }

        /// <summary>Cairo's premultiplied 8-bit channels are not exact, so
        /// comparisons allow a little slack.</summary>
        static void AssertNear(int expected, byte actual, string what, int tolerance = 2)
            => Assert.True(Math.Abs(expected - actual) <= tolerance,
                           $"{what}: expected about {expected}, got {actual}");

        // ------------------------------------------------------- the leaf nodes

        [Fact]
        public void A_colour_node_paints_its_rectangle_and_nothing_else()
        {
            Run(() =>
            {
                var pixel = Render(Color(Red, 0, 0, 4, 8));

                Assert.Equal(255, pixel(1, 1).R);
                Assert.Equal(255, pixel(1, 1).A);
                Assert.Equal(0, pixel(6, 1).A);       // outside the node's bounds
            });
        }

        [Fact]
        public void A_cairo_node_hands_out_a_context_that_draws_into_it()
        {
            Run(() =>
            {
                var node = new Gsk.CairoNode(Rect(0, 0, 8, 8));

                using (var cr = node.DrawContext)
                {
                    cr.SetSourceRGBA(0, 0, 1, 1);
                    cr.Rectangle(0, 0, 4, 8);
                    cr.Fill();
                }

                var pixel = Render(node);

                Assert.Equal(255, pixel(1, 1).B);
                Assert.Equal(0, pixel(6, 1).A);
                Assert.Equal(Gsk.RenderNodeType.CairoNode, node.NodeType);
            });
        }

        [Fact]
        public void A_texture_node_paints_the_texture_it_was_given()
        {
            Run(() =>
            {
                // A 1x1 opaque green pixbuf, scaled over the node's bounds.
                using var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, 1, 1);
                pixbuf.Fill(0x00FF00FFu);

                var texture = new Gdk.Texture(pixbuf);
                var node = new Gsk.TextureNode(texture, Rect(0, 0, 8, 8));

                var pixel = Render(node);

                Assert.Equal(255, pixel(4, 4).G);
                Assert.Equal(0, pixel(4, 4).R);
                Assert.Equal(255, pixel(4, 4).A);
            });
        }

        // ------------------------------------------------ the wrapping nodes

        [Fact]
        public void An_opacity_node_makes_its_child_translucent()
        {
            Run(() =>
            {
                var node = new Gsk.OpacityNode(Color(Red, 0, 0, 8, 8), 0.5f);

                Assert.Equal(0.5f, node.Opacity, 3);
                Assert.Equal(Gsk.RenderNodeType.ColorNode, node.Child.NodeType);

                var pixel = Render(node);

                AssertNear(128, pixel(4, 4).A, "alpha at half opacity");
                // Premultiplied, so red drops with the alpha rather than staying 255.
                AssertNear(128, pixel(4, 4).R, "premultiplied red at half opacity");
            });
        }

        [Fact]
        public void A_clip_node_stops_its_child_at_the_clip()
        {
            Run(() =>
            {
                var node = new Gsk.ClipNode(Color(Red, 0, 0, 8, 8), Rect(0, 0, 4, 8));

                Assert.Equal(4, node.Clip.Width, 3);
                Assert.Equal(4, node.Bounds.Width, 3);   // the clip shrinks the bounds

                var pixel = Render(node);

                Assert.Equal(255, pixel(1, 4).A);
                Assert.Equal(0, pixel(6, 4).A);
            });
        }

        [Fact]
        public void A_transform_node_moves_where_its_child_lands()
        {
            Run(() =>
            {
                var moved = new Gsk.Transform().Translate(Point(4, 0));
                var node = new Gsk.TransformNode(Color(Red, 0, 0, 4, 8), moved);

                var pixel = Render(node);

                Assert.Equal(0, pixel(1, 4).A);         // vacated by the translation
                Assert.Equal(255, pixel(6, 4).A);       // and filled at the far side
                Assert.Equal(4, node.Bounds.X, 3);
            });
        }

        [Fact]
        public void A_repeat_node_tiles_its_child_across_the_bounds()
        {
            Run(() =>
            {
                // A child that covers only the top-left quarter, tiled over the whole.
                var child = Color(Red, 0, 0, 4, 4);
                var node = new Gsk.RepeatNode(Rect(0, 0, 8, 8), child, Rect(0, 0, 4, 4));

                Assert.Equal(4, node.ChildBounds.Width, 3);

                var pixel = Render(node);

                Assert.Equal(255, pixel(1, 1).A);       // the original tile
                Assert.Equal(255, pixel(6, 6).A);       // and a repeat of it
            });
        }

        [Fact]
        public void A_debug_node_carries_its_message_through_to_the_managed_side()
        {
            Run(() =>
            {
                // The message is (transfer full) in the api.xml, so this also pins
                // that the wrapper hands over a string GSK may free.
                var node = new Gsk.DebugNode(Color(Red, 0, 0, 8, 8), "why this node exists");

                Assert.Equal("why this node exists", node.Message);
                Assert.Equal(Gsk.RenderNodeType.DebugNode, node.NodeType);

                // A debug node draws exactly its child.
                Assert.Equal(255, Render(node)(4, 4).A);
            });
        }

        [SkippableFact]
        public void A_copy_node_draws_the_same_thing_as_its_child()
        {
            Skip.IfNot(TestEnvironment.GtkAtLeast(4, 22),
                       TestEnvironment.NeedsGtk(4, 22, "gsk_copy_node_new"));

            Run(() =>
            {
                var node = new Gsk.CopyNode(Color(Blue, 0, 0, 4, 8));

                var pixel = Render(node);

                Assert.Equal(255, pixel(1, 4).B);
                Assert.Equal(0, pixel(6, 4).A);
                Assert.Equal(Gsk.RenderNodeType.ColorNode, node.Child.NodeType);
            });
        }

        // ------------------------------------------------- the combining nodes

        [SkippableFact]
        public void A_container_node_holds_its_children_in_order_and_draws_them_all()
        {
            Skip.IfNot(TestEnvironment.GtkAtLeast(4, 22),
                       TestEnvironment.NeedsGtk(4, 22, "gsk_render_node_get_children"));

            Run(() =>
            {
                var left = Color(Red, 0, 0, 4, 8);
                var right = Color(Blue, 4, 0, 4, 8);

                var node = new Gsk.ContainerNode(new Gsk.RenderNode[] { left, right });

                Assert.Equal(2u, node.NChildren);
                Assert.Equal(2, node.Children.Length);

                // The bounds are the union of the children's.
                Assert.Equal(8, node.Bounds.Width, 3);

                var pixel = Render(node);

                Assert.Equal(255, pixel(1, 4).R);
                Assert.Equal(255, pixel(6, 4).B);
            });
        }

        [Fact]
        public void A_container_node_refuses_a_null_child_rather_than_reffing_one()
        {
            Run(() =>
            {
                // gsk_container_node_new would ref whatever it was handed. Catching
                // this here is the difference between an exception and a segfault.
                Assert.Throws<ArgumentException>(
                    () => new Gsk.ContainerNode(new Gsk.RenderNode[] { Color(Red, 0, 0, 1, 1), null }));
            });
        }

        [Fact]
        public void A_cross_fade_node_at_full_progress_shows_only_the_end_child()
        {
            Run(() =>
            {
                var start = Color(Red, 0, 0, 8, 8);
                var end = Color(Blue, 0, 0, 8, 8);

                var node = new Gsk.CrossFadeNode(start, end, 1f);

                Assert.Equal(1f, node.Progress, 3);
                Assert.Equal(Gsk.RenderNodeType.ColorNode, node.StartChild.NodeType);
                Assert.Equal(Gsk.RenderNodeType.ColorNode, node.EndChild.NodeType);

                var pixel = Render(node);

                Assert.Equal(255, pixel(4, 4).B);
                Assert.Equal(0, pixel(4, 4).R);
            });
        }

        [Fact]
        public void A_cross_fade_node_halfway_mixes_both_children()
        {
            Run(() =>
            {
                var node = new Gsk.CrossFadeNode(
                    Color(Red, 0, 0, 8, 8), Color(Blue, 0, 0, 8, 8), 0.5f);

                var pixel = Render(node);

                AssertNear(128, pixel(4, 4).R, "red at halfway");
                AssertNear(128, pixel(4, 4).B, "blue at halfway");
                Assert.Equal(255, pixel(4, 4).A);
            });
        }

        [Fact]
        public void A_blend_node_keeps_both_children_and_the_mode()
        {
            Run(() =>
            {
                var bottom = Color(Red, 0, 0, 8, 8);
                var top = Color(Blue, 0, 0, 8, 8);

                var node = new Gsk.BlendNode(bottom, top, Gsk.BlendMode.Multiply);

                Assert.Equal(Gsk.BlendMode.Multiply, node.BlendMode);
                Assert.Equal(bottom.Handle, node.BottomChild.Handle);
                Assert.Equal(top.Handle, node.TopChild.Handle);

                // Multiplying opaque red by opaque blue leaves no channel standing.
                var pixel = Render(node);

                Assert.Equal(0, pixel(4, 4).R);
                Assert.Equal(0, pixel(4, 4).B);
                Assert.Equal(255, pixel(4, 4).A);
            });
        }

        [Fact]
        public void A_mask_node_shows_its_source_only_where_the_mask_is_opaque()
        {
            Run(() =>
            {
                var source = Color(Red, 0, 0, 8, 8);
                var mask = Color(Opaque, 0, 0, 4, 8);     // left half only

                var node = new Gsk.MaskNode(source, mask, Gsk.MaskMode.Alpha);

                Assert.Equal(Gsk.MaskMode.Alpha, node.MaskMode);
                Assert.Equal(source.Handle, node.Source.Handle);
                Assert.Equal(mask.Handle, node.Mask.Handle);

                var pixel = Render(node);

                Assert.Equal(255, pixel(1, 4).R);
                Assert.Equal(0, pixel(6, 4).A);
            });
        }

        [Fact]
        public void An_inverted_alpha_mask_shows_the_source_everywhere_else()
        {
            Run(() =>
            {
                var node = new Gsk.MaskNode(
                    Color(Red, 0, 0, 8, 8), Color(Opaque, 0, 0, 4, 8),
                    Gsk.MaskMode.InvertedAlpha);

                var pixel = Render(node);

                Assert.Equal(0, pixel(1, 4).A);
                Assert.Equal(255, pixel(6, 4).R);
            });
        }

        // ---------------------------------------------------------- gradients

        // These constructors took a single ColorStop by value plus a count the
        // caller supplied separately: the only way to build a real gradient was to
        // promise GSK more stops than had been allocated. They now take the array
        // and derive the count from it.

        static Gsk.ColorStop Stop(float offset, Gdk.RGBA colour)
            => new Gsk.ColorStop { Offset = offset, Color = colour };

        [Fact]
        public void A_linear_gradient_runs_from_the_first_stop_to_the_last()
        {
            Run(() =>
            {
                var node = new Gsk.LinearGradientNode(
                    Rect(0, 0, 8, 8), Point(0, 0), Point(8, 0),
                    new[] { Stop(0f, Red), Stop(1f, Blue) });

                var pixel = Render(node);

                // Left is red, right is blue, and the middle is neither.
                Assert.True(pixel(0, 4).R > 200, $"left should be red, was {pixel(0, 4)}");
                Assert.True(pixel(7, 4).B > 200, $"right should be blue, was {pixel(7, 4)}");
                Assert.True(pixel(4, 4).R < pixel(0, 4).R, "red should fall across the gradient");
                Assert.True(pixel(4, 4).B < pixel(7, 4).B, "blue should rise across the gradient");
            });
        }

        [Fact]
        public void A_linear_gradient_reads_back_every_stop_it_was_built_with()
        {
            Run(() =>
            {
                var stops = new[] { Stop(0f, Red), Stop(0.5f, Rgba(0, 1, 0)), Stop(1f, Blue) };

                var node = new Gsk.LinearGradientNode(
                    Rect(0, 0, 8, 8), Point(0, 0), Point(8, 0), stops);

                Assert.Equal(3ul, node.NColorStops);

                var read = node.ColorStops;

                Assert.Equal(3, read.Length);
                Assert.Equal(new[] { 0f, 0.5f, 1f }, read.Select(s => s.Offset));
                Assert.Equal(1f, read[0].Color.Red, 3);
                Assert.Equal(1f, read[1].Color.Green, 3);
                Assert.Equal(1f, read[2].Color.Blue, 3);
            });
        }

        [Fact]
        public void A_gradient_with_fewer_than_two_stops_is_refused_rather_than_asserted_on()
        {
            Run(() =>
            {
                // GSK asserts n_color_stops >= 2, and an assertion inside a
                // constructor aborts the process; this has to be caught in managed
                // code to be catchable at all.
                Assert.Throws<ArgumentException>(() => new Gsk.LinearGradientNode(
                    Rect(0, 0, 8, 8), Point(0, 0), Point(8, 0), new[] { Stop(0f, Red) }));
            });
        }

        [Fact]
        public void A_radial_gradient_is_brightest_at_its_centre()
        {
            Run(() =>
            {
                var node = new Gsk.RadialGradientNode(
                    Rect(0, 0, 8, 8), Point(4, 4), 4f, 4f, 0f, 1f,
                    new[] { Stop(0f, Red), Stop(1f, Blue) });

                Assert.Equal(4, node.Center.X, 3);
                Assert.Equal(4f, node.Hradius, 3);
                Assert.Equal(2, node.ColorStops.Length);

                var pixel = Render(node);

                Assert.True(pixel(4, 4).R > pixel(0, 0).R,
                            $"centre {pixel(4, 4)} should be redder than the corner {pixel(0, 0)}");
            });
        }

        [Fact]
        public void A_conic_gradient_keeps_its_centre_and_rotation()
        {
            Run(() =>
            {
                var node = new Gsk.ConicGradientNode(
                    Rect(0, 0, 8, 8), Point(4, 4), 90f,
                    new[] { Stop(0f, Red), Stop(1f, Blue) });

                Assert.Equal(4, node.Center.Y, 3);
                Assert.Equal(90f, node.Rotation, 3);
                Assert.Equal(2, node.ColorStops.Length);

                // A conic gradient sweeps all the way round, so it covers its bounds.
                Assert.Equal(255, Render(node)(4, 1).A);
            });
        }

        [Fact]
        public void A_repeating_linear_gradient_covers_its_bounds()
        {
            Run(() =>
            {
                // The stop range spans a quarter of the node, so the rest is only
                // painted if the pattern really does repeat.
                var node = new Gsk.RepeatingLinearGradientNode(
                    Rect(0, 0, 8, 8), Point(0, 0), Point(2, 0),
                    new[] { Stop(0f, Red), Stop(1f, Blue) });

                var pixel = Render(node);

                Assert.Equal(255, pixel(7, 4).A);
                Assert.Equal(Gsk.RenderNodeType.RepeatingLinearGradientNode, node.NodeType);
            });
        }

        [Fact]
        public void A_repeating_radial_gradient_covers_its_bounds()
        {
            Run(() =>
            {
                var node = new Gsk.RepeatingRadialGradientNode(
                    Rect(0, 0, 8, 8), Point(4, 4), 2f, 2f, 0f, 1f,
                    new[] { Stop(0f, Red), Stop(1f, Blue) });

                Assert.Equal(255, Render(node)(0, 0).A);
                Assert.Equal(Gsk.RenderNodeType.RepeatingRadialGradientNode, node.NodeType);
            });
        }

        // ------------------------------------------------------------ shadows

        [Fact]
        public void A_shadow_node_draws_a_shadow_beside_its_child()
        {
            Run(() =>
            {
                // An unblurred black shadow offset to the right: the child occupies
                // the left half, so anything painted on the right is the shadow.
                var shadow = new Gsk.Shadow
                {
                    Color = Rgba(0, 0, 0),
                    Dx = 4,
                    Dy = 0,
                    Radius = 0
                };

                var node = new Gsk.ShadowNode(Color(Red, 0, 0, 4, 8), new[] { shadow });

                Assert.Equal(1ul, node.NShadows);
                Assert.Equal(4f, node.GetShadow(0).Dx, 3);

                var pixel = Render(node);

                Assert.Equal(255, pixel(1, 4).R);    // the child itself
                Assert.Equal(255, pixel(6, 4).A);    // its shadow
                Assert.Equal(0, pixel(6, 4).R);      // which is black, not red
            });
        }

        [Fact]
        public void A_shadow_node_keeps_every_shadow_it_was_given()
        {
            Run(() =>
            {
                // The constructor used to take one Shadow by value and a count, so
                // a second shadow could be promised but never delivered.
                var shadows = new[]
                {
                    new Gsk.Shadow { Color = Rgba(0, 0, 0), Dx = 2, Dy = 0, Radius = 0 },
                    new Gsk.Shadow { Color = Rgba(0, 1, 0), Dx = -2, Dy = 0, Radius = 1 },
                };

                var node = new Gsk.ShadowNode(Color(Red, 2, 0, 4, 8), shadows);

                Assert.Equal(2ul, node.NShadows);
                Assert.Equal(new[] { 2f, -2f }, node.Shadows.Select(s => s.Dx));
                Assert.Equal(1f, node.Shadows[1].Color.Green, 3);
                Assert.Equal(1f, node.Shadows[1].Radius, 3);
            });
        }

        [Fact]
        public void A_shadow_node_refuses_an_empty_shadow_list()
        {
            Run(() => Assert.Throws<ArgumentException>(
                () => new Gsk.ShadowNode(Color(Red, 0, 0, 4, 4), new Gsk.Shadow[0])));
        }

        // ------------------------------------------------------- paths and fills

        [Fact]
        public void A_fill_node_paints_the_inside_of_its_path()
        {
            Run(() =>
            {
                var builder = new Gsk.PathBuilder();
                builder.AddRect(Rect(0, 0, 4, 8));
                var path = builder.ToPath();

                var node = new Gsk.FillNode(Color(Red, 0, 0, 8, 8), path, Gsk.FillRule.Winding);

                Assert.Equal(Gsk.FillRule.Winding, node.FillRule);

                var pixel = Render(node);

                Assert.Equal(255, pixel(1, 4).R);    // inside the path
                Assert.Equal(0, pixel(6, 4).A);      // outside it
            });
        }

        [Fact]
        public void A_stroke_node_paints_along_its_path_rather_than_filling_it()
        {
            Run(() =>
            {
                var builder = new Gsk.PathBuilder();
                builder.MoveTo(0, 4);
                builder.LineTo(8, 4);
                var path = builder.ToPath();

                var stroke = new Gsk.Stroke(2f);
                var node = new Gsk.StrokeNode(Color(Red, 0, 0, 8, 8), path, stroke);

                Assert.Equal(2f, node.Stroke.LineWidth, 3);

                var pixel = Render(node);

                Assert.Equal(255, pixel(4, 4).A);    // on the line
                Assert.Equal(0, pixel(4, 0).A);      // well clear of it
            });
        }

        [Fact]
        public void A_stroke_keeps_the_dash_pattern_it_is_given()
        {
            Run(() =>
            {
                // SetDash used to be "float SetDash (ulong n_dash)" -- the array
                // parameter had become an out-parameter, so there was no way to
                // pass a pattern and the obvious call handed GSK a pointer to four
                // bytes of stack.
                var stroke = new Gsk.Stroke(1f);

                Assert.Empty(stroke.Dash);

                stroke.Dash = new[] { 3f, 1f, 2f };

                Assert.Equal(new[] { 3f, 1f, 2f }, stroke.Dash);

                stroke.Dash = null;

                Assert.Empty(stroke.Dash);
            });
        }

        [Fact]
        public void A_dashed_stroke_leaves_gaps_a_solid_one_does_not()
        {
            Run(() =>
            {
                var builder = new Gsk.PathBuilder();
                builder.MoveTo(0, 4);
                builder.LineTo(8, 4);
                var path = builder.ToPath();

                var dashed = new Gsk.Stroke(2f) { Dash = new[] { 2f, 2f } };
                var node = new Gsk.StrokeNode(Color(Red, 0, 0, 8, 8), path, dashed);

                var pixel = Render(node);

                Assert.Equal(255, pixel(1, 4).A);    // in the first dash
                Assert.Equal(0, pixel(3, 4).A);      // in the gap after it
            });
        }

        // ------------------------------------------------- rounded rectangles

        // GskRoundedRect is twelve floats -- graphene_rect_t bounds followed by
        // graphene_size_t corner[4]. It used to be a pointer plus a managed array,
        // so nothing built on one could be used at all; these four node types are
        // what that cost.

        static Gsk.RoundedRect Rounded(float x, float y, float w, float h, float radius)
        {
            var rounded = new Gsk.RoundedRect();
            rounded.InitFromRect(Rect(x, y, w, h), radius);
            return rounded;
        }

        [Fact]
        public void A_rounded_rect_rounds_the_corners_off_a_clip()
        {
            Run(() =>
            {
                var node = new Gsk.RoundedClipNode(Color(Red, 0, 0, 8, 8), Rounded(0, 0, 8, 8, 4));

                Assert.Equal(4f, node.Clip.TopLeftWidth, 3);
                Assert.Equal(8, node.Clip.Width, 3);

                var pixel = Render(node);

                // The middle survives the clip; the corner is cut away by it.
                Assert.Equal(255, pixel(4, 4).A);
                Assert.Equal(0, pixel(0, 0).A);
            });
        }

        [Fact]
        public void A_square_rounded_clip_keeps_the_corners_a_rounded_one_removes()
        {
            Run(() =>
            {
                // The contrast is what makes the test above mean anything: with a
                // zero radius the same node keeps its corner.
                var node = new Gsk.RoundedClipNode(Color(Red, 0, 0, 8, 8), Rounded(0, 0, 8, 8, 0));

                Assert.Equal(255, Render(node)(0, 0).A);
                Assert.True(node.Clip.Equals(Rounded(0, 0, 8, 8, 0)));
            });
        }

        [Fact]
        public void A_border_node_draws_only_the_edges_it_was_given_a_width_for()
        {
            Run(() =>
            {
                // Left edge only: widths are top, right, bottom, left.
                var widths = new[] { 0f, 0f, 0f, 2f };
                var colours = new[] { Red, Red, Red, Red };

                var node = new Gsk.BorderNode(Rounded(0, 0, 8, 8, 0), widths, colours);

                Assert.Equal(widths, node.Widths);
                Assert.Equal(4, node.Colors.Length);
                Assert.Equal(1f, node.Colors[3].Red, 3);
                Assert.Equal(8, node.Outline.Width, 3);

                var pixel = Render(node);

                Assert.Equal(255, pixel(0, 4).R);    // on the left border
                Assert.Equal(0, pixel(7, 4).A);      // the right edge has no width
            });
        }

        [Fact]
        public void A_border_node_keeps_a_different_colour_per_edge()
        {
            Run(() =>
            {
                // get_colors used to return the array's first element as the whole
                // answer, so three of these four were unreachable.
                var colours = new[] { Red, Blue, Rgba(0, 1, 0), Rgba(1, 1, 0) };

                var node = new Gsk.BorderNode(
                    Rounded(0, 0, 8, 8, 0), new[] { 1f, 1f, 1f, 1f }, colours);

                var read = node.Colors;

                Assert.Equal(1f, read[Gsk.BorderNode.TopEdge].Red, 3);
                Assert.Equal(1f, read[Gsk.BorderNode.RightEdge].Blue, 3);
                Assert.Equal(1f, read[Gsk.BorderNode.BottomEdge].Green, 3);
                Assert.Equal(0f, read[Gsk.BorderNode.LeftEdge].Blue, 3);
            });
        }

        [Fact]
        public void An_outset_shadow_paints_outside_its_outline_and_an_inset_one_inside()
        {
            Run(() =>
            {
                var outline = Rounded(2, 2, 4, 4, 0);

                var outset = new Gsk.OutsetShadowNode(outline, Opaque, 0, 0, 1f, 0f);
                var inset = new Gsk.InsetShadowNode(outline, Opaque, 0, 0, 1f, 0f);

                Assert.Equal(1f, outset.Spread, 3);
                Assert.Equal(2, outset.Outline.X, 3);

                // An outset shadow is drawn around the outline, never within it.
                var out_pixel = Render(outset);
                Assert.Equal(255, out_pixel(1, 4).A);
                Assert.Equal(0, out_pixel(4, 4).A);

                // An inset one is the mirror image: inside the outline only.
                var in_pixel = Render(inset);
                Assert.Equal(0, in_pixel(1, 4).A);
                Assert.Equal(255, in_pixel(2, 4).A);
            });
        }

        [Fact]
        public void A_rounded_rect_answers_what_it_contains()
        {
            Run(() =>
            {
                var rect = Rounded(0, 0, 40, 20, 5);

                Assert.True(rect.ContainsPoint(Point(20, 10)), "the middle is inside");
                Assert.False(rect.ContainsPoint(Point(0, 0)), "the rounded corner is not");
                Assert.True(rect.ContainsRect(Rect(10, 5, 20, 10)));
                Assert.True(rect.IntersectsRect(Rect(-5, -5, 20, 20)));
                Assert.False(rect.IntersectsRect(Rect(100, 100, 5, 5)));
            });
        }

        [Fact]
        public void A_rounded_rect_is_rectilinear_only_when_it_has_no_corners()
        {
            Run(() =>
            {
                Assert.True(Rounded(0, 0, 40, 20, 0).IsRectilinear);
                Assert.False(Rounded(0, 0, 40, 20, 5).IsRectilinear);
            });
        }

        [Fact]
        public void Shrinking_a_rounded_rect_insets_each_edge_separately()
        {
            Run(() =>
            {
                var rect = Rounded(0, 0, 40, 20, 5);

                rect.Shrink(2, 4, 6, 8);       // top, right, bottom, left

                Assert.Equal(8, rect.X, 3);
                Assert.Equal(2, rect.Y, 3);
                Assert.Equal(40 - 8 - 4, rect.Width, 3);
                Assert.Equal(20 - 2 - 6, rect.Height, 3);
            });
        }

        [Fact]
        public void Offsetting_a_rounded_rect_moves_it_and_leaves_the_corners_alone()
        {
            Run(() =>
            {
                var rect = Rounded(0, 0, 40, 20, 5);

                rect.Offset(3, 7);

                Assert.Equal(3, rect.X, 3);
                Assert.Equal(7, rect.Y, 3);
                Assert.Equal(40, rect.Width, 3);
                Assert.Equal(5, rect.TopLeftWidth, 3);
            });
        }

        [Fact]
        public void Two_rounded_rects_are_equal_only_when_every_float_matches()
        {
            Run(() =>
            {
                // Generated with noequals/nohash, because a field-less struct would
                // get an Equals that compares nothing and answers true for
                // everything -- which is what a copy-only test would have missed.
                var rect = Rounded(0, 0, 40, 20, 5);

                Assert.Equal(rect, Rounded(0, 0, 40, 20, 5));
                Assert.NotEqual(rect, Rounded(0, 0, 40, 20, 6));
                Assert.NotEqual(rect, Rounded(1, 0, 40, 20, 5));
                Assert.Equal(rect.GetHashCode(), Rounded(0, 0, 40, 20, 5).GetHashCode());
                Assert.NotEqual(rect.GetHashCode(), Rounded(0, 0, 20, 40, 5).GetHashCode());
            });
        }

        // -------------------------------------------------- serialisation

        [Fact]
        public void A_node_survives_a_round_trip_through_its_serialised_form()
        {
            Run(() =>
            {
                var original = new Gsk.ContainerNode(new Gsk.RenderNode[]
                {
                    Color(Red, 0, 0, 4, 8),
                    Color(Blue, 4, 0, 4, 8),
                });

                var bytes = original.Serialize();
                Assert.True(bytes.Size > 0, "a serialised node should not be empty");

                var restored = Gsk.RenderNode.Deserialize(bytes, null);

                Assert.NotNull(restored);
                Assert.Equal(Gsk.RenderNodeType.ContainerNode, restored.NodeType);
                Assert.Equal(8, restored.Bounds.Width, 3);

                // The strongest check available: it draws the same thing.
                var before = Render(original);
                var after = Render(restored);

                Assert.Equal(before(1, 4).R, after(1, 4).R);
                Assert.Equal(before(6, 4).B, after(6, 4).B);
            });
        }

        // ---------------------------------------------------------- the tree

        [SkippableFact]
        public void Children_are_reported_for_a_node_that_has_them_and_not_for_one_that_does_not()
        {
            Skip.IfNot(TestEnvironment.GtkAtLeast(4, 22),
                       TestEnvironment.NeedsGtk(4, 22, "gsk_render_node_get_children"));

            Run(() =>
            {
                var leaf = Color(Red, 0, 0, 4, 8);
                var container = new Gsk.ContainerNode(new Gsk.RenderNode[] { leaf, Color(Blue, 4, 0, 4, 8) });

                Assert.Empty(leaf.Children);
                Assert.Equal(2, container.Children.Length);

                // gsk_render_node_get_children returns a GskRenderNode**; reading it
                // as a single node would give the array's address a node's identity.
                Assert.Equal(leaf.Handle, container.Children[0].Handle);
                Assert.Equal(container.GetChild(1).Handle, container.Children[1].Handle);
            });
        }

        [Fact]
        public void An_opaque_rect_is_reported_for_an_opaque_node_and_refused_for_a_translucent_one()
        {
            Run(() =>
            {
                var opaque = Color(Red, 0, 0, 8, 8);
                var translucent = Color(Rgba(1, 0, 0, 0.5f), 0, 0, 8, 8);

                Assert.True(opaque.GetOpaqueRect(out var covered), "an opaque colour node covers its bounds");
                Assert.Equal(8, covered.Width, 3);

                Assert.False(translucent.GetOpaqueRect(out _), "a half-transparent node covers nothing");
            });
        }

        [Theory]
        [InlineData(Gsk.RenderNodeType.ColorNode)]
        [InlineData(Gsk.RenderNodeType.ContainerNode)]
        [InlineData(Gsk.RenderNodeType.OpacityNode)]
        [InlineData(Gsk.RenderNodeType.ClipNode)]
        [InlineData(Gsk.RenderNodeType.TransformNode)]
        [InlineData(Gsk.RenderNodeType.DebugNode)]
        public void Each_node_reports_the_type_that_matches_its_class(Gsk.RenderNodeType expected)
        {
            Run(() =>
            {
                var child = Color(Red, 0, 0, 4, 4);

                Gsk.RenderNode node = expected switch
                {
                    Gsk.RenderNodeType.ColorNode => child,
                    Gsk.RenderNodeType.ContainerNode => new Gsk.ContainerNode(new[] { child }),
                    Gsk.RenderNodeType.OpacityNode => new Gsk.OpacityNode(child, 0.5f),
                    Gsk.RenderNodeType.ClipNode => new Gsk.ClipNode(child, Rect(0, 0, 2, 2)),
                    Gsk.RenderNodeType.TransformNode => new Gsk.TransformNode(child, new Gsk.Transform()),
                    Gsk.RenderNodeType.DebugNode => new Gsk.DebugNode(child, "d"),
                    _ => throw new ArgumentOutOfRangeException(nameof(expected)),
                };

                Assert.Equal(expected, node.NodeType);
            });
        }
    }
}
