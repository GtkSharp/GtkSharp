using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The composite half of the GListModel pipeline: the stages that take other
    /// stages as their input rather than a predicate or an expression.
    /// <c>GtkMultiSorter</c>, <c>GtkEveryFilter</c>/<c>GtkAnyFilter</c>,
    /// <c>GtkFlattenListModel</c>, <c>GtkSliceListModel</c>,
    /// <c>GtkMapListModel</c> and <c>GtkTreeListModel</c> had never been called
    /// from a test -- <c>ActionsAndModelsTests</c> and <c>ExpressionTests</c>
    /// cover the leaf stages (<c>SortListModel</c>, <c>FilterListModel</c>,
    /// <c>StringSorter</c>, <c>CustomFilter</c>) and stop there.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every ordering assertion here is computed by the test with LINQ over the
    /// same input array the model was built from, so the oracle is arithmetic
    /// the binding had no hand in. Where ownership is the subject the oracle is
    /// GObject's own reference count, read straight out of the instance: the
    /// <c>ref_count</c> field sits one pointer into <c>GObject</c>, which is
    /// public ABI and is what <c>AbiStructTests</c> independently asserts.
    /// </para>
    /// </remarks>
    public class ListTransformTests : GtkTestBase
    {
        public ListTransformTests(GtkFixture fixture) : base(fixture) { }

        // --------------------------------------------------------- the subject

        /// <summary>
        /// A row with two independent integer keys, so that a multi sorter has
        /// something to break ties with, plus a name that is unique across the
        /// fixture so the assertion can name identities rather than values.
        /// </summary>
        [GLib.TypeName("GtkSharpTestsTransformRow")]
        public class Row : GLib.Object
        {
            private string name = "";
            private int group;
            private int score;

            public Row() { }
            public Row(IntPtr raw) : base(raw) { }

            [GLib.Property("name")]
            public string Name
            {
                get { return name; }
                set { name = value; Notify("name"); }
            }

            [GLib.Property("group")]
            public int Group
            {
                get { return group; }
                set { group = value; Notify("group"); }
            }

            [GLib.Property("score")]
            public int Score
            {
                get { return score; }
                set { score = value; Notify("score"); }
            }
        }

        private static GLib.GType RowType
        {
            get { return (GLib.GType) typeof(Row); }
        }

        /// <summary>The fixture: no two rows share a (group, score) pair, and no
        /// two share a score, so both orderings below are total.</summary>
        private static readonly (string Name, int Group, int Score)[] Fixture =
        {
            ("delta",   2, 10),
            ("alpha",   1, 40),
            ("charlie", 2, 20),
            ("bravo",   1, 30),
        };

        private static GLib.ListStore RowStore()
        {
            var store = new GLib.ListStore(RowType);
            foreach (var (name, group, score) in Fixture)
                store.Append(new Row { Name = name, Group = group, Score = score }.Handle);
            return store;
        }

        private static NumericSorter SortOn(string property)
            => new NumericSorter(new PropertyExpression(RowType, null, property));

        private static string[] Names(GLib.IListModel model)
            => Enumerable.Range(0, (int) model.NItems)
                         .Select(i => ((Row) model.GetObject((uint) i)).Name)
                         .ToArray();

        private static string[] Strings(GLib.IListModel model)
            => Enumerable.Range(0, (int) model.NItems)
                         .Select(i => ((StringObject) model.GetObject((uint) i)).String)
                         .ToArray();

        /// <summary>
        /// GObject's reference count, read out of the instance. The layout is
        /// <c>{ GTypeInstance (one pointer); guint ref_count; gpointer qdata; }</c>,
        /// so the count is one pointer in -- the offset
        /// <see cref="AbiStructTests"/> asserts independently.
        /// </summary>
        private static int RefCount(GLib.Object obj)
            => Marshal.ReadInt32(obj.Handle, IntPtr.Size);

        // ---------------------------------------------------------- MultiSorter

        [Fact]
        public void A_multi_sorter_uses_its_later_sorters_only_to_break_the_earlier_ones_ties()
        {
            Run(() =>
            {
                var store = RowStore();

                var byGroupThenScore = new MultiSorter();
                byGroupThenScore.Append(SortOn("group"));
                byGroupThenScore.Append(SortOn("score"));

                var sorted = new SortListModel(store, byGroupThenScore);

                var expected = Fixture.OrderBy(r => r.Group).ThenBy(r => r.Score)
                                      .Select(r => r.Name).ToArray();
                Assert.Equal(expected, Names(sorted));

                // The control: the same two keys in the other order. Score alone
                // is already a total order over the fixture, so the group sorter
                // never gets a say and the result has to be different.
                var byScoreThenGroup = new MultiSorter();
                byScoreThenGroup.Append(SortOn("score"));
                byScoreThenGroup.Append(SortOn("group"));

                var other = new SortListModel(RowStore(), byScoreThenGroup);

                var otherExpected = Fixture.OrderBy(r => r.Score).ThenBy(r => r.Group)
                                           .Select(r => r.Name).ToArray();
                Assert.Equal(otherExpected, Names(other));
                Assert.NotEqual(expected, otherExpected);
            });
        }

        [Fact]
        public void Removing_the_primary_sorter_leaves_the_multi_sorter_sorting_by_what_is_left()
        {
            // GtkMultiSorter is a GListModel of its own sorters, and editing that
            // list has to re-sort whatever is downstream of it.
            Run(() =>
            {
                var group = SortOn("group");
                var score = SortOn("score");

                var multi = new MultiSorter();
                multi.Append(group);
                multi.Append(score);

                var sorted = new SortListModel(RowStore(), multi);

                Assert.Equal(2u, multi.NItems);
                Assert.Equal(group.Handle, multi.GetObject(0).Handle);
                Assert.Equal(score.Handle, multi.GetObject(1).Handle);

                multi.Remove(0);

                Assert.Equal(1u, multi.NItems);
                Assert.Equal(score.Handle, multi.GetObject(0).Handle);

                Assert.Equal(Fixture.OrderBy(r => r.Score).Select(r => r.Name).ToArray(),
                             Names(sorted));
            });
        }

        [Fact]
        public void Appending_a_sorter_gives_the_multi_sorter_a_reference_of_its_own()
        {
            // gtk_multi_sorter_append is (transfer full): it consumes a reference.
            // If the binding handed over the caller's one instead of taking a new
            // one, Remove would free a sorter the caller still holds a wrapper
            // for. The counts below are the whole point of that distinction.
            Run(() =>
            {
                var inner = SortOn("score");
                int held = RefCount(inner);

                var multi = new MultiSorter();
                multi.Append(inner);

                Assert.Equal(held + 1, RefCount(inner));

                multi.Remove(0);

                Assert.Equal(held, RefCount(inner));

                // And the caller's wrapper outlived the round trip: a sorter over
                // a freed instance could not answer this.
                var low = new Row { Name = "low", Group = 0, Score = 1 };
                var high = new Row { Name = "high", Group = 0, Score = 2 };
                Assert.Equal(Ordering.Smaller, inner.Compare(low.Handle, high.Handle));
                Assert.Equal(Ordering.Larger, inner.Compare(high.Handle, low.Handle));
            });
        }

        // ---------------------------------------------------------- MultiFilter

        private static readonly string[] Words =
            { "ant", "bee", "beetle", "cicada", "bug", "butterfly" };

        private static bool IsLong(string s) => s.Length >= 6;
        private static bool StartsWithB(string s) => s.StartsWith("b", StringComparison.Ordinal);

        private static CustomFilter FilterOn(Func<string, bool> predicate)
            => new CustomFilter(item => predicate(((StringObject) GLib.Object.GetObject(item)).String));

        [Fact]
        public void An_every_filter_keeps_the_intersection_and_an_any_filter_the_union()
        {
            Run(() =>
            {
                var every = new EveryFilter();
                every.Append(FilterOn(IsLong));
                every.Append(FilterOn(StartsWithB));

                var any = new AnyFilter();
                any.Append(FilterOn(IsLong));
                any.Append(FilterOn(StartsWithB));

                var intersection = Words.Where(w => IsLong(w) && StartsWithB(w)).ToArray();
                var union = Words.Where(w => IsLong(w) || StartsWithB(w)).ToArray();

                Assert.Equal(intersection,
                             Strings(new FilterListModel(new StringList(Words), every)));
                Assert.Equal(union,
                             Strings(new FilterListModel(new StringList(Words), any)));

                // The pair is the control for each other: over this fixture the
                // intersection is a proper subset of the union, so a combinator
                // that ignored one of its children would collapse them together.
                Assert.True(intersection.Length < union.Length);
                Assert.Subset(union.ToHashSet(), intersection.ToHashSet());
            });
        }

        [Fact]
        public void Removing_one_of_an_every_filters_children_widens_what_it_matches()
        {
            Run(() =>
            {
                var isLong = FilterOn(IsLong);
                var startsWithB = FilterOn(StartsWithB);

                var every = new EveryFilter();
                every.Append(isLong);
                every.Append(startsWithB);

                var filtered = new FilterListModel(new StringList(Words), every);

                Assert.Equal(2u, every.NItems);
                Assert.Equal(Words.Where(w => IsLong(w) && StartsWithB(w)).ToArray(),
                             Strings(filtered));

                every.Remove(1);

                Assert.Equal(1u, every.NItems);
                Assert.Equal(Words.Where(IsLong).ToArray(), Strings(filtered));

                // Asked directly rather than through a FilterListModel, the same
                // composite has to give the same answer.
                var cicada = new StringObject("cicada");
                var bee = new StringObject("bee");
                Assert.True(every.Match(cicada.Handle));
                Assert.False(every.Match(bee.Handle));
            });
        }

        // ---------------------------------------------------- FlattenListModel

        [Fact]
        public void A_flatten_list_model_concatenates_its_inner_models_in_order()
        {
            Run(() =>
            {
                var inner = new[]
                {
                    new StringList(new[] { "a1", "a2" }),
                    new StringList(new[] { "b1" }),
                    new StringList(new[] { "c1", "c2", "c3" }),
                };

                var outer = new GLib.ListStore(GLib.ListModelAdapter.GType);
                foreach (var list in inner)
                    outer.Append(list.Handle);

                var flat = new FlattenListModel(outer);

                var expected = inner.SelectMany(
                    list => Enumerable.Range(0, (int) list.NItems)
                                      .Select(i => list.GetString((uint) i))).ToArray();

                Assert.Equal(expected, Strings(flat));
                Assert.Equal((uint) expected.Length, flat.NItems);

                // Which inner model owns each flattened position, computed here
                // from the lengths rather than asked of Gtk.
                var owner = inner.SelectMany(
                    list => Enumerable.Repeat(list, (int) list.NItems)).ToArray();

                for (uint i = 0; i < expected.Length; i++)
                    Assert.Equal(owner[i].Handle,
                                 ((GLib.Object) flat.GetModelForItem(i)).Handle);
            });
        }

        [Fact]
        public void Appending_to_an_inner_model_moves_the_flattened_model_at_the_computed_offset()
        {
            Run(() =>
            {
                var first = new StringList(new[] { "a1", "a2" });
                var second = new StringList(new[] { "b1" });
                var third = new StringList(new[] { "c1" });

                var outer = new GLib.ListStore(GLib.ListModelAdapter.GType);
                outer.Append(first.Handle);
                outer.Append(second.Handle);
                outer.Append(third.Handle);

                var flat = new FlattenListModel(outer);

                var moves = new List<(uint Position, uint Removed, uint Added)>();
                flat.ItemsChanged += (o, args) =>
                    moves.Add((args.Position, args.Removed, args.Added));

                // "b2" lands at the end of the second model, which begins at the
                // combined length of everything before it.
                second.Append("b2");

                Assert.Equal(new[] { (3u, 0u, 1u) }, moves);
                Assert.Equal(new[] { "a1", "a2", "b1", "b2", "c1" }, Strings(flat));

                // The control: the same operation on the first model has to be
                // reported at a different offset, not at the same one.
                moves.Clear();
                first.Append("a3");

                Assert.Equal(new[] { (2u, 0u, 1u) }, moves);
                Assert.Equal(new[] { "a1", "a2", "a3", "b1", "b2", "c1" }, Strings(flat));
            });
        }

        // ------------------------------------------------------ SliceListModel

        [Fact]
        public void A_slice_is_the_window_its_offset_and_size_describe_and_stops_at_the_end()
        {
            Run(() =>
            {
                var source = new StringList(Words);

                var slice = new SliceListModel(source, 2, 3);
                Assert.Equal(Words.Skip(2).Take(3).ToArray(), Strings(slice));

                // A window wider than what is left is truncated, not padded.
                slice.Size = 100;
                Assert.Equal(Words.Skip(2).ToArray(), Strings(slice));

                slice.Offset = 1;
                slice.Size = 2;
                Assert.Equal(Words.Skip(1).Take(2).ToArray(), Strings(slice));

                // The control: a window that starts past the end is empty while
                // the model it slices is not.
                slice.Offset = (uint) Words.Length + 5;
                slice.Size = 3;
                Assert.Equal(0u, slice.NItems);
                Assert.Equal((uint) Words.Length, source.NItems);
            });
        }

        // -------------------------------------------------------- MapListModel

        private static string Reversed(string s)
        {
            var chars = s.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        [Fact]
        public void A_map_list_model_presents_what_its_map_function_returned()
        {
            // GtkMapListModelMapFunc is (transfer full) on both sides: it is
            // handed a reference to consume and must hand one back. The binding
            // exposes that as raw pointers, so the test does both halves
            // explicitly -- GetObject(ptr, true) takes the incoming reference,
            // OwnedHandle produces the outgoing one.
            Run(() =>
            {
                var source = new StringList(Words);

                var mapped = new MapListModel(source, item =>
                {
                    var incoming = (StringObject) GLib.Object.GetObject(item, true);
                    return new StringObject(Reversed(incoming.String)).OwnedHandle;
                });

                Assert.True(mapped.HasMap);
                Assert.Equal(Words.Select(Reversed).ToArray(), Strings(mapped));
                Assert.Equal((uint) Words.Length, mapped.NItems);
            });
        }

        [Fact]
        public void Clearing_the_map_function_turns_the_model_back_into_a_pass_through()
        {
            Run(() =>
            {
                var source = new StringList(Words);

                var mapped = new MapListModel(source, item =>
                {
                    var incoming = (StringObject) GLib.Object.GetObject(item, true);
                    return new StringObject(Reversed(incoming.String)).OwnedHandle;
                });

                Assert.Equal(Words.Select(Reversed).ToArray(), Strings(mapped));

                mapped.MapFunc = null;

                Assert.False(mapped.HasMap);
                Assert.Equal(Words, Strings(mapped));
            });
        }

        // -------------------------------------------------------- TreeListModel

        /// <summary>
        /// The tree the two structural tests below describe: "a" has two
        /// children, the first of which has one of its own; "b" is a leaf.
        /// </summary>
        private static readonly Dictionary<string, string[]> Tree =
            new Dictionary<string, string[]>
            {
                { "a", new[] { "a1", "a2" } },
                { "a1", new[] { "a1x" } },
            };

        private static TreeListModel BuildTree(bool autoexpand = false)
        {
            var root = new StringList(new[] { "a", "b" });

            return new TreeListModel(root, false, autoexpand, item =>
            {
                var value = ((StringObject) GLib.Object.GetObject(item)).String;
                return Tree.TryGetValue(value, out var children)
                    ? new StringList(children)
                    : null;
            });
        }

        /// <summary>The (value, depth) of every row currently in the tree model.</summary>
        private static (string Value, uint Depth)[] Rows(TreeListModel tree)
            => Enumerable.Range(0, (int) ((GLib.IListModel) tree).NItems)
                         .Select(i =>
                         {
                             var row = tree.GetRow((uint) i);
                             var item = (StringObject) GLib.Object.GetObject(row.Item, true);
                             return (item.String, row.Depth);
                         })
                         .ToArray();

        [Fact]
        public void A_tree_list_model_inserts_a_rows_children_directly_after_it_when_it_expands()
        {
            Run(() =>
            {
                var tree = BuildTree();

                Assert.Equal(new[] { ("a", 0u), ("b", 0u) }, Rows(tree));

                tree.GetRow(0).Expanded = true;
                Assert.Equal(new[] { ("a", 0u), ("a1", 1u), ("a2", 1u), ("b", 0u) }, Rows(tree));

                // Expanding a child pushes everything after it down again, and
                // the grandchild is a level deeper than its parent.
                tree.GetRow(1).Expanded = true;
                Assert.Equal(
                    new[] { ("a", 0u), ("a1", 1u), ("a1x", 2u), ("a2", 1u), ("b", 0u) },
                    Rows(tree));

                Assert.Equal(1u, tree.GetRow(2).Parent.Position);
                Assert.Equal(2u, tree.GetRow(2).Position);

                // Collapsing the root takes the whole subtree with it, expanded
                // grandchild included.
                tree.GetRow(0).Expanded = false;
                Assert.Equal(new[] { ("a", 0u), ("b", 0u) }, Rows(tree));
            });
        }

        [Fact]
        public void A_row_whose_create_function_returned_null_can_never_be_expanded()
        {
            // The control for the test above: returning null from the create
            // function is how Gtk 4 says "leaf", and a leaf must not grow the
            // model when something tries to expand it.
            Run(() =>
            {
                var tree = BuildTree();

                Assert.True(tree.GetRow(0).IsExpandable);
                Assert.False(tree.GetRow(1).IsExpandable);

                tree.GetRow(1).Expanded = true;

                Assert.False(tree.GetRow(1).Expanded);
                Assert.Equal(new[] { ("a", 0u), ("b", 0u) }, Rows(tree));
            });
        }

        [Fact]
        public void The_tree_takes_a_reference_of_its_own_to_the_model_its_create_function_returned()
        {
            // GtkTreeListModelCreateModelFunc's return is (transfer full), so Gtk
            // unrefs the child model when the row collapses. The binding used to
            // hand back a bare Handle: Gtk then owned a reference nobody had
            // taken, and the collapse destroyed a model the caller still held.
            Run(() =>
            {
                var child = new StringList(new[] { "leaf" });

                // A second reference, deliberately never released: it makes a
                // missing ref show up as a wrong count rather than as a freed
                // object, which is undefined behaviour rather than a test result.
                IntPtr guard = child.OwnedHandle;
                Assert.NotEqual(IntPtr.Zero, guard);

                var tree = new TreeListModel(new StringList(new[] { "root" }), false, false,
                                             item => child);

                int atRest = RefCount(child);

                // is_expandable asks the create function for a model and drops it
                // again straight away, so it has to be a round trip to nowhere.
                Assert.True(tree.GetRow(0).IsExpandable);
                Assert.Equal(atRest, RefCount(child));

                tree.GetRow(0).Expanded = true;
                Assert.Equal(atRest + 1, RefCount(child));
                Assert.Equal(2u, ((GLib.IListModel) tree).NItems);

                tree.GetRow(0).Expanded = false;
                Assert.Equal(atRest, RefCount(child));

                // And ours survived all of it.
                Assert.Equal(1u, child.NItems);
                Assert.Equal("leaf", child.GetString(0));
            });
        }
    }
}
