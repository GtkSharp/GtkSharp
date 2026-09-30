using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The legacy tree <em>view</em>: <c>TreeView</c>, <c>TreeViewColumn</c>,
    /// <c>TreeSelection</c>, <c>TreeRowReference</c>, the cell renderers,
    /// <c>CellArea</c>/<c>ICellLayout</c>, <c>ITreeSortable</c> and the two
    /// drag-and-drop interfaces every store implements.
    /// <para>
    /// <c>TreeModelTests</c>, <c>TreeWrapperTests</c> and <c>NodeStoreTests</c>
    /// cover the models; nothing covered what displays them. Deprecated in Gtk
    /// 4.10 and still shipped in 4.22, this is what a ported application arrives
    /// holding, and it is where the largest block of untested hand-written
    /// <c>GtkSharp</c> code lives.
    /// </para>
    /// </summary>
    public class TreeViewStackTests : GtkTestBase
    {
        public TreeViewStackTests(GtkFixture fixture) : base(fixture) { }

        private static ListStore NamesAndCities(params (string name, string city)[] rows)
        {
            var store = new ListStore(typeof(string), typeof(string));
            foreach (var (name, city) in rows)
                store.AppendValues(name, city);
            return store;
        }

        /// <summary>Reads column 0 of every top-level row, in model order.</summary>
        private static string[] ColumnZero(ITreeModel model)
        {
            var values = new List<string>();

            if (model.GetIterFirst(out var iter))
                do
                    values.Add((string) model.GetValue(iter, 0));
                while (model.IterNext(ref iter));

            return values.ToArray();
        }

        // ------------------------------------------------ TreeViewColumn: attributes

        [Fact]
        public void A_column_attribute_copies_a_model_column_into_the_renderer()
        {
            // The whole point of an attribute mapping: CellSetCellData is what a
            // TreeView calls before drawing each row, and its only observable
            // effect is the property it writes on the renderer.
            Run(() =>
            {
                var store = NamesAndCities(("Ada", "London"), ("Grace", "New York"));
                var cell = new CellRendererText();
                var column = new TreeViewColumn("Name", cell, "text", 0);

                store.GetIterFirst(out var first);
                column.CellSetCellData(store, first, false, false);
                Assert.Equal("Ada", cell.Text);

                store.IterNthChild(out var second, 1);
                column.CellSetCellData(store, second, false, false);
                Assert.Equal("Grace", cell.Text);
            });
        }

        [Fact]
        public void SetAttributes_replaces_the_mapping_rather_than_adding_to_it()
        {
            // SetAttributes is hand-written on TreeViewColumn, CellAreaBox and as
            // an extension method over ICellLayout, and all three are
            // ClearAttributes followed by AddAttribute. Without the clear, the
            // old mapping would still be applied and the last one written would
            // decide the value -- which is exactly the bug the clear prevents.
            Run(() =>
            {
                var store = NamesAndCities(("Ada", "London"));
                var cell = new CellRendererText();
                var column = new TreeViewColumn("Name", cell, "text", 0);

                store.GetIterFirst(out var iter);
                column.CellSetCellData(store, iter, false, false);
                Assert.Equal("Ada", cell.Text);

                column.SetAttributes(cell, "text", 1);
                column.CellSetCellData(store, iter, false, false);
                Assert.Equal("London", cell.Text);
            });
        }

        [Fact]
        public void An_odd_number_of_attribute_arguments_is_refused()
        {
            Run(() =>
            {
                var column = new TreeViewColumn();
                var cell = new CellRendererText();

                Assert.Throws<ArgumentException>(() => column.SetAttributes(cell, "text"));
            });
        }

        [Fact]
        public void A_cell_data_func_runs_after_the_attributes_and_wins()
        {
            // Gtk applies the attribute mapping first and then calls the data
            // func, so a func that writes the same property overrides it. That
            // ordering is the reason a data func is useful at all.
            Run(() =>
            {
                var store = NamesAndCities(("Ada", "London"), ("Grace", "New York"));
                var cell = new CellRendererText();
                var column = new TreeViewColumn("Name", cell, "text", 0);

                var rowsSeen = new List<string>();
                column.SetCellDataFunc(cell, (CellLayoutDataFunc) ((layout, renderer, model, iter) =>
                {
                    var name = (string) model.GetValue(iter, 0);
                    rowsSeen.Add(name);
                    ((CellRendererText) renderer).Text = name.ToUpperInvariant();
                }));

                store.GetIterFirst(out var first);
                column.CellSetCellData(store, first, false, false);
                Assert.Equal("ADA", cell.Text);

                store.IterNthChild(out var second, 1);
                column.CellSetCellData(store, second, false, false);
                Assert.Equal("GRACE", cell.Text);

                // The func was handed each row exactly once, in the order asked.
                Assert.Equal(new[] { "Ada", "Grace" }, rowsSeen);
            });
        }

        [Fact]
        public void Clearing_a_cell_data_func_hands_the_column_back_to_its_attributes()
        {
            Run(() =>
            {
                var store = NamesAndCities(("Ada", "London"));
                var cell = new CellRendererText();
                var column = new TreeViewColumn("Name", cell, "text", 0);

                column.SetCellDataFunc(cell, (CellLayoutDataFunc) ((layout, renderer, model, iter) =>
                    ((CellRendererText) renderer).Text = "from the func"));

                store.GetIterFirst(out var iter);
                column.CellSetCellData(store, iter, false, false);
                Assert.Equal("from the func", cell.Text);

                column.SetCellDataFunc(cell, (CellLayoutDataFunc) null);
                column.CellSetCellData(store, iter, false, false);
                Assert.Equal("Ada", cell.Text);
            });
        }

        [Fact]
        public void Clearing_a_columns_attributes_stops_it_writing_the_renderer()
        {
            // ClearAttributes leaves the renderer packed -- only the mapping goes
            // -- so the cell keeps whatever it was last given rather than being
            // reset. A column that looks stale on screen is what this produces.
            Run(() =>
            {
                var store = NamesAndCities(("Ada", "London"), ("Grace", "New York"));
                var cell = new CellRendererText();
                var column = new TreeViewColumn("Name", cell, "text", 0);

                store.GetIterFirst(out var first);
                column.CellSetCellData(store, first, false, false);
                Assert.Equal("Ada", cell.Text);

                column.ClearAttributes(cell);
                Assert.Single(column.Cells);

                store.IterNthChild(out var second, 1);
                column.CellSetCellData(store, second, false, false);
                Assert.Equal("Ada", cell.Text);

                // Clear() removes the renderers themselves.
                column.Clear();
                Assert.Empty(column.Cells);
            });
        }

        // ------------------------------------------------- TreeView: column list

        [Fact]
        public void Columns_are_reported_in_the_order_they_were_inserted_and_moved()
        {
            Run(() =>
            {
                var view = new TreeView(NamesAndCities(("Ada", "London")));

                var name = view.AppendColumn("Name", new CellRendererText(), "text", 0);
                var city = view.AppendColumn("City", new CellRendererText(), "text", 1);
                var extra = new TreeViewColumn { Title = "Extra" };
                view.InsertColumn(extra, 1);

                Assert.Equal(3u, view.NColumns);
                Assert.Equal(new[] { "Name", "Extra", "City" },
                             view.Columns.Select(c => c.Title).ToArray());

                // A null base column means "move to the front".
                view.MoveColumnAfter(city, null);
                Assert.Equal(new[] { "City", "Name", "Extra" },
                             view.Columns.Select(c => c.Title).ToArray());

                Assert.Equal(2, view.RemoveColumn(name));
                Assert.Equal(new[] { "City", "Extra" },
                             view.Columns.Select(c => c.Title).ToArray());
                Assert.Equal(view, city.TreeView);
            });
        }

        [Fact]
        public void A_column_appended_with_a_data_func_is_driven_by_that_func()
        {
            // TreeView.AppendColumn(title, cell, TreeCellDataFunc) is one of the
            // hand-written overloads: it builds the column, packs the cell and
            // installs the func in one step, and nothing had called it.
            Run(() =>
            {
                var store = NamesAndCities(("Ada", "London"), ("Grace", "New York"));
                var view = new TreeView(store);
                var cell = new CellRendererText();

                var column = view.AppendColumn("Initial", cell, (TreeCellDataFunc)
                    ((treeColumn, renderer, model, iter) =>
                        ((CellRendererText) renderer).Text =
                            ((string) model.GetValue(iter, 0)).Substring(0, 1)));

                Assert.Equal("Initial", column.Title);
                Assert.Same(column, view.GetColumn(0));

                store.IterNthChild(out var second, 1);
                column.CellSetCellData(store, second, false, false);
                Assert.Equal("G", cell.Text);
            });
        }

        // ------------------------------------------------------------- sorting

        [Fact]
        public void Clicking_a_sortable_column_sorts_the_model_and_toggles_the_order()
        {
            // Setting a sort column id is what wires a header to the model's
            // sortable interface: the click is handled by Gtk, not by the
            // application, and the oracle is the order the rows come back in.
            Run(() =>
            {
                var store = NamesAndCities(("charlie", "c"), ("alpha", "a"), ("bravo", "b"));
                var view = new TreeView(store);
                var column = view.AppendColumn("Name", new CellRendererText(), "text", 0);

                column.SortColumnId = 0;
                Assert.True(column.Clickable);      // set_sort_column_id makes it so

                column.Click();
                Assert.Equal(new[] { "alpha", "bravo", "charlie" }, ColumnZero(store));
                Assert.True(column.SortIndicator);
                Assert.Equal(SortType.Ascending, column.SortOrder);

                column.Click();
                Assert.Equal(new[] { "charlie", "bravo", "alpha" }, ColumnZero(store));
                Assert.Equal(SortType.Descending, column.SortOrder);
            });
        }

        [Fact]
        public void A_sort_func_decides_the_order_rather_than_the_column_type()
        {
            // A TreeIterCompareFunc is a managed callback Gtk calls during its
            // own sort. Ordering by length puts the rows in an order no
            // comparison of the strings themselves produces, so the func is
            // demonstrably what decided it.
            Run(() =>
            {
                var store = NamesAndCities(("elephant", "x"), ("ox", "y"), ("camel", "z"));

                store.SetSortFunc(0, (model, a, b) =>
                    ((string) model.GetValue(a, 0)).Length
                        .CompareTo(((string) model.GetValue(b, 0)).Length));
                store.SetSortColumnId(0, SortType.Ascending);

                Assert.Equal(new[] { "ox", "camel", "elephant" }, ColumnZero(store));

                store.SetSortColumnId(0, SortType.Descending);
                Assert.Equal(new[] { "elephant", "camel", "ox" }, ColumnZero(store));
            });
        }

        [Fact]
        public void The_default_sort_func_answers_for_the_default_sort_column()
        {
            // GTK_TREE_SORTABLE_DEFAULT_SORT_COLUMN_ID is -2 and
            // GTK_TREE_SORTABLE_UNSORTED_SORT_COLUMN_ID is -1; neither constant
            // is bound, and swapping them silently gives an unsorted model.
            const int DefaultSortColumnId = -1;

            Run(() =>
            {
                var store = NamesAndCities(("charlie", "c"), ("alpha", "a"), ("bravo", "b"));

                Assert.False(store.HasDefaultSortFunc);

                store.DefaultSortFunc = (model, a, b) =>
                    string.CompareOrdinal((string) model.GetValue(b, 0),
                                          (string) model.GetValue(a, 0));

                Assert.True(store.HasDefaultSortFunc);

                store.SetSortColumnId(DefaultSortColumnId, SortType.Ascending);
                Assert.Equal(new[] { "charlie", "bravo", "alpha" }, ColumnZero(store));
            });
        }

        [Fact]
        public void Changing_the_sort_column_reports_it_through_the_signal_and_the_getter()
        {
            Run(() =>
            {
                var store = NamesAndCities(("b", "1"), ("a", "2"));

                var changes = 0;
                store.SortColumnChanged += (o, args) => changes++;

                store.SetSortColumnId(1, SortType.Descending);

                Assert.Equal(1, changes);
                Assert.True(store.GetSortColumnId(out var column, out var order));
                Assert.Equal(1, column);
                Assert.Equal(SortType.Descending, order);
            });
        }

        // ----------------------------------------------------- TreeSelection

        private static TreeView ViewOf(params string[] names)
        {
            var store = new ListStore(typeof(string));
            foreach (var name in names)
                store.AppendValues(name);

            var view = new TreeView(store);
            view.AppendColumn("Name", new CellRendererText(), "text", 0);
            return view;
        }

        [Fact]
        public void Selecting_a_path_makes_it_the_selected_row_and_raises_changed()
        {
            Run(() =>
            {
                var view = ViewOf("a", "b", "c");
                var selection = view.Selection;

                var changes = 0;
                selection.Changed += (o, args) => changes++;

                selection.SelectPath(new TreePath("1"));

                Assert.Equal(1, changes);
                Assert.Equal(1, selection.CountSelectedRows());
                Assert.True(selection.PathIsSelected(new TreePath("1")));

                // The hand-written one-argument overload passes NULL for the
                // model, so the iter has to come back on its own.
                Assert.True(selection.GetSelected(out var iter));
                Assert.Equal("b", view.Model.GetValue(iter, 0));
                Assert.True(selection.IterIsSelected(iter));
            });
        }

        [Fact]
        public void GetSelected_also_hands_back_the_model_the_view_is_showing()
        {
            Run(() =>
            {
                var view = ViewOf("a", "b");
                view.Selection.SelectPath(new TreePath("0"));

                Assert.True(view.Selection.GetSelected(out ITreeModel model, out var iter));
                Assert.Same(view.Model, model);
                Assert.Equal("a", model.GetValue(iter, 0));
                Assert.Same(view, view.Selection.TreeView);
            });
        }

        [Fact]
        public void A_multiple_selection_reports_its_rows_in_path_order()
        {
            Run(() =>
            {
                var view = ViewOf("a", "b", "c", "d");
                var selection = view.Selection;
                selection.Mode = SelectionMode.Multiple;

                selection.SelectRange(new TreePath("1"), new TreePath("2"));

                Assert.Equal(2, selection.CountSelectedRows());

                // The hand-written GetSelectedRows() passes NULL for the model.
                Assert.Equal(new[] { "1", "2" },
                             selection.GetSelectedRows().Select(p => p.ToString()).ToArray());

                // And the generated overload that also returns it.
                var paths = selection.GetSelectedRows(out ITreeModel model);
                Assert.Same(view.Model, model);
                Assert.Equal(new[] { "1", "2" }, paths.Select(p => p.ToString()).ToArray());

                selection.UnselectRange(new TreePath("1"), new TreePath("1"));
                Assert.Equal(new[] { "2" },
                             selection.GetSelectedRows().Select(p => p.ToString()).ToArray());
            });
        }

        [Fact]
        public void An_empty_selection_reports_an_empty_array_rather_than_null()
        {
            // gtk_tree_selection_get_selected_rows returns a NULL GList for an
            // empty selection, and the hand-written wrapper has a guard for it:
            // without one, GLib.List over IntPtr.Zero is what every caller of
            // this property would meet first.
            Run(() =>
            {
                var view = ViewOf("a", "b");

                Assert.Empty(view.Selection.GetSelectedRows());
                Assert.Equal(0, view.Selection.CountSelectedRows());
                Assert.False(view.Selection.GetSelected(out _));
            });
        }

        [Fact]
        public void A_selection_function_can_refuse_a_row()
        {
            // TreeSelectionFunc is consulted before the selection changes and a
            // false return vetoes it -- so a veto is silent, and the row simply
            // does not light up.
            Run(() =>
            {
                var view = ViewOf("open", "locked", "open too");
                var selection = view.Selection;

                var asked = new List<string>();
                selection.SelectFunction = (sel, model, path, currentlySelected) =>
                {
                    asked.Add(path.ToString());
                    model.GetIter(out var iter, path);
                    return (string) model.GetValue(iter, 0) != "locked";
                };

                selection.SelectPath(new TreePath("1"));
                Assert.Equal(0, selection.CountSelectedRows());

                selection.SelectPath(new TreePath("2"));
                Assert.True(selection.PathIsSelected(new TreePath("2")));

                Assert.Equal(new[] { "1", "2" }, asked);
            });
        }

        [Fact]
        public void SelectedForeach_visits_every_selected_row_in_order()
        {
            Run(() =>
            {
                var view = ViewOf("a", "b", "c", "d");
                var selection = view.Selection;
                selection.Mode = SelectionMode.Multiple;

                selection.SelectPath(new TreePath("3"));
                selection.SelectPath(new TreePath("0"));

                var visited = new List<string>();
                selection.SelectedForeach((model, path, iter) =>
                    visited.Add(path + "=" + model.GetValue(iter, 0)));

                Assert.Equal(new[] { "0=a", "3=d" }, visited);
            });
        }

        [Fact]
        public void Select_all_needs_multiple_mode_and_then_takes_every_row()
        {
            // gtk_tree_selection_select_all is a no-op in SINGLE or BROWSE mode
            // -- it warns and returns -- so calling it on a default selection
            // does nothing at all, which reads as a broken widget.
            Run(() =>
            {
                var view = ViewOf("a", "b", "c");
                var selection = view.Selection;
                selection.Mode = SelectionMode.Multiple;

                selection.SelectAll();
                Assert.Equal(3, selection.CountSelectedRows());

                selection.UnselectPath(new TreePath("1"));
                Assert.Equal(new[] { "0", "2" },
                             selection.GetSelectedRows().Select(p => p.ToString()).ToArray());

                selection.UnselectAll();
                Assert.Equal(0, selection.CountSelectedRows());
            });
        }

        // --------------------------------------------------- TreeRowReference

        [Fact]
        public void A_row_reference_follows_its_row_where_a_path_does_not()
        {
            // This is the whole reason TreeRowReference exists: a TreePath is a
            // position and goes stale the moment anything is inserted above it,
            // while a reference is kept in step by the model's own signals.
            Run(() =>
            {
                var store = NamesAndCities(("a", "1"), ("b", "2"), ("c", "3"));
                var path = new TreePath("2");
                var reference = new TreeRowReference(store, path);

                store.InsertWithValues(0, "new", "0");

                Assert.True(reference.Valid());
                Assert.Equal("3", reference.Path.ToString());

                store.GetIter(out var byReference, reference.Path);
                Assert.Equal("c", store.GetValue(byReference, 0));

                // The path itself never moved, and now names a different row.
                Assert.Equal("2", path.ToString());
                store.GetIter(out var byPath, path);
                Assert.Equal("b", store.GetValue(byPath, 0));

                Assert.Same(store, reference.Model);
            });
        }

        [Fact]
        public void A_row_reference_to_a_deleted_row_reports_itself_invalid()
        {
            // An invalidated reference answers null for its path rather than
            // throwing, so code that reads Path without checking Valid gets a
            // null it did not ask about.
            Run(() =>
            {
                var store = NamesAndCities(("a", "1"), ("b", "2"));
                var reference = new TreeRowReference(store, new TreePath("1"));

                Assert.True(reference.Valid());

                store.IterNthChild(out var second, 1);
                store.Remove(ref second);

                Assert.False(reference.Valid());
                Assert.Null(reference.Path);
            });
        }

        [Fact]
        public void A_copied_row_reference_tracks_the_same_row_independently()
        {
            Run(() =>
            {
                var store = NamesAndCities(("a", "1"), ("b", "2"));
                var original = new TreeRowReference(store, new TreePath("1"));
                var copy = original.Copy();

                store.InsertWithValues(0, "new", "0");

                Assert.Equal("2", original.Path.ToString());
                Assert.Equal("2", copy.Path.ToString());

                copy.Dispose();

                // Disposing the copy must not disturb the original: they are two
                // handles onto one row, not one shared object.
                Assert.True(original.Valid());
                Assert.Equal("2", original.Path.ToString());
            });
        }

        // ------------------------------------------------- TreeView: expansion

        private static TreeView TreeOfDepth(int depth)
        {
            var store = new TreeStore(typeof(string));

            TreeIter parent = TreeIter.Zero;
            for (int i = 0; i < depth; i++)
                parent = i == 0
                    ? store.AppendValues("level 0")
                    : store.AppendValues(parent, "level " + i);

            return new TreeView(store);
        }

        [Fact]
        public void Expanding_a_row_reports_it_open_and_raises_the_signal_with_its_path()
        {
            Run(() =>
            {
                var view = TreeOfDepth(2);
                var root = new TreePath("0");

                string expanded = null, collapsed = null;
                view.RowExpanded += (o, args) => expanded = args.Path.ToString();
                view.RowCollapsed += (o, args) => collapsed = args.Path.ToString();

                Assert.False(view.GetRowExpanded(root));

                Assert.True(view.ExpandRow(root, false));
                Assert.True(view.GetRowExpanded(root));
                Assert.Equal("0", expanded);

                Assert.True(view.CollapseRow(root));
                Assert.False(view.GetRowExpanded(root));
                Assert.Equal("0", collapsed);
            });
        }

        [Fact]
        public void ExpandToPath_opens_the_row_as_well_as_its_ancestors()
        {
            // "Expand to" reads like "open everything above it and stop", which
            // is what would make the row itself visible. gtk_tree_view_expand_to_path
            // walks 1..depth inclusive, so the row named is opened too and its
            // children are on screen -- one more level than the caller asked for.
            Run(() =>
            {
                var view = TreeOfDepth(5);

                view.ExpandToPath(new TreePath("0:0:0"));

                Assert.True(view.GetRowExpanded(new TreePath("0")));
                Assert.True(view.GetRowExpanded(new TreePath("0:0")));
                Assert.True(view.GetRowExpanded(new TreePath("0:0:0")));

                // Its child, which has children of its own, is still shut.
                Assert.False(view.GetRowExpanded(new TreePath("0:0:0:0")));
            });
        }

        [Fact]
        public void MapExpandedRows_visits_exactly_the_rows_that_are_open()
        {
            Run(() =>
            {
                var view = TreeOfDepth(4);

                view.ExpandAll();

                var open = new List<string>();
                view.MapExpandedRows((v, path) => open.Add(path.ToString()));

                // The leaf has no children, so it is not expandable.
                Assert.Equal(new[] { "0", "0:0", "0:0:0" }, open);

                view.CollapseAll();

                open.Clear();
                view.MapExpandedRows((v, path) => open.Add(path.ToString()));
                Assert.Empty(open);
            });
        }

        // ------------------------------------------- TreeView: cursor and rows

        [Fact]
        public void Setting_the_cursor_reports_it_back_and_raises_CursorChanged()
        {
            Run(() =>
            {
                var view = ViewOf("a", "b", "c");
                var column = view.GetColumn(0);

                var changes = 0;
                view.CursorChanged += (o, args) => changes++;

                view.SetCursor(new TreePath("2"), column, false);

                Assert.True(changes > 0, "moving the cursor should raise CursorChanged");

                view.GetCursor(out var path, out var focusColumn);
                Assert.Equal("2", path.ToString());
                Assert.Same(column, focusColumn);

                // Moving the cursor also selects the row it lands on.
                Assert.True(view.Selection.PathIsSelected(new TreePath("2")));
            });
        }

        [Fact]
        public void An_activated_rows_path_outlives_the_emission_that_carried_it()
        {
            // The path is deliberately kept past the end of the handler, which
            // is what an application does when it remembers the activated row.
            //
            // GtkTreeView::row-activated declares its GtkTreePath *without*
            // G_SIGNAL_TYPE_STATIC_SCOPE, so g_signal_emit copies the path into
            // the emission's GValue and g_value_unset frees that copy when the
            // emission ends -- the pointer the handler was handed is not the one
            // the caller passed in, and it is dead as soon as the signal is
            // over. GLib.Value used to wrap it without copying, which made this
            // an access violation out of TreePath.ToString: a dead test host
            // rather than a failing test, landing wherever the allocator
            // happened to hand the block out again.
            //
            // The churn below is what makes a regression fail rather than pass
            // by luck: it hands the freed block to somebody else before the
            // captured path is read.
            Run(() =>
            {
                var view = ViewOf("a", "b");
                var column = view.GetColumn(0);

                TreePath activatedPath = null;
                TreeViewColumn activatedColumn = null;
                IntPtr handleInHandler = IntPtr.Zero;
                view.RowActivated += (o, args) =>
                {
                    activatedPath = args.Path;
                    activatedColumn = args.Column;
                    handleInHandler = args.Path.Handle;
                };

                var passed = new TreePath("1");
                view.ActivateRow(passed, column);

                Assert.NotEqual(passed.Handle, handleInHandler);   // Gtk copied it

                var churn = new List<TreePath>();
                for (int i = 0; i < 256; i++)
                    churn.Add(new TreePath("7:7:7:7"));

                Assert.Equal("1", activatedPath?.ToString());
                Assert.Same(column, activatedColumn);

                GC.KeepAlive(churn);
                GC.KeepAlive(passed);
            });
        }

        // ----------------------------------------------- a CellRenderer subclass

        /// <summary>
        /// A managed <c>CellRenderer</c>. Overriding the measurement vfuncs is
        /// what used to fail at class-init, because the binding still described
        /// <c>gtk_cell_renderer_get_size</c>, which Gtk 4 removed.
        /// </summary>
        private sealed class FixedSizeRenderer : CellRenderer
        {
            public const int Width = 137;
            public const int Height = 41;

            public Widget MeasuredFor { get; private set; }
            public int SnapshotCalls { get; private set; }
            public Gdk.Rectangle LastCellArea { get; private set; }

            protected override void OnGetPreferredWidth(Widget widget, out int minimum, out int natural)
            {
                MeasuredFor = widget;
                minimum = Width;
                natural = Width;
            }

            protected override void OnGetPreferredHeight(Widget widget, out int minimum, out int natural)
            {
                minimum = Height;
                natural = Height;
            }

            protected override void OnGetPreferredHeightForWidth(Widget widget, int width,
                                                                 out int minimum, out int natural)
            {
                minimum = Height;
                natural = Height;
            }

            protected override void OnGetPreferredWidthForHeight(Widget widget, int height,
                                                                 out int minimum, out int natural)
            {
                minimum = Width;
                natural = Width;
            }

            protected override void OnSnapshot(Snapshot snapshot, Widget widget,
                                               Gdk.Rectangle backgroundArea, Gdk.Rectangle cellArea,
                                               CellRendererState flags)
            {
                SnapshotCalls++;
                LastCellArea = cellArea;
            }
        }

        [Fact]
        public void A_managed_cell_renderer_is_measured_through_its_own_vfunc()
        {
            // gtk_cell_renderer_get_size no longer exists in Gtk 4, and the
            // binding that still described it made every CellRenderer subclass
            // throw from class-init -- i.e. the first time the type was used at
            // all. This asks Gtk to measure the renderer, so the answer can only
            // have come back out through the class vtable.
            Run(() =>
            {
                var renderer = new FixedSizeRenderer();
                var area = new CellAreaBox();
                area.PackStart(renderer, true, true, false);

                var widget = new TreeView();
                area.RequestRenderer(renderer, Orientation.Horizontal, widget, -1,
                                     out var minimum, out var natural);

                Assert.Equal(FixedSizeRenderer.Width, minimum);
                Assert.Equal(FixedSizeRenderer.Width, natural);
                Assert.Same(widget, renderer.MeasuredFor);

                area.RequestRenderer(renderer, Orientation.Vertical, widget, -1,
                                     out var minHeight, out _);
                Assert.Equal(FixedSizeRenderer.Height, minHeight);
            });
        }

        [Fact]
        public void A_managed_cell_renderer_is_drawn_through_OnSnapshot()
        {
            // Gtk 4 replaced gtk_cell_renderer_render with a snapshot vfunc.
            // Nothing else in the suite reaches a managed renderer's draw path.
            Run(() =>
            {
                var renderer = new FixedSizeRenderer();
                var area = new CellAreaBox();
                area.PackStart(renderer, true, true, false);

                var widget = new TreeView();
                var context = area.CreateContext();

                var wide = new Gdk.Rectangle(0, 0, 200, 50);
                context.Allocate(wide.Width, wide.Height);
                using (var snapshot = new Snapshot())
                    area.Snapshot(context, widget, snapshot, wide, wide,
                                  CellRendererState.Selected, false);

                Assert.Equal(1, renderer.SnapshotCalls);
                Assert.Equal(wide.Height, renderer.LastCellArea.Height);
                var wideWidth = renderer.LastCellArea.Width;

                // The rectangle the renderer is handed follows the one the area
                // was drawn into, so narrowing by 100 narrows it by 100 -- which
                // holds whatever padding the theme adds on either side.
                var narrow = new Gdk.Rectangle(0, 0, 100, 50);
                context.Allocate(narrow.Width, narrow.Height);
                using (var snapshot = new Snapshot())
                    area.Snapshot(context, widget, snapshot, narrow, narrow,
                                  CellRendererState.Selected, false);

                Assert.Equal(2, renderer.SnapshotCalls);
                Assert.Equal(100, wideWidth - renderer.LastCellArea.Width);
            });
        }

        // ------------------------------------------------ CellArea / ICellLayout

        [Fact]
        public void A_cell_area_applies_every_renderers_attributes_at_once()
        {
            Run(() =>
            {
                var store = NamesAndCities(("Ada", "London"));
                var area = new CellAreaBox();

                var nameCell = new CellRendererText();
                var cityCell = new CellRendererText();
                area.PackStart(nameCell, true, true, false);
                area.PackEnd(cityCell, false, true, false);

                area.AddAttribute(nameCell, "text", 0);
                area.AddAttribute(cityCell, "text", 1);

                Assert.Equal(0, area.AttributeGetColumn(nameCell, "text"));
                Assert.Equal(1, area.AttributeGetColumn(cityCell, "text"));

                store.GetIterFirst(out var iter);
                area.ApplyAttributes(store, iter, false, false);

                Assert.Equal("Ada", nameCell.Text);
                Assert.Equal("London", cityCell.Text);

                // Disconnecting the attribute reports -1 and stops the write.
                area.AttributeDisconnect(cityCell, "text");
                Assert.Equal(-1, area.AttributeGetColumn(cityCell, "text"));

                cityCell.Text = "untouched";
                area.ApplyAttributes(store, iter, false, false);
                Assert.Equal("untouched", cityCell.Text);
            });
        }

        [Fact]
        public void A_cell_area_reports_its_renderers_in_the_order_they_were_added()
        {
            // Not in the order they are laid out: a renderer packed at the end
            // still comes back where it was added, so the sequence Foreach and
            // Cells hand over is no guide to what is drawn where. Code that
            // treats the list as left-to-right puts the wrong cell first.
            Run(() =>
            {
                var area = new CellAreaBox();
                var start = new CellRendererText();
                var end = new CellRendererPixbuf();
                var alsoStart = new CellRendererToggle();

                area.PackStart(start, true, true, false);
                area.PackEnd(end, false, true, false);
                area.PackStart(alsoStart, false, true, false);

                Assert.True(area.HasRenderer(start));
                Assert.Equal(3, area.Cells.Length);

                var visited = new List<CellRenderer>();
                area.Foreach(cell => { visited.Add(cell); return false; });
                Assert.Equal(new CellRenderer[] { start, end, alsoStart }, visited);

                area.Remove(alsoStart);
                Assert.False(area.HasRenderer(alsoStart));
                Assert.Equal(new CellRenderer[] { start, end }, area.Cells);
            });
        }

        [Fact]
        public void A_cell_callback_that_returns_true_stops_the_walk()
        {
            // gtk_cell_area_foreach stops on TRUE, which is the reverse of the
            // "keep going" convention most callbacks in this stack use.
            Run(() =>
            {
                var area = new CellAreaBox();
                var first = new CellRendererText();
                area.PackStart(first, true, true, false);
                area.PackStart(new CellRendererText(), true, true, false);
                area.PackStart(new CellRendererText(), true, true, false);

                var visited = new List<CellRenderer>();
                area.Foreach(cell => { visited.Add(cell); return true; });

                Assert.Equal(new CellRenderer[] { first }, visited);
            });
        }

        [Fact]
        public void The_cell_layout_extension_method_reaches_every_implementor()
        {
            // SetAttributes used to be declared on ICellLayout itself, which left
            // every implementor owing an implementation; it is an extension
            // method now, so it has to work through the interface for a type
            // that never declared it -- CellView here.
            Run(() =>
            {
                var store = NamesAndCities(("Ada", "London"));
                var cell = new CellRendererText();

                var view = new CellView();
                ICellLayout layout = view;
                layout.PackStart(cell, true);
                layout.SetAttributes(cell, "text", 1);

                store.GetIterFirst(out var iter);
                view.Area.ApplyAttributes(store, iter, false, false);

                Assert.Equal("London", cell.Text);
                Assert.Equal(new CellRenderer[] { cell }, layout.Cells);
            });
        }

        // ------------------------------------------------------ cell renderers

        [Fact]
        public void Activating_a_toggle_renderer_emits_toggled_without_flipping_it()
        {
            // GtkCellRendererToggle does not change its own "active" property --
            // it reports the path and leaves the model to the application. A
            // handler that assumes the renderer already flipped writes the value
            // it started with back into the store.
            Run(() =>
            {
                var view = ViewOf("a", "b");
                var renderer = new CellRendererToggle { Activatable = true, Active = false };

                string toggledPath = null;
                renderer.Toggled += (o, args) => toggledPath = args.Path;

                var area = new Gdk.Rectangle(0, 0, 20, 20);
                Assert.True(renderer.Activate(null, view, "1", area, area, CellRendererState.Selected));

                Assert.Equal("1", toggledPath);
                Assert.False(renderer.Active);
            });
        }

        [Fact]
        public void A_toggle_renderer_that_is_not_activatable_reports_the_click_as_unhandled()
        {
            Run(() =>
            {
                var view = ViewOf("a");
                var renderer = new CellRendererToggle { Activatable = false };

                var toggles = 0;
                renderer.Toggled += (o, args) => toggles++;

                var area = new Gdk.Rectangle(0, 0, 20, 20);
                Assert.False(renderer.Activate(null, view, "0", area, area, default));

                Assert.Equal(0, toggles);
            });
        }

        [Fact]
        public void An_accel_renderer_shows_the_label_Gtk_makes_of_the_same_accelerator()
        {
            // The renderer's text is produced by gtk_accelerator_get_label, so
            // the oracle is Gtk's own answer for the same key and modifiers --
            // which keeps this independent of the locale the host is in.
            Run(() =>
            {
                const uint key = (uint) Gdk.Key.a;

                var renderer = new CellRendererAccel
                {
                    AccelKey = key,
                    AccelMods = Gdk.ModifierType.ControlMask
                };

                Assert.Equal(Accelerator.GetLabel(key, Gdk.ModifierType.ControlMask),
                             renderer.Text);

                renderer.AccelMods = Gdk.ModifierType.ShiftMask;
                Assert.Equal(Accelerator.GetLabel(key, Gdk.ModifierType.ShiftMask),
                             renderer.Text);

                // The two spellings must actually differ, or the assertions above
                // would hold for a renderer that ignored the modifiers entirely.
                Assert.NotEqual(Accelerator.GetLabel(key, Gdk.ModifierType.ControlMask),
                                Accelerator.GetLabel(key, Gdk.ModifierType.ShiftMask));
            });
        }

        [Fact]
        public void A_pixbuf_renderer_drops_the_icon_name_when_it_is_given_an_image()
        {
            // The image properties are one slot: setting any of them clears the
            // others, so a renderer configured with both shows whichever was
            // written last and says nothing about the one it discarded.
            //
            // GtkCellRendererPixbuf:pixbuf is write-only in Gtk 4 (the gir says
            // readable="0"), so the binding emits a setter and no getter and the
            // image has to be read back through :texture.
            Run(() =>
            {
                var renderer = new CellRendererPixbuf { IconName = "list-add" };
                Assert.Equal("list-add", renderer.IconName);
                Assert.Null(renderer.Texture);

                var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 2, 2);
                pixbuf.Fill(0xff0000ffu);
                renderer.Pixbuf = pixbuf;

                Assert.Null(renderer.IconName);
                Assert.NotNull(renderer.Texture);
                Assert.Equal(2, renderer.Texture.Width);
            });
        }

        // ---------------------------------------- TreeDragSource / TreeDragDest

        [Fact]
        public void A_row_dragged_out_of_a_store_and_dropped_back_in_moves_it()
        {
            // This is the entire reorder-by-drag path, driven by hand: the model
            // is asked for the row's drag content, the content is read back into
            // the model and path it names, the copy is inserted at the drop and
            // the original is deleted. Every step is a real interface method
            // that a GtkTreeView would otherwise be the only caller of.
            Run(() =>
            {
                var store = NamesAndCities(("a", "1"), ("b", "2"), ("c", "3"));
                var source = new TreePath("0");

                Assert.True(store.RowDraggable(source));

                var provider = store.DragDataGet(source);
                var types = provider.Formats.GetGtypes();
                Assert.NotEmpty(types);

                Assert.True(provider.GetValue(types[0], out var value));

                Assert.True(Tree.GetRowDragData(value, out var model, out var path));
                Assert.Same(store, model);
                Assert.Equal("0", path.ToString());

                var destination = new TreePath("3");
                Assert.True(store.RowDropPossible(destination, value));
                Assert.True(store.DragDataReceived(destination, value));
                Assert.Equal(new[] { "a", "b", "c", "a" }, ColumnZero(store));

                Assert.True(store.DragDataDelete(source));
                Assert.Equal(new[] { "b", "c", "a" }, ColumnZero(store));
            });
        }

        [Fact]
        public void A_tree_store_refuses_to_drop_a_row_inside_its_own_subtree()
        {
            // A row cannot become its own descendant, and the model is what says
            // so -- the view never gets far enough to find out.
            Run(() =>
            {
                var store = new TreeStore(typeof(string));
                var parent = store.AppendValues("parent");
                store.AppendValues(parent, "child");
                store.AppendValues("stranger");

                var provider = store.DragDataGet(new TreePath("0"));
                var types = provider.Formats.GetGtypes();
                Assert.True(provider.GetValue(types[0], out var value));

                // Into the parent's own children: refused.
                Assert.False(store.RowDropPossible(new TreePath("0:1"), value));

                // Beside the stranger: allowed.
                Assert.True(store.RowDropPossible(new TreePath("2"), value));
            });
        }
    }
}
