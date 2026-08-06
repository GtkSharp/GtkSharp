using System;
using Gtk;

namespace Samples
{
    // Gtk 4.10 deprecated GtkTreeView, GtkListStore and the whole cell-renderer
    // stack. The replacement is a different shape rather than a renamed one:
    //
    //   GtkListStore    ->  any GListModel; here a GListStore of GObjects
    //   TreeModelColumn ->  ordinary properties on that GObject
    //   GtkCellRenderer ->  a factory that builds real widgets per row
    //   GtkTreeSelection -> a selection model wrapping the list model
    //
    // Rows must be GObjects, so the data type below derives from GLib.Object.
    [Section(ContentType = typeof(ColumnView), Category = Category.Widgets)]
    public class ListStoreSection : Box
    {
        private readonly GLib.ListStore _model;

        public ListStoreSection() : base(Orientation.Vertical, 3)
        {
            _model = new GLib.ListStore((GLib.GType) typeof(Bug));
            foreach (var bug in Bugs)
                _model.Append(bug.Handle);

            var view = new ColumnView(new SingleSelection(_model));
            view.AppendColumn(FixedColumn());
            view.AppendColumn(TextColumn("Number", bug => bug.Number.ToString()));
            view.AppendColumn(TextColumn("Severity", bug => bug.Severity));
            view.AppendColumn(TextColumn("Description", bug => bug.Description));

            var scroller = new ScrolledWindow { Child = view, Vexpand = true };
            Append(scroller);
        }

        /// <summary>A row. Gtk 4 list models hold GObjects, not struct tuples.</summary>
        public class Bug : GLib.Object
        {
            public Bug() : base() { }
            public Bug(IntPtr raw) : base(raw) { }

            public bool IsFixed { get; set; }
            public int Number { get; set; }
            public string Severity { get; set; }
            public string Description { get; set; }
        }

        /// <summary>
        /// A column of check buttons. Under Gtk 3 this was a
        /// CellRendererToggle; now the factory puts a real GtkCheckButton in
        /// each row, which is why it can be interacted with directly.
        /// </summary>
        private ColumnViewColumn FixedColumn()
        {
            var factory = new SignalListItemFactory();

            factory.Setup += (o, args) => {
                var item = (ListItem) args.Object;
                item.Child = new CheckButton();
            };

            factory.Bind += (o, args) => {
                var item = (ListItem) args.Object;
                var bug = (Bug) GLib.Object.GetObject(item.Item);
                var check = (CheckButton) item.Child;

                check.Active = bug.IsFixed;
                check.Toggled += (s, e) => bug.IsFixed = check.Active;
            };

            return new ColumnViewColumn("Fixed", factory);
        }

        private ColumnViewColumn TextColumn(string title, Func<Bug, string> text)
        {
            var factory = new SignalListItemFactory();

            factory.Setup += (o, args) => {
                var item = (ListItem) args.Object;
                item.Child = new Label { Halign = Align.Start };
            };

            factory.Bind += (o, args) => {
                var item = (ListItem) args.Object;
                var bug = (Bug) GLib.Object.GetObject(item.Item);
                ((Label) item.Child).Text = text(bug);
            };

            return new ColumnViewColumn(title, factory) { Expand = true };
        }

        private static readonly Bug[] Bugs = {
            new Bug { IsFixed = false, Number = 60482, Severity = "Normal",      Description = "scrollable notebooks and hidden tabs" },
            new Bug { IsFixed = false, Number = 60620, Severity = "Critical",    Description = "gdk_window_clear_area (gdkwindow-win32.c) is not thread-safe" },
            new Bug { IsFixed = false, Number = 50214, Severity = "Major",       Description = "Xft support does not clean up correctly" },
            new Bug { IsFixed = true,  Number = 52877, Severity = "Major",       Description = "GtkFileSelection needs a refresh method." },
            new Bug { IsFixed = false, Number = 56070, Severity = "Normal",      Description = "Can't click button after setting it insensitive" },
            new Bug { IsFixed = true,  Number = 56355, Severity = "Normal",      Description = "GtkLabel - Not all changes propagate correctly" },
            new Bug { IsFixed = false, Number = 50055, Severity = "Normal",      Description = "Rework width/height computations for TreeView" },
            new Bug { IsFixed = false, Number = 58278, Severity = "Normal",      Description = "gtk_dialog_set_response_sensitive () doesn't work" },
            new Bug { IsFixed = false, Number = 55767, Severity = "Normal",      Description = "Getters for all setters" },
            new Bug { IsFixed = false, Number = 56925, Severity = "Normal",      Description = "Gtkcalender size" },
            new Bug { IsFixed = false, Number = 56221, Severity = "Normal",      Description = "Selectable label needs right-click copy menu" },
            new Bug { IsFixed = true,  Number = 50939, Severity = "Normal",      Description = "Add shift clicking to GtkTextView" },
            new Bug { IsFixed = false, Number = 6112,  Severity = "Enhancement", Description = "netscape-like collapsable toolbars" },
            new Bug { IsFixed = false, Number = 1,     Severity = "Normal",      Description = "First bug :=)" },
        };
    }
}
