using System;
using System.Linq;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The hand-written halves of <c>TreeModelFilter</c>, <c>TreeModelSort</c> and
    /// <c>TreeStore</c>, plus <c>NodeSelection</c>. The earlier tree tests went
    /// through the generated <c>ITreeModel</c> surface; these reach the overloads
    /// written by hand on each wrapper — one <c>SetValue</c> per CLR type, the
    /// iterator conversions in both directions, and the node-level selection API,
    /// which had no coverage at all.
    /// </summary>
    public class TreeWrapperTests : GtkTestBase
    {
        public TreeWrapperTests(GtkFixture fixture) : base(fixture) { }

        private static ListStore NamesAndAges(params (string name, int age)[] rows)
        {
            var store = new ListStore(typeof(string), typeof(int));
            foreach (var (name, age) in rows)
                store.AppendValues(name, age);
            return store;
        }

        private static string[] ColumnZero(ITreeModel model)
        {
            var values = new System.Collections.Generic.List<string>();

            if (model.GetIterFirst(out var iter))
                do
                    values.Add((string) model.GetValue(iter, 0));
                while (model.IterNext(ref iter));

            return values.ToArray();
        }

        // -------------------------------------------------- TreeModelSort

        [Fact]
        public void A_sorted_model_reports_its_own_child_count_and_nth_child()
        {
            Run(() =>
            {
                var store = NamesAndAges(("charlie", 3), ("alpha", 1), ("bravo", 2));

                var sorted = new TreeModelSort(store);
                sorted.SetSortColumnId(0, SortType.Ascending);

                Assert.Equal(3, sorted.IterNChildren());

                Assert.True(sorted.IterNthChild(out var second, 1));
                Assert.Equal("bravo", sorted.GetValue(second, 0));

                Assert.True(sorted.IterChildren(out var first));
                Assert.Equal("alpha", sorted.GetValue(first, 0));
            });
        }

        [Fact]
        public void Writing_through_a_sorted_model_is_refused_with_the_way_to_do_it()
        {
            // GtkTreeModel has no set operation -- writing a row is the store's
            // job, not the view's. These threw NotImplementedException, which
            // reads as "unfinished" rather than "ask the child model", so the
            // message now says which call to make instead.
            Run(() =>
            {
                var store = NamesAndAges(("beta", 2), ("alpha", 1));

                var sorted = new TreeModelSort(store);
                sorted.SetSortColumnId(0, SortType.Ascending);

                sorted.GetIterFirst(out var iter);          // "alpha"

                var error = Assert.Throws<NotSupportedException>(() => sorted.SetValue(iter, 1, 99));
                Assert.Contains("ConvertIterToChildIter", error.Message);

                // And the way the message describes does work. Writing to the
                // child makes the sorted model re-emit its rows, which spends
                // the iter that was held across the call -- so it is fetched
                // again rather than reused.
                var childRow = sorted.ConvertIterToChildIter(iter);
                store.SetValue(childRow, 1, 99);

                Assert.True(sorted.GetIterFirst(out var reread));
                Assert.Equal("alpha", sorted.GetValue(reread, 0));
                Assert.Equal(99, sorted.GetValue(reread, 1));
            });
        }

        [Fact]
        public void Iterator_conversion_round_trips_between_the_sorted_model_and_its_child()
        {
            Run(() =>
            {
                var store = NamesAndAges(("charlie", 3), ("alpha", 1), ("bravo", 2));

                var sorted = new TreeModelSort(store);
                sorted.SetSortColumnId(0, SortType.Ascending);

                store.GetIterFirst(out var childIter);      // "charlie", row 0
                var sortedIter = sorted.ConvertChildIterToIter(childIter);

                // Sorted ascending, "charlie" is last.
                Assert.Equal("2", sorted.GetPath(sortedIter).ToString());
                Assert.Equal("charlie", sorted.GetValue(sortedIter, 0));

                var back = sorted.ConvertIterToChildIter(sortedIter);
                Assert.Equal("0", store.GetPath(back).ToString());
            });
        }

        [Fact]
        public void Path_conversion_agrees_with_iterator_conversion()
        {
            Run(() =>
            {
                var store = NamesAndAges(("charlie", 3), ("alpha", 1));

                var sorted = new TreeModelSort(store);
                sorted.SetSortColumnId(0, SortType.Ascending);

                var sortedPath = sorted.ConvertChildPathToPath(new TreePath("0"));
                Assert.Equal("1", sortedPath.ToString());

                Assert.Equal("0", sorted.ConvertPathToChildPath(sortedPath).ToString());
            });
        }

        [Fact]
        public void Appending_to_a_sorted_model_is_refused_rather_than_overflowing_the_stack()
        {
            // AppendValues read "return AppendValues ((Array) values);". There
            // is no AppendValues (Array) overload, so the cast bound straight
            // back to this method with the array wrapped in a fresh object[]:
            // infinite recursion, and a stack overflow that takes the process
            // down rather than raising anything a caller could catch.
            Run(() =>
            {
                var store = NamesAndAges(("alpha", 1), ("charlie", 3));

                var sorted = new TreeModelSort(store);
                sorted.SetSortColumnId(0, SortType.Ascending);

                Assert.Throws<NotSupportedException>(() => sorted.AppendValues("bravo", 2));

                // Appending to the child model is what works, and the row turns
                // up here in sort order.
                store.AppendValues("bravo", 2);

                Assert.Equal(new[] { "alpha", "bravo", "charlie" }, ColumnZero(sorted));
            });
        }

        [Fact]
        public void Changing_the_sort_column_reorders_and_reports_the_new_column()
        {
            Run(() =>
            {
                var store = NamesAndAges(("alpha", 3), ("bravo", 1), ("charlie", 2));

                var sorted = new TreeModelSort(store);

                sorted.SetSortColumnId(1, SortType.Ascending);
                Assert.Equal(new[] { "bravo", "charlie", "alpha" }, ColumnZero(sorted));

                Assert.True(sorted.GetSortColumnId(out var column, out var order));
                Assert.Equal(1, column);
                Assert.Equal(SortType.Ascending, order);

                sorted.SetSortColumnId(0, SortType.Descending);
                Assert.Equal(new[] { "charlie", "bravo", "alpha" }, ColumnZero(sorted));
            });
        }

        // ------------------------------------------------ TreeModelFilter

        private static TreeModelFilter FilterOf(ITreeModel child, TreeModelFilterVisibleFunc visible)
        {
            var filter = (TreeModelFilter) child.FilterNew(null);
            filter.VisibleFunc = visible;
            return filter;
        }

        [Fact]
        public void A_filtered_model_reports_its_own_child_count_and_nth_child()
        {
            Run(() =>
            {
                var store = NamesAndAges(("keep a", 1), ("drop", 2), ("keep b", 3));

                var filtered = FilterOf(store,
                    (model, iter) => ((string) model.GetValue(iter, 0)).StartsWith("keep"));

                Assert.Equal(2, filtered.IterNChildren());

                Assert.True(filtered.IterNthChild(out var second, 1));
                Assert.Equal("keep b", filtered.GetValue(second, 0));

                Assert.True(filtered.IterChildren(out var first));
                Assert.Equal("keep a", filtered.GetValue(first, 0));
            });
        }

        [Fact]
        public void Writing_through_a_filter_is_refused_the_same_way()
        {
            Run(() =>
            {
                var store = NamesAndAges(("drop", 1), ("keep", 2));

                var filtered = FilterOf(store,
                    (model, iter) => (string) model.GetValue(iter, 0) == "keep");

                filtered.GetIterFirst(out var iter);

                var error = Assert.Throws<NotSupportedException>(() => filtered.SetValue(iter, 1, 42));
                Assert.Contains("ConvertIterToChildIter", error.Message);

                var childRow = filtered.ConvertIterToChildIter(iter);
                store.SetValue(childRow, 1, 42);

                Assert.Equal(42, filtered.GetValue(iter, 1));
            });
        }

        [Fact]
        public void Filtered_iterator_conversion_round_trips()
        {
            Run(() =>
            {
                var store = NamesAndAges(("drop", 1), ("keep", 2));

                var filtered = FilterOf(store,
                    (model, iter) => (string) model.GetValue(iter, 0) == "keep");

                store.IterNthChild(out var childIter, 1);   // "keep"
                var filteredIter = filtered.ConvertChildIterToIter(childIter);

                Assert.Equal("0", filtered.GetPath(filteredIter).ToString());

                var back = filtered.ConvertIterToChildIter(filteredIter);
                Assert.Equal("1", store.GetPath(back).ToString());
            });
        }

        [Fact]
        public void A_filter_with_a_root_shows_only_that_subtree()
        {
            Run(() =>
            {
                var store = new TreeStore(typeof(string));

                var first = store.AppendValues("first");
                store.AppendValues(first, "first child");
                var second = store.AppendValues("second");
                store.AppendValues(second, "second child a");
                store.AppendValues(second, "second child b");

                // Rooted at the second top-level row, its children become the
                // filtered model's top level.
                var filtered = (TreeModelFilter) store.FilterNew(new TreePath("1"));

                Assert.Equal(new[] { "second child a", "second child b" }, ColumnZero(filtered));
            });
        }

        [Fact]
        public void A_modify_func_can_present_a_column_the_child_model_does_not_have()
        {
            // SetModifyFunc is the reason TreeModelFilter is more than a filter:
            // it lets the wrapper synthesise columns. Nothing had exercised it.
            Run(() =>
            {
                var store = NamesAndAges(("ada", 36), ("grace", 45));

                var filtered = (TreeModelFilter) store.FilterNew(null);

                filtered.SetModifyFunc(1, new[] { GLib.GType.String },
                    (ITreeModel model, TreeIter iter, ref GLib.Value value, int column) =>
                    {
                        var child = filtered.ConvertIterToChildIter(iter);
                        var name = (string) store.GetValue(child, 0);
                        value = new GLib.Value(name.ToUpperInvariant());
                    });

                Assert.Equal(new[] { "ADA", "GRACE" }, ColumnZero(filtered));
            });
        }

        // ------------------------------------------------------ TreeStore

        [Fact]
        public void SetValues_writes_a_whole_row_at_once()
        {
            Run(() =>
            {
                var store = new TreeStore(typeof(string), typeof(int), typeof(bool));
                var iter = store.AppendNode();

                store.SetValues(iter, "written", 7, true);

                Assert.Equal("written", store.GetValue(iter, 0));
                Assert.Equal(7, store.GetValue(iter, 1));
                Assert.Equal(true, store.GetValue(iter, 2));
            });
        }

        [Fact]
        public void Every_TreeStore_SetValue_overload_writes_its_own_type()
        {
            Run(() =>
            {
                var store = new TreeStore(typeof(bool), typeof(double), typeof(int),
                                          typeof(string), typeof(float), typeof(uint));
                var iter = store.AppendNode();

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
        public void The_insert_node_helpers_place_children_where_their_names_say()
        {
            Run(() =>
            {
                var store = new TreeStore(typeof(string));
                var parent = store.AppendValues("parent");

                var second = store.AppendNode(parent);
                store.SetValue(second, 0, "second");

                var first = store.PrependNode(parent);
                store.SetValue(first, 0, "first");

                var third = store.InsertNodeAfter(parent, second);
                store.SetValue(third, 0, "third");

                var zeroth = store.InsertNodeBefore(parent, first);
                store.SetValue(zeroth, 0, "zeroth");

                Assert.Equal(new[] { "zeroth", "first", "second", "third" },
                             ChildrenOf(store, parent));
            });
        }

        [Fact]
        public void InsertNode_at_a_position_places_the_row_there()
        {
            Run(() =>
            {
                var store = new TreeStore(typeof(string));
                store.AppendValues("a");
                store.AppendValues("c");

                var inserted = store.InsertNode(1);
                store.SetValue(inserted, 0, "b");

                Assert.Equal(new[] { "a", "b", "c" }, ColumnZero(store));
            });
        }

        [Fact]
        public void A_tree_store_reports_ancestry_between_rows()
        {
            Run(() =>
            {
                var store = new TreeStore(typeof(string));

                var parent = store.AppendValues("parent");
                var child = store.AppendValues(parent, "child");
                var stranger = store.AppendValues("stranger");

                Assert.True(store.IsAncestor(parent, child));
                Assert.False(store.IsAncestor(stranger, child));
                Assert.Equal(0, store.IterDepth(parent));
                Assert.Equal(1, store.IterDepth(child));
            });
        }

        [Fact]
        public void Moving_and_swapping_rows_inside_a_parent_reorders_them()
        {
            Run(() =>
            {
                var store = new TreeStore(typeof(string));
                var parent = store.AppendValues("parent");

                var a = store.AppendValues(parent, "a");
                var b = store.AppendValues(parent, "b");
                var c = store.AppendValues(parent, "c");

                store.Swap(a, c);
                Assert.Equal(new[] { "c", "b", "a" }, ChildrenOf(store, parent));

                store.MoveBefore(a, c);
                Assert.Equal(new[] { "a", "c", "b" }, ChildrenOf(store, parent));
            });
        }

        // -------------------------------------------------- NodeSelection

        [TreeNode(ListOnly = true)]
        public class Row : TreeNode
        {
            public Row(string label) => Label = label;

            [TreeNodeValue(Column = 0)]
            public string Label { get; set; }
        }

        private static (NodeView view, NodeStore store) Rows(params string[] labels)
        {
            var store = new NodeStore(typeof(Row));
            foreach (var label in labels)
                store.AddNode(new Row(label));

            var view = new NodeView(store);
            view.AppendColumn("Label", new CellRendererText(), "text", 0);
            return (view, store);
        }

        [Fact]
        public void Selecting_a_node_makes_it_the_selected_node()
        {
            Run(() =>
            {
                var (view, store) = Rows("a", "b", "c");
                var second = store.Cast<Row>().ElementAt(1);

                view.NodeSelection.SelectNode(second);

                Assert.True(view.NodeSelection.NodeIsSelected(second));
                Assert.Single(view.NodeSelection.SelectedNodes);
                Assert.Same(second, view.NodeSelection.SelectedNodes[0]);
            });
        }

        [Fact]
        public void Unselecting_a_node_leaves_nothing_selected()
        {
            Run(() =>
            {
                var (view, store) = Rows("a", "b");
                var first = store.Cast<Row>().First();

                view.NodeSelection.SelectNode(first);
                Assert.True(view.NodeSelection.NodeIsSelected(first));

                view.NodeSelection.UnselectNode(first);

                Assert.False(view.NodeSelection.NodeIsSelected(first));
                Assert.Empty(view.NodeSelection.SelectedNodes);
            });
        }

        [Fact]
        public void Selecting_a_range_selects_every_node_between_its_ends()
        {
            Run(() =>
            {
                var (view, store) = Rows("a", "b", "c", "d");
                view.NodeSelection.Mode = SelectionMode.Multiple;

                var nodes = store.Cast<Row>().ToArray();
                view.NodeSelection.SelectRange(nodes[1], nodes[2]);

                Assert.Equal(2, view.NodeSelection.SelectedNodes.Length);
                Assert.False(view.NodeSelection.NodeIsSelected(nodes[0]));
                Assert.True(view.NodeSelection.NodeIsSelected(nodes[1]));
                Assert.True(view.NodeSelection.NodeIsSelected(nodes[2]));
                Assert.False(view.NodeSelection.NodeIsSelected(nodes[3]));
            });
        }

        [Fact]
        public void Select_all_and_unselect_all_move_every_node_at_once()
        {
            Run(() =>
            {
                var (view, store) = Rows("a", "b", "c");
                view.NodeSelection.Mode = SelectionMode.Multiple;

                view.NodeSelection.SelectAll();
                Assert.Equal(3, view.NodeSelection.SelectedNodes.Length);

                view.NodeSelection.UnselectAll();
                Assert.Empty(view.NodeSelection.SelectedNodes);
            });
        }

        [Fact]
        public void Selecting_by_path_agrees_with_selecting_by_node()
        {
            Run(() =>
            {
                var (view, store) = Rows("a", "b");
                var second = store.Cast<Row>().ElementAt(1);

                view.NodeSelection.SelectPath(new TreePath("1"));

                Assert.True(view.NodeSelection.PathIsSelected(new TreePath("1")));
                Assert.True(view.NodeSelection.NodeIsSelected(second));

                view.NodeSelection.UnselectPath(new TreePath("1"));

                Assert.False(view.NodeSelection.PathIsSelected(new TreePath("1")));
            });
        }

        [Fact]
        public void The_selection_mode_is_reported_back_and_Single_allows_only_one()
        {
            Run(() =>
            {
                var (view, store) = Rows("a", "b");
                var nodes = store.Cast<Row>().ToArray();

                view.NodeSelection.Mode = SelectionMode.Single;
                Assert.Equal(SelectionMode.Single, view.NodeSelection.Mode);

                view.NodeSelection.SelectNode(nodes[0]);
                view.NodeSelection.SelectNode(nodes[1]);

                // Single mode drops the earlier selection rather than adding.
                Assert.Single(view.NodeSelection.SelectedNodes);
                Assert.Same(nodes[1], view.NodeSelection.SelectedNodes[0]);
            });
        }

        [Fact]
        public void Changing_the_selection_raises_Changed()
        {
            Run(() =>
            {
                var (view, store) = Rows("a", "b");
                var changes = 0;

                view.NodeSelection.Changed += (o, args) => changes++;

                view.NodeSelection.SelectNode(store.Cast<Row>().First());

                Assert.True(changes > 0, "selecting a node should raise Changed");
            });
        }

        private static string[] ChildrenOf(TreeStore store, TreeIter parent)
        {
            var values = new System.Collections.Generic.List<string>();

            if (store.IterChildren(out var child, parent))
                do
                    values.Add((string) store.GetValue(child, 0));
                while (store.IterNext(ref child));

            return values.ToArray();
        }
    }
}
