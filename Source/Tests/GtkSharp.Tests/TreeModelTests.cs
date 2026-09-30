using System;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The tree-model stack — <c>ListStore</c>, <c>TreeStore</c>,
    /// <c>TreeModelFilter</c>, <c>TreeModelSort</c> — is around a thousand
    /// hand-written lines, and the samples reach almost none of it. Gtk 4
    /// deprecated these in favour of <c>GLib.ListStore</c> with a
    /// <c>SignalListItemFactory</c>, but they are still shipped, still bound and
    /// still what a ported application arrives with, so they are worth holding to
    /// the same standard as the rest.
    /// </summary>
    public class TreeModelTests : GtkTestBase
    {
        public TreeModelTests(GtkFixture fixture) : base(fixture) { }

        private static ListStore NamesAndAges(params (string name, int age)[] rows)
        {
            var store = new ListStore(typeof(string), typeof(int));
            foreach (var (name, age) in rows)
                store.AppendValues(name, age);
            return store;
        }

        // --------------------------------------------------------- Gtk.ListStore

        [Fact]
        public void A_list_store_returns_the_values_that_were_appended()
        {
            Run(() =>
            {
                var store = NamesAndAges(("Ada", 36), ("Grace", 45));

                Assert.True(store.GetIterFirst(out var iter));
                Assert.Equal("Ada", store.GetValue(iter, 0));
                Assert.Equal(36, store.GetValue(iter, 1));

                Assert.True(store.IterNext(ref iter));
                Assert.Equal("Grace", store.GetValue(iter, 0));
                Assert.Equal(45, store.GetValue(iter, 1));

                Assert.False(store.IterNext(ref iter));
            });
        }

        [Fact]
        public void A_list_store_reports_its_column_count_and_types()
        {
            Run(() =>
            {
                var store = new ListStore(typeof(string), typeof(int), typeof(bool));

                Assert.Equal(3, store.NColumns);
                Assert.Equal(GLib.GType.String, store.GetColumnType(0));
                Assert.Equal(GLib.GType.Int, store.GetColumnType(1));
                Assert.Equal(GLib.GType.Boolean, store.GetColumnType(2));
            });
        }

        [Fact]
        public void Setting_a_value_replaces_what_the_row_held()
        {
            Run(() =>
            {
                var store = NamesAndAges(("Ada", 36));
                store.GetIterFirst(out var iter);

                store.SetValue(iter, 1, 37);

                Assert.Equal(37, store.GetValue(iter, 1));
                Assert.Equal("Ada", store.GetValue(iter, 0));   // the other column is untouched
            });
        }

        [Fact]
        public void SetValue_reaches_every_overload_it_offers()
        {
            // There is one hand-written overload per CLR type, each boxing into
            // a GLib.Value of its own GType, so each needs its own oracle.
            Run(() =>
            {
                var store = new ListStore(typeof(bool), typeof(double), typeof(int),
                                          typeof(string), typeof(float), typeof(uint));
                var iter = store.Append();

                store.SetValue(iter, 0, true);
                store.SetValue(iter, 1, 2.5d);
                store.SetValue(iter, 2, -7);
                store.SetValue(iter, 3, "text");
                store.SetValue(iter, 4, 1.5f);
                store.SetValue(iter, 5, 4000000000u);

                Assert.Equal(true, store.GetValue(iter, 0));
                Assert.Equal(2.5d, store.GetValue(iter, 1));
                Assert.Equal(-7, store.GetValue(iter, 2));
                Assert.Equal("text", store.GetValue(iter, 3));
                Assert.Equal(1.5f, store.GetValue(iter, 4));
                Assert.Equal(4000000000u, store.GetValue(iter, 5));
            });
        }

        [Fact]
        public void Inserting_with_values_puts_the_row_at_the_position_asked_for()
        {
            Run(() =>
            {
                var store = NamesAndAges(("first", 1), ("third", 3));

                store.InsertWithValues(1, "second", 2);

                Assert.Equal(3, store.IterNChildren());
                Assert.Equal(new[] { "first", "second", "third" }, ColumnZero(store));
            });
        }

        [Fact]
        public void Removing_a_row_shortens_the_store_and_leaves_the_iter_on_the_next_one()
        {
            Run(() =>
            {
                var store = NamesAndAges(("a", 1), ("b", 2), ("c", 3));
                store.GetIterFirst(out var iter);

                // Remove returns whether the iter is still valid afterwards.
                Assert.True(store.Remove(ref iter));

                Assert.Equal(2, store.IterNChildren());
                Assert.Equal("b", store.GetValue(iter, 0));
                Assert.Equal(new[] { "b", "c" }, ColumnZero(store));
            });
        }

        [Fact]
        public void Removing_the_last_row_reports_the_iter_as_spent()
        {
            Run(() =>
            {
                var store = NamesAndAges(("only", 1));
                store.GetIterFirst(out var iter);

                Assert.False(store.Remove(ref iter));
                Assert.Equal(0, store.IterNChildren());
                Assert.False(store.GetIterFirst(out _));
            });
        }

        [Fact]
        public void Swapping_and_moving_rows_reorders_the_store()
        {
            Run(() =>
            {
                var store = NamesAndAges(("a", 1), ("b", 2), ("c", 3));

                store.GetIterFirst(out var iterA);
                store.IterNthChild(out var iterC, 2);

                store.Swap(iterA, iterC);
                Assert.Equal(new[] { "c", "b", "a" }, ColumnZero(store));

                // An iter identifies a row, not a position: after the swap
                // iterA still names "a", which is now last. Treating an iter as
                // an index is how reordering code quietly moves the wrong row.
                Assert.Equal("a", store.GetValue(iterA, 0));
                Assert.Equal("2", store.GetPath(iterA).ToString());

                store.MoveAfter(iterA, iterC);
                Assert.Equal(new[] { "c", "a", "b" }, ColumnZero(store));
            });
        }

        [Fact]
        public void Clearing_a_store_empties_it()
        {
            Run(() =>
            {
                var store = NamesAndAges(("a", 1), ("b", 2));

                store.Clear();

                Assert.Equal(0, store.IterNChildren());
                Assert.False(store.GetIterFirst(out _));
            });
        }

        [Fact]
        public void A_row_can_be_found_again_from_the_path_that_named_it()
        {
            Run(() =>
            {
                var store = NamesAndAges(("a", 1), ("b", 2), ("c", 3));

                var path = new TreePath("1");

                Assert.True(store.GetIter(out var iter, path));
                Assert.Equal("b", store.GetValue(iter, 0));

                // And back the other way.
                Assert.Equal("1", store.GetPath(iter).ToString());
            });
        }

        [Fact]
        public void A_path_past_the_end_is_refused_rather_than_returning_a_junk_iter()
        {
            Run(() =>
            {
                var store = NamesAndAges(("only", 1));

                Assert.False(store.GetIter(out _, new TreePath("5")));
            });
        }

        [Fact]
        public void Changing_a_row_raises_RowChanged_with_the_path_that_changed()
        {
            Run(() =>
            {
                var store = NamesAndAges(("a", 1), ("b", 2));
                store.IterNthChild(out var second, 1);

                string changed = null;
                store.RowChanged += (o, args) => changed = args.Path.ToString();

                store.SetValue(second, 0, "renamed");

                Assert.Equal("1", changed);
            });
        }

        [Fact]
        public void Appending_and_deleting_raise_RowInserted_and_RowDeleted()
        {
            Run(() =>
            {
                var store = NamesAndAges(("a", 1));

                string inserted = null, deleted = null;
                store.RowInserted += (o, args) => inserted = args.Path.ToString();
                store.RowDeleted += (o, args) => deleted = args.Path.ToString();

                store.AppendValues("b", 2);
                Assert.Equal("1", inserted);

                store.IterNthChild(out var iter, 1);
                store.Remove(ref iter);
                Assert.Equal("1", deleted);
            });
        }

        // --------------------------------------------------------- Gtk.TreeStore

        [Fact]
        public void A_tree_store_nests_rows_under_a_parent()
        {
            Run(() =>
            {
                var store = new TreeStore(typeof(string));

                var parent = store.AppendValues("root");
                store.AppendValues(parent, "child one");
                store.AppendValues(parent, "child two");

                Assert.Equal(1, store.IterNChildren());          // one row at the top
                Assert.Equal(2, store.IterNChildren(parent));

                Assert.True(store.IterChildren(out var child, parent));
                Assert.Equal("child one", store.GetValue(child, 0));

                Assert.True(store.IterNext(ref child));
                Assert.Equal("child two", store.GetValue(child, 0));
            });
        }

        [Fact]
        public void A_child_knows_its_parent_and_its_path_records_the_depth()
        {
            Run(() =>
            {
                var store = new TreeStore(typeof(string));

                var parent = store.AppendValues("root");
                var child = store.AppendValues(parent, "child");
                var grandchild = store.AppendValues(child, "grandchild");

                Assert.True(store.IterParent(out var found, grandchild));
                Assert.Equal("child", store.GetValue(found, 0));

                var path = store.GetPath(grandchild);
                Assert.Equal("0:0:0", path.ToString());
                Assert.Equal(3, path.Depth);
            });
        }

        [Fact]
        public void Removing_a_parent_takes_its_children_with_it()
        {
            Run(() =>
            {
                var store = new TreeStore(typeof(string));

                var doomed = store.AppendValues("doomed");
                store.AppendValues(doomed, "child");
                store.AppendValues("kept");

                store.Remove(ref doomed);

                Assert.Equal(1, store.IterNChildren());
                Assert.True(store.GetIterFirst(out var remaining));
                Assert.Equal("kept", store.GetValue(remaining, 0));
            });
        }

        [Fact]
        public void The_node_helpers_place_rows_where_their_names_say()
        {
            Run(() =>
            {
                var store = new TreeStore(typeof(string));

                var second = store.AppendNode();
                store.SetValue(second, 0, "second");

                var first = store.PrependNode();
                store.SetValue(first, 0, "first");

                var third = store.InsertNodeAfter(second);
                store.SetValue(third, 0, "third");

                Assert.Equal(new[] { "first", "second", "third" }, ColumnZero(store));
            });
        }

        // ---------------------------------------------------- Gtk.TreeModelSort

        [Fact]
        public void A_sorted_model_presents_the_rows_in_order_without_moving_them()
        {
            Run(() =>
            {
                var store = NamesAndAges(("charlie", 3), ("alpha", 1), ("bravo", 2));

                var sorted = new TreeModelSort(store);
                sorted.SetSortColumnId(0, SortType.Ascending);

                Assert.Equal(new[] { "alpha", "bravo", "charlie" }, ColumnZero(sorted));

                // The underlying store keeps its own order.
                Assert.Equal(new[] { "charlie", "alpha", "bravo" }, ColumnZero(store));
            });
        }

        [Fact]
        public void Sorting_descending_reverses_the_order()
        {
            Run(() =>
            {
                var store = NamesAndAges(("alpha", 1), ("bravo", 2), ("charlie", 3));

                var sorted = new TreeModelSort(store);
                sorted.SetSortColumnId(1, SortType.Descending);

                Assert.Equal(new[] { "charlie", "bravo", "alpha" }, ColumnZero(sorted));
            });
        }

        [Fact]
        public void A_sorted_iter_converts_back_to_the_row_it_stands_for()
        {
            Run(() =>
            {
                var store = NamesAndAges(("charlie", 3), ("alpha", 1));

                var sorted = new TreeModelSort(store);
                sorted.SetSortColumnId(0, SortType.Ascending);

                sorted.GetIterFirst(out var sortedIter);          // "alpha"
                var childIter = sorted.ConvertIterToChildIter(sortedIter);

                Assert.Equal("alpha", store.GetValue(childIter, 0));
                Assert.Equal("1", store.GetPath(childIter).ToString());
            });
        }

        // -------------------------------------------------- Gtk.TreeModelFilter

        [Fact]
        public void A_filter_hides_the_rows_its_function_rejects()
        {
            Run(() =>
            {
                var store = NamesAndAges(("keep", 1), ("drop", 2), ("keep too", 3));

                // TreeModelFilter has no constructor taking a child model; the only
                // way to build one is the child model's own FilterNew, mirroring C.
                var filtered = (TreeModelFilter) store.FilterNew(null);
                filtered.VisibleFunc = (model, iter) =>
                    ((string) model.GetValue(iter, 0)).StartsWith("keep");

                Assert.Equal(new[] { "keep", "keep too" }, ColumnZero(filtered));
            });
        }

        [Fact]
        public void Refiltering_picks_up_a_change_that_makes_a_row_visible()
        {
            Run(() =>
            {
                var store = NamesAndAges(("hidden", 1), ("shown", 2));

                // TreeModelFilter has no constructor taking a child model; the only
                // way to build one is the child model's own FilterNew, mirroring C.
                var filtered = (TreeModelFilter) store.FilterNew(null);
                filtered.VisibleFunc = (model, iter) =>
                    (string) model.GetValue(iter, 0) == "shown";

                Assert.Single(ColumnZero(filtered));

                store.GetIterFirst(out var first);
                store.SetValue(first, 0, "shown");
                filtered.Refilter();

                Assert.Equal(2, ColumnZero(filtered).Length);
            });
        }

        [Fact]
        public void A_filtered_iter_converts_back_to_the_underlying_row()
        {
            Run(() =>
            {
                var store = NamesAndAges(("drop", 1), ("keep", 2));

                // TreeModelFilter has no constructor taking a child model; the only
                // way to build one is the child model's own FilterNew, mirroring C.
                var filtered = (TreeModelFilter) store.FilterNew(null);
                filtered.VisibleFunc = (model, iter) =>
                    (string) model.GetValue(iter, 0) == "keep";

                Assert.True(filtered.GetIterFirst(out var filteredIter));

                var childIter = filtered.ConvertIterToChildIter(filteredIter);

                Assert.Equal("keep", store.GetValue(childIter, 0));
                Assert.Equal("1", store.GetPath(childIter).ToString());
            });
        }

        // ------------------------------------------------------------ Gtk.TreePath

        [Fact]
        public void A_tree_path_parses_and_prints_the_same_string()
        {
            Run(() =>
            {
                var path = new TreePath("2:0:5");

                Assert.Equal(3, path.Depth);
                Assert.Equal(new[] { 2, 0, 5 }, path.Indices);
                Assert.Equal("2:0:5", path.ToString());
            });
        }

        [Fact]
        public void Walking_a_tree_path_up_and_down_is_reversible()
        {
            Run(() =>
            {
                var path = new TreePath("1:2");

                path.Down();
                Assert.Equal("1:2:0", path.ToString());

                Assert.True(path.Up());
                Assert.Equal("1:2", path.ToString());

                path.Next();
                Assert.Equal("1:3", path.ToString());

                Assert.True(path.Prev());
                Assert.Equal("1:2", path.ToString());
            });
        }

        [Fact]
        public void Tree_paths_compare_in_tree_order()
        {
            Run(() =>
            {
                Assert.True(new TreePath("0").Compare(new TreePath("1")) < 0);
                Assert.True(new TreePath("1:0").Compare(new TreePath("1")) > 0);
                Assert.Equal(0, new TreePath("2:3").Compare(new TreePath("2:3")));

                Assert.True(new TreePath("1").IsAncestor(new TreePath("1:0")));
                Assert.True(new TreePath("1:0").IsDescendant(new TreePath("1")));
            });
        }

        /// <summary>Reads column 0 of every top-level row, in model order.</summary>
        private static string[] ColumnZero(ITreeModel model)
        {
            var values = new System.Collections.Generic.List<string>();

            if (model.GetIterFirst(out var iter))
                do
                    values.Add((string) model.GetValue(iter, 0));
                while (model.IterNext(ref iter));

            return values.ToArray();
        }
    }
}
