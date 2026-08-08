using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The three satellite assemblies whose hand-written layer is small and whose
    /// generated surface is almost untouched: libadwaita, GtkSourceView and Gsk.
    /// </summary>
    /// <remarks>
    /// All three are from-scratch bindings added during the Gtk 4 migration, so
    /// none of them has any history of having worked. The oracles here are kept
    /// outside the library wherever one exists: what sorting and upper-casing do
    /// to strings the test chose, how many values an enum has, the arithmetic of
    /// a translation and its inverse, and the bytes the test itself read back off
    /// disk.
    /// </remarks>
    public class SatelliteAssemblyTests : GtkTestBase
    {
        public SatelliteAssemblyTests(GtkFixture fixture) : base(fixture)
        {
            Run(() =>
            {
                // Adw must be initialised before any of its widgets are used, and
                // adw_init is a no-op the second time.
                if (!Adw.Global.IsInitialized)
                    Adw.Global.Init();
            });
        }

        // ------------------------------------------------------------- helpers

        private static Graphene.Point PointAt(float x, float y)
        {
            var point = new Graphene.Point();
            point.Init(x, y);
            return point;
        }

        private static Graphene.Rect RectAt(float x, float y, float width, float height)
        {
            var rect = Graphene.Rect.Alloc();
            rect.Init(x, y, width, height);
            return rect;
        }

        private static Gdk.RGBA Opaque(float red, float green, float blue)
        {
            return new Gdk.RGBA { Red = red, Green = green, Blue = blue, Alpha = 1f };
        }

        /// <summary>
        /// Runs the main loop until <paramref name="condition"/> holds, so that a
        /// test can wait on work GtkSourceView schedules on an idle rather than
        /// guessing at a sleep. Returns what the condition said at the end.
        /// </summary>
        private static bool PumpUntil(Func<bool> condition, int timeoutMs = 3000)
        {
            var clock = Stopwatch.StartNew();
            while (!condition() && clock.ElapsedMilliseconds < timeoutMs)
            {
                if (Gtk.Application.EventsPending())
                    Gtk.Application.RunIteration(false);
                else
                    System.Threading.Thread.Sleep(1);
            }

            return condition();
        }

        private static string TextOf(Gtk.TextBuffer buffer)
        {
            return buffer.GetText(buffer.StartIter, buffer.EndIter, true);
        }

        // ================================================================= Gsk

        [Fact]
        public void A_container_node_hands_back_the_children_it_was_built_from()
        {
            // Two array-shaped calls that codegen had no rule for, because the
            // length is a separate parameter rather than a NULL terminator:
            // gsk_container_node_new (GskRenderNode **, guint) came out taking a
            // single node, and gsk_render_node_get_children wrapped the *array*
            // address in one RenderNode. Both handed a GskRenderNodeClass pointer
            // back as though it were a node. Rebound over arrays; the ordering is
            // the oracle, since GSK draws children in the order it was given them.
            Run(() =>
            {
                var children = new Gsk.RenderNode[]
                {
                    new Gsk.ColorNode(Opaque(1, 0, 0), RectAt(0, 0, 10, 10)),
                    new Gsk.ColorNode(Opaque(0, 1, 0), RectAt(20, 0, 10, 10)),
                    new Gsk.ColorNode(Opaque(0, 0, 1), RectAt(40, 0, 10, 10)),
                };

                using var container = new Gsk.ContainerNode(children);

                Assert.Equal(Gsk.RenderNodeType.ContainerNode, container.NodeType);
                Assert.Equal(3u, container.NChildren);

                var read = container.Children;
                Assert.Equal(3, read.Length);
                Assert.Equal(new float[] { 0, 20, 40 },
                             new[] { read[0].Bounds.X, read[1].Bounds.X, read[2].Bounds.X });

                // A leaf has no children, and that has to be an empty array
                // rather than a one-element array holding rubbish.
                Assert.Empty(read[0].Children);
            });
        }

        [Fact]
        public void Node_bounds_compose_the_way_the_operations_describe()
        {
            // Every node computes its bounds from its children's, so this is
            // arithmetic the test can do itself: a container unions, a clip
            // intersects, a transform moves.
            Run(() =>
            {
                var left = new Gsk.ColorNode(Opaque(1, 0, 0), RectAt(0, 0, 10, 10));
                var right = new Gsk.ColorNode(Opaque(0, 1, 0), RectAt(20, 20, 10, 10));

                using var container = new Gsk.ContainerNode(new Gsk.RenderNode[] { left, right });
                Assert.Equal(0f, container.Bounds.X, 3);
                Assert.Equal(30f, container.Bounds.Width, 3);
                Assert.Equal(30f, container.Bounds.Height, 3);

                using var clipped = new Gsk.ClipNode(container, RectAt(5, 5, 100, 100));
                Assert.Equal(5f, clipped.Bounds.X, 3);
                Assert.Equal(25f, clipped.Bounds.Width, 3);

                using var moved = new Gsk.TransformNode(container, new Gsk.Transform().Translate(PointAt(100, 0)));
                Assert.Equal(100f, moved.Bounds.X, 3);
                Assert.Equal(30f, moved.Bounds.Width, 3);
            });
        }

        [Fact]
        public void A_node_tree_survives_serialisation_and_deserialisation()
        {
            // gsk_render_node_serialize/deserialize exist for testing and
            // debugging, and the format is textual, so the round trip is the one
            // thing GSK guarantees about it: the same version of GTK reads back
            // what it wrote.
            Run(() =>
            {
                var children = new Gsk.RenderNode[]
                {
                    new Gsk.ColorNode(Opaque(1, 0, 0), RectAt(0, 0, 10, 20)),
                    new Gsk.ColorNode(Opaque(0, 0, 1), RectAt(30, 0, 10, 20)),
                };
                using var original = new Gsk.ContainerNode(children);

                using var bytes = original.Serialize();
                var failures = new List<string>();

                using var restored = Gsk.RenderNode.Deserialize(
                    bytes, (start, end, error) => failures.Add("parse error"));

                Assert.Empty(failures);
                Assert.NotNull(restored);
                Assert.Equal(Gsk.RenderNodeType.ContainerNode, restored.NodeType);
                Assert.Equal(original.Bounds.X, restored.Bounds.X, 3);
                Assert.Equal(original.Bounds.Width, restored.Bounds.Width, 3);

                var restoredChildren = restored.Children;
                Assert.Equal(2, restoredChildren.Length);
                Assert.Equal(30f, restoredChildren[1].Bounds.X, 3);
            });
        }

        [Fact]
        public void A_node_written_to_a_file_is_the_bytes_serialize_produced()
        {
            // The oracle is outside GSK entirely: what the test read back off
            // disk, compared against what Serialize handed it in memory.
            Run(() =>
            {
                using var node = new Gsk.ColorNode(Opaque(0, 1, 0), RectAt(1, 2, 3, 4));
                var path = Path.Combine(Path.GetTempPath(), "gtksharp-node-" + Guid.NewGuid().ToString("N") + ".node");

                try
                {
                    Assert.True(node.WriteToFile(path));

                    var onDisk = File.ReadAllBytes(path);
                    using var inMemory = node.Serialize();
                    Assert.Equal(inMemory.Data, onDisk);

                    using var fromDisk = new GLib.Bytes(onDisk);
                    using var restored = Gsk.RenderNode.Deserialize(fromDisk, (s, e, err) => { });

                    Assert.Equal(Gsk.RenderNodeType.ColorNode, restored.NodeType);
                    Assert.Equal(3f, restored.Bounds.Width, 3);
                }
                finally
                {
                    File.Delete(path);
                }
            });
        }

        [Fact]
        public void Deserialising_something_that_is_not_a_node_still_hands_back_a_node()
        {
            // The failure path is the only reason ParseErrorFunc is bound at all,
            // and a null delegate there would be reached by nothing else. What is
            // surprising is the return value: gsk_render_node_deserialize reports
            // the errors and then hands back an *empty container node* anyway --
            // which is exactly what an empty document produces too. So the
            // callback is the only thing that separates "this was not a render
            // node" from "this was nothing", and a caller who tests the result for
            // null concludes that arbitrary rubbish parsed fine.
            Run(() =>
            {
                using var rubbish = new GLib.Bytes(System.Text.Encoding.UTF8.GetBytes("this is not a render node"));

                var starts = new List<ulong>();
                using var node = Gsk.RenderNode.Deserialize(
                    rubbish, (start, end, error) => starts.Add(start.Bytes));

                Assert.NotEmpty(starts);
                Assert.NotNull(node);
                Assert.Equal(Gsk.RenderNodeType.ContainerNode, node.NodeType);
                Assert.Empty(node.Children);

                // The locations name a byte inside the input the test supplied,
                // which is the only claim ParseLocation makes that does not
                // depend on gsk's grammar. Reading them at all needs the struct
                // fields to have getters.
                Assert.All(starts, offset => Assert.True(offset < 25, "offset " + offset + " is past the input"));

                using var nothing = new GLib.Bytes(new byte[0]);
                var errors = 0;
                using var fromNothing = Gsk.RenderNode.Deserialize(nothing, (start, end, error) => errors++);

                Assert.Equal(0, errors);
                Assert.Equal(Gsk.RenderNodeType.ContainerNode, fromNothing.NodeType);
            });
        }

        [Fact]
        public void A_transform_builder_does_not_consume_the_transform_it_was_called_on()
        {
            // All twelve GskTransform builders take their receiver as
            // (transfer full): the transform that comes back links the old one
            // into its chain and keeps that reference. An api.xml <method>
            // describes its parameters' ownership and never the instance's, so
            // codegen passed Handle and the wrapper went on owning a reference
            // the callee had already eaten. Both then unref, and the second free
            // is deferred onto the main loop by the generated finalizer -- the
            // shape that makes a crash appear to move between runs.
            //
            // Two oracles: the receiver still describes the same transform after
            // calls that ate a reference, and nothing complains once every
            // wrapper has been finalized and its deferred unref has run.
            Run(() =>
            {
                var complaints = new List<string>();
                GLib.LogFunc record = (domain, level, message) => complaints.Add(domain + ": " + message);
                var glib = GLib.Log.SetLogHandler("GLib", GLib.LogLevelFlags.Critical | GLib.LogLevelFlags.Warning, record);
                var gsk = GLib.Log.SetLogHandler("Gsk", GLib.LogLevelFlags.Critical | GLib.LogLevelFlags.Warning, record);

                try
                {
                    for (int i = 0; i < 50; i++)
                    {
                        using var translate = new Gsk.Transform().Translate(PointAt(5, 7));
                        using var scaled = translate.Scale(2, 2);
                        using var inverted = translate.Invert();

                        float dx, dy;
                        translate.ToTranslate(out dx, out dy);
                        Assert.Equal(5.0, dx, 3);
                        Assert.Equal(7.0, dy, 3);

                        // translate(5,7) then scale(2,2) maps x to 2x + 5.
                        float sx, sy, adx, ady;
                        scaled.ToAffine(out sx, out sy, out adx, out ady);
                        Assert.Equal(2.0, sx, 3);
                        Assert.Equal(5.0, adx, 3);
                        Assert.Equal(7.0, ady, 3);

                        float ix, iy;
                        inverted.ToTranslate(out ix, out iy);
                        Assert.Equal(-5.0, ix, 3);
                        Assert.Equal(-7.0, iy, 3);
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    // The finalizer queues the unref onto the main loop with a
                    // 50 ms timeout, so a double free only surfaces once the
                    // loop has run.
                    var deadline = Stopwatch.StartNew();
                    while (deadline.ElapsedMilliseconds < 400)
                        if (Gtk.Application.EventsPending())
                            Gtk.Application.RunIteration(false);
                }
                finally
                {
                    GLib.Log.RemoveLogHandler("GLib", glib);
                    GLib.Log.RemoveLogHandler("Gsk", gsk);
                }

                Assert.Empty(complaints);
            });
        }

        [Fact]
        public void A_transform_maps_points_and_rectangles_by_its_own_arithmetic()
        {
            // GSK composes right-to-left: the last operation added is applied to
            // the point first. Getting that backwards is silent -- everything
            // still has the right category and the right number of operations.
            Run(() =>
            {
                using var transform = new Gsk.Transform().Translate(PointAt(100, 0)).Scale(2, 3);

                var mapped = transform.Point(PointAt(1, 1));
                Assert.Equal(102.0, mapped.X, 3);
                Assert.Equal(3.0, mapped.Y, 3);

                var bounds = transform.Bounds(RectAt(0, 0, 10, 10));
                Assert.Equal(100.0, bounds.X, 3);
                Assert.Equal(20.0, bounds.Width, 3);
                Assert.Equal(30.0, bounds.Height, 3);
            });
        }

        [Fact]
        public void The_identity_transform_is_a_null_pointer_and_so_is_a_failure()
        {
            // The obvious expectation -- transform.With(transform.Invert()) is an
            // identity transform -- is wrong, and wrong in the direction that
            // crashes. GSK represents the identity as a NULL GskTransform*, and
            // every function in the family accepts NULL for it, so a composition
            // that cancels out returns nothing at all and the binding has no
            // choice but to hand back null.
            //
            // gsk_transform_invert overloads that same NULL for "not invertible",
            // so from managed code a successful inverse and a failed one are
            // indistinguishable. Both are pinned here, because a null coming back
            // from Invert() reads like an error and is usually a success.
            Run(() =>
            {
                using var translate = new Gsk.Transform().Translate(PointAt(5, 7));
                using var inverse = translate.Invert();
                Assert.NotNull(inverse);

                // The maths still holds: there and back is where it started.
                var there = translate.Point(PointAt(3, 3));
                var back = inverse.Point(there);
                Assert.Equal(3.0, back.X, 3);
                Assert.Equal(3.0, back.Y, 3);

                Assert.Null(translate.With(inverse));

                // A singular transform has no inverse, and says so the same way.
                using var singular = new Gsk.Transform().Scale(0, 0);
                Assert.Equal(Gsk.TransformCategory.TwoDAffine, singular.Category);
                Assert.Null(singular.Invert());

                // gsk_transform_new does allocate an object, and it compares equal
                // to the NULL that means the same thing -- so Equal(null), not a
                // null check, is how a caller asks "is this the identity".
                using var allocated = new Gsk.Transform();
                Assert.Equal(Gsk.TransformCategory.Identity, allocated.Category);
                Assert.True(allocated.Equal(null));
                Assert.False(translate.Equal(null));
                Assert.Equal("none", allocated.ToString());

                // Which makes parsing the identity's own printed form the one case
                // where Parse succeeds and yields null.
                Gsk.Transform parsed;
                Assert.True(Gsk.Transform.Parse("none", out parsed));
                Assert.Null(parsed);
            });
        }

        [Fact]
        public void Transform_categories_narrow_as_the_operations_do()
        {
            // The enum runs from Unknown up to Identity, so "more specific" is a
            // larger value -- and the classification, not the ordering, is what
            // callers switch on.
            Run(() =>
            {
                using var identity = new Gsk.Transform();
                Assert.Equal(Gsk.TransformCategory.Identity, identity.Category);

                using var translated = identity.Translate(PointAt(1, 2));
                Assert.Equal(Gsk.TransformCategory.TwoDTranslate, translated.Category);

                using var scaled = translated.Scale(2, 2);
                Assert.Equal(Gsk.TransformCategory.TwoDAffine, scaled.Category);

                using var rotated = scaled.Rotate(30);
                Assert.Equal(Gsk.TransformCategory.TwoD, rotated.Category);

                var axis = new Graphene.Vec3();
                axis.Init(1, 1, 0);
                using var spun = rotated.Rotate3d(30, axis);
                Assert.Equal(Gsk.TransformCategory.ThreeD, spun.Category);
            });
        }

        [Fact]
        public void A_transform_parses_back_from_the_form_it_prints()
        {
            // gsk_transform_parse is documented as the inverse of
            // gsk_transform_to_string, which makes this the one assertion that
            // does not depend on the exact spelling GSK chose.
            Run(() =>
            {
                using var original = new Gsk.Transform().Translate(PointAt(5, 7)).Scale(2, 2);
                var printed = original.ToString();

                Gsk.Transform parsed;
                Assert.True(Gsk.Transform.Parse(printed, out parsed), "gsk could not parse back its own output: " + printed);

                using (parsed)
                {
                    Assert.True(original.Equal(parsed), printed + " did not compare equal to itself");
                    Assert.Equal(printed, parsed.ToString());
                }

                Gsk.Transform nonsense;
                Assert.False(Gsk.Transform.Parse("wobble(3)", out nonsense));
            });
        }

        [Fact]
        public void A_path_builder_survives_being_drained_into_a_path()
        {
            // gsk_path_builder_free_to_path frees the builder outright, the same
            // way gdk_content_formats_builder_free_to_formats does, and codegen
            // cannot see that. The wrapper here keeps the builder alive, so the
            // second use below is the assertion; the path's own bounds are the
            // arithmetic oracle.
            Run(() =>
            {
                using var builder = new Gsk.PathBuilder();
                builder.AddRect(RectAt(10, 20, 30, 40));

                using var first = builder.FreeToPath();

                Graphene.Rect bounds;
                Assert.True(first.GetBounds(out bounds));
                Assert.Equal(10.0, bounds.X, 3);
                Assert.Equal(30.0, bounds.Width, 3);

                // Draining a builder clears its operations but must not destroy
                // it: the same wrapper is still a usable builder.
                builder.AddRect(RectAt(0, 0, 5, 5));
                using var second = builder.FreeToPath();

                Graphene.Rect secondBounds;
                Assert.True(second.GetBounds(out secondBounds));
                Assert.Equal(5.0, secondBounds.Width, 3);
            });
        }

        [Fact]
        public void A_path_parses_back_from_the_svg_it_prints()
        {
            Run(() =>
            {
                using var builder = new Gsk.PathBuilder();
                builder.MoveTo(0, 0);
                builder.LineTo(10, 0);
                builder.LineTo(10, 10);
                builder.Close();

                using var original = builder.ToPath();
                Assert.True(original.IsClosed);

                using var parsed = Gsk.Path.Parse(original.ToString());
                Assert.True(original.Equal(parsed), original.ToString() + " did not round-trip");

                Graphene.Rect bounds;
                Assert.True(parsed.GetBounds(out bounds));
                Assert.Equal(10.0, bounds.Width, 3);
            });
        }

        [Fact]
        public void The_rounded_rect_struct_lays_its_twelve_floats_out_the_way_gsk_reads_them()
        {
            // This test used to assert the opposite, and said so: "written to fail
            // the day the layout is corrected". That day came. What was there:
            //
            //     struct _GskRoundedRect {
            //         graphene_rect_t bounds;      /*  16 bytes, by value  */
            //         graphene_size_t corner[4];   /*  4 x 8 = 32 bytes    */
            //     };                               /*  48 bytes total      */
            //
            // Both member types are bound as *classes* (boxed opaques), so codegen
            // emitted `bounds` as one IntPtr and `corner` as a ByValArray of four
            // object references -- 40 bytes on Windows, and on Linux not
            // marshallable at all. Every generated method did
            //
            //     AllocHGlobal (Marshal.SizeOf<Gsk.RoundedRect> ())
            //
            // and handed that to a GSK function that reads and writes 48 bytes
            // through it: an eight-byte heap overrun on every call, with `bounds`
            // read as an address built out of two floats.
            //
            // The fields are now removed in GskSharp.metadata and declared in
            // Source/Libs/GskSharp/RoundedRect.cs, which is the only file that
            // declares any -- so sequential layout is that file's declaration
            // order and nothing else. The offsets below are arithmetic over the C
            // declaration and hold independently of this binding.
            Run(() =>
            {
                var type = typeof(Gsk.RoundedRect);

                Assert.Equal(48, Marshal.SizeOf<Gsk.RoundedRect>());

                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                            BindingFlags.NonPublic);

                Assert.Equal(12, fields.Length);
                Assert.All(fields, f => Assert.Equal(typeof(float), f.FieldType));

                // graphene_rect_t bounds -- origin then size.
                Assert.Equal(0, (int) Marshal.OffsetOf<Gsk.RoundedRect>("X"));
                Assert.Equal(4, (int) Marshal.OffsetOf<Gsk.RoundedRect>("Y"));
                Assert.Equal(8, (int) Marshal.OffsetOf<Gsk.RoundedRect>("Width"));
                Assert.Equal(12, (int) Marshal.OffsetOf<Gsk.RoundedRect>("Height"));

                // graphene_size_t corner[4] -- clockwise from the top left.
                Assert.Equal(16, (int) Marshal.OffsetOf<Gsk.RoundedRect>("TopLeftWidth"));
                Assert.Equal(24, (int) Marshal.OffsetOf<Gsk.RoundedRect>("TopRightWidth"));
                Assert.Equal(32, (int) Marshal.OffsetOf<Gsk.RoundedRect>("BottomRightWidth"));
                Assert.Equal(40, (int) Marshal.OffsetOf<Gsk.RoundedRect>("BottomLeftWidth"));
                Assert.Equal(44, (int) Marshal.OffsetOf<Gsk.RoundedRect>("BottomLeftHeight"));
            });
        }

        [Fact]
        public void A_rounded_rect_reads_back_what_gsk_wrote_into_it()
        {
            // The layout test above says the bytes are in the right places; this
            // one says GSK agrees, which is the part reflection cannot show.
            Run(() =>
            {
                var bounds = Graphene.Rect.Alloc();
                bounds.Init(1, 2, 40, 20);

                var rect = new Gsk.RoundedRect();
                rect.InitFromRect(bounds, 5);

                Assert.Equal(1, rect.X, 3);
                Assert.Equal(2, rect.Y, 3);
                Assert.Equal(40, rect.Width, 3);
                Assert.Equal(20, rect.Height, 3);

                // init_from_rect gives every corner the same radius.
                Assert.Equal(5, rect.TopLeftWidth, 3);
                Assert.Equal(5, rect.TopLeftHeight, 3);
                Assert.Equal(5, rect.BottomRightWidth, 3);
                Assert.Equal(5, rect.BottomLeftHeight, 3);

                // And the returned value is the receiver, not a copy read out of
                // memory the wrapper had already freed.
                Assert.Equal(rect, rect.InitFromRect(bounds, 5));
            });
        }

        // ========================================================== GtkSource

        [Fact]
        public void The_language_manager_guesses_a_language_from_a_file_name()
        {
            // Guessing is what an editor actually does on open; GetLanguage by id
            // is what a test does. Content type is deliberately not exercised
            // here: GLib content types are mime types on Linux and file
            // extensions on Windows, so an assertion on one would be a platform
            // assertion wearing a binding's clothes.
            Run(() =>
            {
                var manager = GtkSource.LanguageManager.Default;

                Assert.Equal("c-sharp", manager.GuessLanguage("Program.cs", null).Id);
                Assert.Equal("python3", manager.GuessLanguage("setup.py", null).Id);

                // A negative result has to be null rather than some fallback, or
                // an editor silently highlights plain text as whatever came first.
                Assert.Null(manager.GuessLanguage("notes.qqzz", null));
            });
        }

        [Fact]
        public void A_language_lists_the_globs_and_style_ids_it_claims()
        {
            // Three string arrays off the same object, which is where a lost
            // level of indirection in the strv marshalling would show.
            Run(() =>
            {
                var language = GtkSource.LanguageManager.Default.GetLanguage("c-sharp");

                Assert.Contains("*.cs", language.Globs);
                Assert.Contains("text/x-csharp", language.MimeTypes);
                Assert.Equal("C#", language.Name);
                Assert.Equal("Source", language.Section);
                Assert.False(language.Hidden);

                // A language's style ids are namespaced with its own id -- there
                // is no "def:comment" in this list, and asking for one gets
                // nothing. The shared def: styles that a theme actually colours
                // are reached through the *fallback*, which is the indirection
                // that lets one theme cover every language.
                Assert.All(language.StyleIds, id => Assert.StartsWith("c-sharp:", id));
                Assert.Contains("c-sharp:comment", language.StyleIds);
                Assert.Equal("Comment", language.GetStyleName("c-sharp:comment"));
                Assert.Equal("def:comment", language.GetStyleFallback("c-sharp:comment"));
                Assert.Equal("def:special-char", language.GetStyleFallback("c-sharp:escaped-character"));
            });
        }

        [Fact]
        public void Sorting_lines_orders_them_and_leaves_a_trailing_newline_behind()
        {
            // The ordering oracle is ordinal string comparison, not GtkSourceView:
            // case-sensitively 'B' (0x42) sorts before 'a' (0x61), which is the
            // pair that tells a case-sensitive sort from a case-folding one.
            //
            // The surprise is the newline. Sorting rebuilds the range out of the
            // lines it collected joined by "\n" and then terminates it, so a
            // buffer whose last line had no newline grows one -- and a selection
            // sort in an editor silently adds a blank line at the end of the
            // document. It is idempotent, because the new empty last line is not
            // one of the lines the next sort collects.
            Run(() =>
            {
                var buffer = new GtkSource.Buffer((Gtk.TextTagTable) null);
                buffer.Text = "pear\napple\nBanana";

                buffer.SortLines(buffer.StartIter, buffer.EndIter, GtkSource.SortFlags.CaseSensitive, 0);
                Assert.Equal("Banana\napple\npear\n", TextOf(buffer));

                buffer.SortLines(buffer.StartIter, buffer.EndIter, GtkSource.SortFlags.CaseSensitive, 0);
                Assert.Equal("Banana\napple\npear\n", TextOf(buffer));

                buffer.SortLines(buffer.StartIter, buffer.EndIter,
                                 GtkSource.SortFlags.CaseSensitive | GtkSource.SortFlags.ReverseOrder, 0);
                Assert.Equal("pear\napple\nBanana\n", TextOf(buffer));

                // Without CaseSensitive the comparison folds case, so 'apple'
                // comes first and 'Banana' second -- the opposite of the pair
                // above, from the same three lines.
                buffer.SortLines(buffer.StartIter, buffer.EndIter, GtkSource.SortFlags.None, 0);
                Assert.Equal("apple\nBanana\npear\n", TextOf(buffer));

                buffer.Text = "b\na\nb\na";
                buffer.SortLines(buffer.StartIter, buffer.EndIter, GtkSource.SortFlags.RemoveDuplicates, 0);
                Assert.Equal("a\nb\n", TextOf(buffer));
            });
        }

        [Fact]
        public void Changing_case_and_joining_lines_do_what_the_names_say()
        {
            Run(() =>
            {
                var buffer = new GtkSource.Buffer((Gtk.TextTagTable) null);
                buffer.Text = "hello wide world";

                buffer.ChangeCase(GtkSource.ChangeCaseType.Upper, buffer.StartIter, buffer.EndIter);
                Assert.Equal("HELLO WIDE WORLD", TextOf(buffer));

                buffer.ChangeCase(GtkSource.ChangeCaseType.Title, buffer.StartIter, buffer.EndIter);
                Assert.Equal("Hello Wide World", TextOf(buffer));

                buffer.ChangeCase(GtkSource.ChangeCaseType.Toggle, buffer.StartIter, buffer.EndIter);
                Assert.Equal("hELLO wIDE wORLD", TextOf(buffer));

                buffer.Text = "one\ntwo\nthree";
                buffer.JoinLines(buffer.StartIter, buffer.EndIter);
                Assert.Equal("one two three", TextOf(buffer));
            });
        }

        [Fact]
        public void A_search_context_counts_every_occurrence_and_honours_case()
        {
            // Occurrence counting is scheduled on an idle and reports -1 while it
            // is still scanning, so "how many" is only an answer once the loop has
            // run -- a test that reads it straight after setting the text sees the
            // sentinel and would pass against any implementation at all.
            Run(() =>
            {
                var buffer = new GtkSource.Buffer((Gtk.TextTagTable) null);
                buffer.Text = "Foo foo FOO bar";

                var settings = new GtkSource.SearchSettings { SearchText = "foo", CaseSensitive = false };
                var context = new GtkSource.SearchContext(buffer, settings);

                Assert.True(PumpUntil(() => context.OccurrencesCount == 3),
                            $"case-insensitive search found {context.OccurrencesCount} of 3");

                settings.CaseSensitive = true;
                Assert.True(PumpUntil(() => context.OccurrencesCount == 1),
                            $"case-sensitive search found {context.OccurrencesCount} of 1");

                settings.SearchText = "nothing here";
                Assert.True(PumpUntil(() => context.OccurrencesCount == 0),
                            $"a search for absent text found {context.OccurrencesCount}");
            });
        }

        [Fact]
        public void Searching_forward_lands_on_the_offsets_the_test_put_the_matches_at()
        {
            Run(() =>
            {
                var buffer = new GtkSource.Buffer((Gtk.TextTagTable) null);
                buffer.Text = "foo bar foo baz foo";

                var settings = new GtkSource.SearchSettings { SearchText = "foo", CaseSensitive = true };
                var context = new GtkSource.SearchContext(buffer, settings);
                PumpUntil(() => context.OccurrencesCount == 3);

                Gtk.TextIter start, end;
                bool wrapped;

                Assert.True(context.Forward(buffer.StartIter, out start, out end, out wrapped));
                Assert.Equal(0, start.Offset);
                Assert.Equal(3, end.Offset);
                Assert.False(wrapped);
                Assert.Equal(1, context.GetOccurrencePosition(start, end));

                Assert.True(context.Forward(end, out start, out end, out wrapped));
                Assert.Equal(8, start.Offset);
                Assert.Equal(2, context.GetOccurrencePosition(start, end));

                // Searching backwards from the end finds the last one, and the
                // position is counted from the front either way.
                Assert.True(context.Backward(buffer.EndIter, out start, out end, out wrapped));
                Assert.Equal(16, start.Offset);
                Assert.Equal(3, context.GetOccurrencePosition(start, end));
            });
        }

        [Fact]
        public void A_regex_search_replaces_every_match_and_says_how_many()
        {
            // ReplaceAll's return value is the count, so the buffer's text and
            // that number have to agree -- and the regex flag is the thing that
            // decides whether "f.o" is a pattern or three literal characters.
            Run(() =>
            {
                var buffer = new GtkSource.Buffer((Gtk.TextTagTable) null);
                buffer.Text = "foo fao f.o fxo";

                var settings = new GtkSource.SearchSettings { SearchText = "f.o", RegexEnabled = false, CaseSensitive = true };
                var context = new GtkSource.SearchContext(buffer, settings);

                Assert.True(PumpUntil(() => context.OccurrencesCount == 1),
                            $"a literal f.o should match once, found {context.OccurrencesCount}");

                settings.RegexEnabled = true;
                Assert.True(PumpUntil(() => context.OccurrencesCount == 4),
                            $"the pattern f.o should match four times, found {context.OccurrencesCount}");

                Assert.Equal(4u, context.ReplaceAll("XX"));
                Assert.Equal("XX XX XX XX", TextOf(buffer));
            });
        }

        [Fact]
        public void Highlighting_marks_a_comment_as_a_comment()
        {
            // Context classes are the only view GtkSourceView offers onto the
            // regions its syntax engine produced, and they are what spell-check
            // and auto-indent are driven from. EnsureHighlight is what forces the
            // engine to run without a view being mapped.
            Run(() =>
            {
                var language = GtkSource.LanguageManager.Default.GetLanguage("c");
                var buffer = new GtkSource.Buffer((Gtk.TextTagTable) null)
                {
                    Language = language,
                    HighlightSyntax = true,
                };

                //          0123456789...
                buffer.Text = "int x; /* note */";
                buffer.EnsureHighlight(buffer.StartIter, buffer.EndIter);

                var insideComment = buffer.GetIterAtOffset(11);
                var insideCode = buffer.GetIterAtOffset(1);

                Assert.True(buffer.IterHasContextClass(insideComment, "comment"),
                            "offset 11 is inside /* note */");
                Assert.False(buffer.IterHasContextClass(insideCode, "comment"),
                             "offset 1 is inside the declaration");
                Assert.Contains("comment", buffer.GetContextClassesAtIter(insideComment));

                // The toggle walk is how an editor finds the extent of a region.
                var walk = buffer.StartIter;
                Assert.True(buffer.IterForwardToContextClassToggle(ref walk, "comment"));
                Assert.Equal(7, walk.Offset);
            });
        }

        [Fact]
        public void A_region_keeps_what_was_added_and_drops_what_was_subtracted()
        {
            // GtkSource.Region is set arithmetic over buffer offsets, so the
            // oracle is the arithmetic itself.
            Run(() =>
            {
                var buffer = new GtkSource.Buffer((Gtk.TextTagTable) null);
                buffer.Text = new string('x', 40);

                var region = new GtkSource.Region(buffer);
                Assert.True(region.IsEmpty);

                region.AddSubregion(buffer.GetIterAtOffset(0), buffer.GetIterAtOffset(10));
                region.AddSubregion(buffer.GetIterAtOffset(20), buffer.GetIterAtOffset(30));

                Assert.False(region.IsEmpty);
                Assert.Equal(new[] { (0, 10), (20, 30) }, Subregions(region));

                Gtk.TextIter bStart, bEnd;
                Assert.True(region.GetBounds(out bStart, out bEnd));
                Assert.Equal(0, bStart.Offset);
                Assert.Equal(30, bEnd.Offset);

                // Cutting a hole through the middle of both leaves the two ends.
                region.SubtractSubregion(buffer.GetIterAtOffset(5), buffer.GetIterAtOffset(25));
                Assert.Equal(new[] { (0, 5), (25, 30) }, Subregions(region));
            });
        }

        private static List<(int, int)> Subregions(GtkSource.Region region)
        {
            var found = new List<(int, int)>();
            var iter = region.StartRegionIter;

            while (!iter.IsEnd)
            {
                Gtk.TextIter start, end;
                if (iter.GetSubregion(out start, out end))
                    found.Add((start.Offset, end.Offset));

                if (!iter.Next())
                    break;
            }

            return found;
        }

        [Fact]
        public void A_snippet_context_expands_one_reference_and_is_not_a_template_engine()
        {
            // "Expand" reads like string interpolation. It is not: the input has
            // to be exactly one reference, and anything with a second character
            // around it comes back verbatim. "hello $name" and "$name $name" are
            // both returned unchanged, with no error -- so a caller who treats it
            // as a template engine gets literal dollar signs in the buffer. (The
            // ${...} form belongs to the snippet *parser*, which splits a snippet
            // into chunks before any of them reaches a context.)
            Run(() =>
            {
                var context = new GtkSource.SnippetContext();
                context.SetVariable("name", "world wide");
                context.SetConstant("greeting", "Hello");

                Assert.Equal("world wide", context.GetVariable("name"));
                Assert.Equal("world wide", context.Expand("$name"));
                Assert.Equal("Hello", context.Expand("$greeting"));

                Assert.Equal("hello $name", context.Expand("hello $name"));
                Assert.Equal("$name $name", context.Expand("$name $name"));
                Assert.Equal("${name}", context.Expand("${name}"));

                // An unknown name resolves to itself rather than to the empty
                // string, and a backslash escapes the sigil.
                Assert.Equal("$nope", context.Expand("$nope"));
                Assert.Equal("$name", context.Expand("\\$name"));

                // The post-processing filters are the part that really is a small
                // language, and they are what a .snippets file is written against.
                Assert.Equal("World wide", context.Expand("$name|capitalize"));
                Assert.Equal("WORLD WIDE", context.Expand("$name|upper"));
                Assert.Equal("WorldWide", context.Expand("$name|camelize"));
                Assert.Equal("world_wide", context.Expand("$name|functify"));

                context.ClearVariables();
                Assert.Null(context.GetVariable("name"));

                // A constant survives ClearVariables; that is what makes it a
                // constant rather than a variable set early.
                Assert.Equal("Hello", context.GetVariable("greeting"));
            });
        }

        [Fact]
        public void A_snippet_context_knows_the_date_without_being_told_it()
        {
            // The oracle is the calendar rather than GtkSourceView. These are the
            // constants a "file header" snippet resolves, and they are zero-padded
            // strings rather than numbers -- a caller comparing CURRENT_MONTH
            // against "8" never matches in any month before October.
            Run(() =>
            {
                var context = new GtkSource.SnippetContext();
                var now = DateTime.Now;

                Assert.Equal(now.Year.ToString(), context.GetVariable("CURRENT_YEAR"));
                Assert.Equal((now.Year % 100).ToString("00"), context.GetVariable("CURRENT_YEAR_SHORT"));
                Assert.Equal(now.Month.ToString("00"), context.GetVariable("CURRENT_MONTH"));

                // Nothing has told this context about a file, so the filename is
                // the empty string -- present, and empty. A name it has never
                // heard of is null instead, which is the difference between "no
                // value yet" and "no such variable".
                Assert.Equal("", context.GetVariable("TM_FILENAME"));
                Assert.Null(context.GetVariable("NOT_A_SNIPPET_VARIABLE"));
            });
        }

        [Fact]
        public void A_snippet_keeps_the_chunks_it_was_given_in_order()
        {
            Run(() =>
            {
                var snippet = new GtkSource.Snippet("forloop", "c-sharp") { Name = "for loop" };

                snippet.AddChunk(new GtkSource.SnippetChunk { Spec = "for (int i = 0; i < " });
                snippet.AddChunk(new GtkSource.SnippetChunk { Spec = "count", FocusPosition = 1 });
                snippet.AddChunk(new GtkSource.SnippetChunk { Spec = "; i++)" });

                Assert.Equal(3u, snippet.NChunks);
                Assert.Equal("count", snippet.GetNthChunk(1).Spec);
                Assert.Equal(1, snippet.GetNthChunk(1).FocusPosition);
                Assert.Equal("for loop", snippet.Name);

                // A copy is a copy of the chunks too, not a second handle onto
                // the same ones.
                var copy = snippet.Copy();
                Assert.Equal(3u, copy.NChunks);
                Assert.Equal("count", copy.GetNthChunk(1).Spec);
                Assert.Equal(1, copy.GetNthChunk(1).FocusPosition);
                Assert.NotEqual(snippet.GetNthChunk(1).Handle, copy.GetNthChunk(1).Handle);

                // gtk_source_snippet_copy carries the trigger, the language and
                // the description across -- and drops the name, which is the one
                // field a snippet chooser puts on screen. Taking a snippet out of
                // the manager and copying it for insertion therefore loses its
                // label, with nothing to say so.
                Assert.Equal("forloop", copy.Trigger);
                Assert.Equal("c-sharp", copy.LanguageId);
                Assert.Null(copy.Name);
            });
        }

        // ============================================================ Adwaita

        [Fact]
        public void A_navigation_view_pops_back_to_the_page_it_was_pushed_from()
        {
            Run(() =>
            {
                var view = new Adw.NavigationView();
                var home = new Adw.NavigationPage(new Gtk.Label("home"), "Home", "home");
                var detail = new Adw.NavigationPage(new Gtk.Label("detail"), "Detail", "detail");

                view.Push(home);
                Assert.Equal("home", view.VisiblePage.Tag);
                Assert.Equal(1u, view.NavigationStack.NItems);

                view.Push(detail);
                Assert.Equal("detail", view.VisiblePage.Tag);
                Assert.Equal(2u, view.NavigationStack.NItems);
                Assert.Equal("home", view.GetPreviousPage(detail).Tag);

                Assert.True(view.Pop());
                Assert.Equal("home", view.VisiblePage.Tag);

                // Popping the root page has nothing to go back to, and says so
                // rather than emptying the view.
                Assert.False(view.Pop());
                Assert.Equal("home", view.VisiblePage.Tag);
                Assert.Equal(1u, view.NavigationStack.NItems);
            });
        }

        [Fact]
        public void Replacing_the_navigation_stack_leaves_the_last_page_visible()
        {
            // adw_navigation_view_replace takes AdwNavigationPage ** plus a count
            // and replace_with_tags a char ** plus a count, and codegen has no
            // rule for an array whose length is a separate parameter -- so both
            // came out taking a single value, and libadwaita read the first
            // machine word of it as element zero. Rebound over arrays.
            Run(() =>
            {
                var view = new Adw.NavigationView();
                var first = new Adw.NavigationPage(new Gtk.Label("one"), "One", "one");
                var second = new Adw.NavigationPage(new Gtk.Label("two"), "Two", "two");
                var third = new Adw.NavigationPage(new Gtk.Label("three"), "Three", "three");

                view.Replace(new[] { first, second, third });

                Assert.Equal(3u, view.NavigationStack.NItems);
                Assert.Equal("three", view.VisiblePage.Tag);
                Assert.Equal("two", view.GetPreviousPage(third).Tag);

                Assert.True(view.PopToTag("one"));
                Assert.Equal("one", view.VisiblePage.Tag);
                Assert.Equal(1u, view.NavigationStack.NItems);

                // Replacing with an empty stack is legal and leaves nothing
                // visible; a wrongly bound array cannot express that at all.
                view.Replace(new Adw.NavigationPage[0]);
                Assert.Equal(0u, view.NavigationStack.NItems);
                Assert.Null(view.VisiblePage);
            });
        }

        [Fact]
        public void Replacing_by_tag_finds_the_pages_that_were_added_to_the_view()
        {
            // Add() puts a page in the view's pool without showing it, which is
            // the only thing that makes the tag form usable.
            Run(() =>
            {
                var view = new Adw.NavigationView();
                view.Add(new Adw.NavigationPage(new Gtk.Label("a"), "A", "a"));
                view.Add(new Adw.NavigationPage(new Gtk.Label("b"), "B", "b"));
                view.Add(new Adw.NavigationPage(new Gtk.Label("c"), "C", "c"));

                Assert.Equal("A", view.FindPage("a").Title);
                Assert.Null(view.FindPage("nope"));

                view.ReplaceWithTags(new[] { "a", "b" });

                Assert.Equal(2u, view.NavigationStack.NItems);
                Assert.Equal("b", view.VisiblePage.Tag);

                view.PushByTag("c");
                Assert.Equal("c", view.VisiblePage.Tag);
                Assert.Equal(3u, view.NavigationStack.NItems);
                Assert.Equal("b", view.GetPreviousPage(view.VisiblePage).Tag);

                // A page may appear on the navigation stack only once. Pushing one
                // that is already on it does not move it to the top -- libadwaita
                // logs a critical and does nothing -- so a "go to section" button
                // wired straight to PushByTag works once and is then inert, which
                // is invisible to any caller that does not read the log.
                var complaints = new List<string>();
                GLib.LogFunc record = (domain, level, message) => complaints.Add(message);
                var handler = GLib.Log.SetLogHandler("Adwaita", GLib.LogLevelFlags.Critical, record);
                try
                {
                    view.PushByTag("a");
                }
                finally
                {
                    GLib.Log.RemoveLogHandler("Adwaita", handler);
                }

                Assert.Contains(complaints, m => m.Contains("'a'"));
                Assert.Equal("c", view.VisiblePage.Tag);
                Assert.Equal(3u, view.NavigationStack.NItems);
            });
        }

        [Fact]
        public void A_view_stack_page_carries_the_title_its_child_was_added_with()
        {
            Run(() =>
            {
                var stack = new Adw.ViewStack();
                var inbox = new Gtk.Label("inbox");
                var sent = new Gtk.Label("sent");

                var inboxPage = stack.AddTitledWithIcon(inbox, "inbox", "Inbox", "mail-inbox-symbolic");
                stack.AddTitled(sent, "sent", "Sent");

                Assert.Equal("Inbox", inboxPage.Title);
                Assert.Equal("inbox", inboxPage.Name);
                Assert.Equal("mail-inbox-symbolic", inboxPage.IconName);
                Assert.Equal(inbox.Handle, inboxPage.Child.Handle);

                Assert.Equal(2u, ((GLib.IListModel) stack.Pages).NItems);
                Assert.Equal(sent.Handle, stack.GetChildByName("sent").Handle);

                // The visible child and its name are two views of one property,
                // so setting either has to move the other.
                stack.VisibleChild = sent;
                Assert.Equal("sent", stack.VisibleChildName);
                stack.VisibleChildName = "inbox";
                Assert.Equal(inbox.Handle, stack.VisibleChild.Handle);

                stack.Remove(sent);
                Assert.Equal(1u, ((GLib.IListModel) stack.Pages).NItems);
                Assert.Null(stack.GetChildByName("sent"));
            });
        }

        [Fact]
        public void Dismissing_a_toast_tells_whoever_was_listening()
        {
            // A toast is one-shot: the overlay owns it after AddToast and the
            // only way to observe the end of its life is ::dismissed.
            Run(() =>
            {
                var overlay = new Adw.ToastOverlay { Child = new Gtk.Label("content") };

                var dismissed = 0;
                var toast = new Adw.Toast("Saved") { Timeout = 0, Priority = Adw.ToastPriority.High };
                toast.Dismissed += (o, a) => dismissed++;

                overlay.AddToast(toast);
                Assert.Equal(0, dismissed);

                toast.Dismiss();
                Assert.Equal(1, dismissed);

                // Dismissing twice must not fire twice: the toast is already gone.
                toast.Dismiss();
                Assert.Equal(1, dismissed);
            });
        }

        [Fact]
        public void The_style_manager_reports_dark_when_the_scheme_forces_it()
        {
            // ColorScheme is what an application sets and Dark is what it reads
            // back to pick its own colours, so the two have to agree. The default
            // is restored afterwards because StyleManager.Default is shared with
            // every other test in the run.
            Run(() =>
            {
                var manager = Adw.StyleManager.Default;
                var before = manager.ColorScheme;

                try
                {
                    manager.ColorScheme = Adw.ColorScheme.ForceDark;
                    Assert.Equal(Adw.ColorScheme.ForceDark, manager.ColorScheme);
                    Assert.True(manager.Dark, "ForceDark should make Dark true");

                    manager.ColorScheme = Adw.ColorScheme.ForceLight;
                    Assert.False(manager.Dark, "ForceLight should make Dark false");
                }
                finally
                {
                    manager.ColorScheme = before;
                }
            });
        }

        [Fact]
        public void An_enum_list_model_holds_one_item_per_value_of_the_enum()
        {
            // The oracle is the enum declaration itself: AdwColorScheme has five
            // values, numbered 0 to 4, and their nicks are fixed by libadwaita.
            Run(() =>
            {
                var model = new Adw.EnumListModel(Adw.ColorSchemeGType.GType);

                Assert.Equal(5u, model.NItems);
                Assert.Equal(4u, model.FindPosition((int) Adw.ColorScheme.ForceDark));

                var item = (Adw.EnumListItem) model.GetObject(1);
                Assert.Equal((int) Adw.ColorScheme.ForceLight, item.Value);
                Assert.Equal("force-light", item.Nick);
                Assert.Equal("ADW_COLOR_SCHEME_FORCE_LIGHT", item.Name);
            });
        }

        [Fact]
        public void Spring_params_derive_damping_from_the_ratio_mass_and_stiffness()
        {
            // damping = damping_ratio * 2 * sqrt (mass * stiffness) is physics,
            // not libadwaita, so the two constructors must agree about it.
            Run(() =>
            {
                using var byRatio = new Adw.SpringParams(0.5, 2, 8);

                Assert.Equal(0.5, byRatio.DampingRatio, 6);
                Assert.Equal(2.0, byRatio.Mass, 6);
                Assert.Equal(8.0, byRatio.Stiffness, 6);
                Assert.Equal(0.5 * 2 * Math.Sqrt(2 * 8), byRatio.Damping, 6);

                using var byDamping = Adw.SpringParams.NewFull(4, 2, 8);
                Assert.Equal(4.0, byDamping.Damping, 6);
                Assert.Equal(byRatio.DampingRatio, byDamping.DampingRatio, 6);
            });
        }

        [Fact]
        public void A_breakpoint_condition_parses_back_from_the_form_it_prints()
        {
            // Breakpoint conditions arrive from .ui files as strings, so parse and
            // print are the pair that has to be consistent.
            Run(() =>
            {
                using var built = new Adw.BreakpointCondition(
                    Adw.BreakpointConditionLengthType.MaxWidth, 400, Adw.LengthUnit.Px);

                var printed = built.ToString();
                Assert.Equal("max-width: 400px", printed);

                using var parsed = Adw.BreakpointCondition.Parse(printed);
                Assert.Equal(printed, parsed.ToString());

                using var either = Adw.BreakpointCondition.NewOr(
                    built, new Adw.BreakpointCondition(Adw.BreakpointConditionLengthType.MinHeight, 200, Adw.LengthUnit.Px));
                Assert.Equal("max-width: 400px or min-height: 200px", either.ToString());
            });
        }

        [Fact]
        public void A_banner_and_a_status_page_compose_a_child_and_keep_their_text()
        {
            // Both wrap their content in internal widgetry, so "the child came
            // back" is a statement about Adw's own plumbing rather than about a
            // property setter.
            Run(() =>
            {
                var button = new Gtk.Button { Label = "Retry" };
                var page = new Adw.StatusPage
                {
                    Title = "Nothing here",
                    Description = "<b>Add</b> something",
                    IconName = "folder-symbolic",
                    Child = button,
                };

                Assert.Equal(button.Handle, page.Child.Handle);
                Assert.Equal("<b>Add</b> something", page.Description);

                var banner = new Adw.Banner("Update available")
                {
                    ButtonLabel = "Install",
                    UseMarkup = false,
                };

                // Revealed defaults to false: a banner that showed itself the
                // moment it was constructed would be a layout change nobody asked
                // for.
                Assert.False(banner.Revealed);

                var clicks = 0;
                banner.ButtonClicked += (o, a) => clicks++;
                GLib.Signal.Emit(banner, "button-clicked");
                Assert.Equal(1, clicks);
            });
        }
    }
}
