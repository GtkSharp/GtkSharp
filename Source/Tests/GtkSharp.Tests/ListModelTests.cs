using System;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The Gtk 4 list stack: a GListModel of GObjects, a selection model over
    /// it, and a factory that builds the row widgets.
    /// </summary>
    /// <remarks>
    /// This replaces TreeModel/TreeStore/CellRenderer, which Gtk 4.10
    /// deprecated. Row data has to be a real GObject, so these also pin that a
    /// managed GLib.Object subclass can be created, stored and read back --
    /// everything the ported samples do rests on that.
    /// </remarks>
    public class ListModelTests : GtkTestBase
    {
        public ListModelTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>A row: a managed subclass of GLib.Object.</summary>
        public class Row : GLib.Object
        {
            public Row() : base() { }
            public Row(IntPtr raw) : base(raw) { }

            public string Name { get; set; }
            public int Size { get; set; }
        }

        [Fact]
        public void A_managed_GObject_subclass_can_be_constructed()
        {
            Run(() =>
            {
                var row = new Row { Name = "hello", Size = 3 };

                Assert.NotEqual(IntPtr.Zero, row.Handle);
                Assert.Equal("hello", row.Name);
            });
        }

        [Fact]
        public void ListStore_returns_the_same_object_that_was_appended()
        {
            Run(() =>
            {
                var store = new GLib.ListStore((GLib.GType) typeof(Row));
                var row = new Row { Name = "first", Size = 1 };

                store.Append(row.Handle);

                Assert.Equal(1u, store.NItems);

                var read = store.GetObject(0) as Row;
                Assert.NotNull(read);
                Assert.Equal("first", read.Name);
            });
        }

        [Fact]
        public void SingleSelection_tracks_the_selected_row()
        {
            Run(() =>
            {
                var store = new GLib.ListStore((GLib.GType) typeof(Row));
                store.Append(new Row { Name = "a" }.Handle);
                store.Append(new Row { Name = "b" }.Handle);

                var selection = new SingleSelection(store);

                selection.Selected = 1;

                Assert.Equal(1u, selection.Selected);
                // SelectedItem is a gpointer in GIR, so it arrives as IntPtr.
                var selected = GLib.Object.GetObject(selection.SelectedItem) as Row;
                Assert.Equal("b", selected?.Name);
            });
        }

        [Fact]
        public void ColumnView_keeps_the_columns_it_is_given()
        {
            Run(() =>
            {
                var store = new GLib.ListStore((GLib.GType) typeof(Row));
                var view = new ColumnView(new SingleSelection(store));

                var column = new ColumnViewColumn("Name", new SignalListItemFactory());
                view.AppendColumn(column);

                Assert.Equal(1u, view.Columns.NItems);
                Assert.Equal("Name", column.Title);
            });
        }
    }
}
