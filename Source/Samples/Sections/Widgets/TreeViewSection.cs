// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using System;
using Gtk;

namespace Samples
{
    // The Gtk 3 version of this sample used GtkTreeView over a GtkTreeStore,
    // both deprecated in Gtk 4.10. The replacement changes the shape rather than
    // the names: rows are GObjects in a GListModel, columns are factories that
    // build real widgets, and selection is a model rather than a helper object.
    //
    // What is demonstrated is the same: add, edit and remove rows, and watch the
    // model report every change.
    [Section(ContentType = typeof(ColumnView), Category = Category.Widgets)]
    public class TreeViewSection : Box
    {
        private readonly GLib.ListStore _model;
        private readonly SingleSelection _selection;
        private readonly Entry _entry;
        private readonly Gdk.Texture _icon;
        private int _nextIndex = 1;

        public TreeViewSection() : base(Orientation.Vertical, 3)
        {
            _icon = new Gdk.Texture(new Gdk.Pixbuf(typeof(ImageSection).Assembly, "Testpic", 32, 32));

            _model = new GLib.ListStore((GLib.GType) typeof(Item));
            _selection = new SingleSelection(_model);

            // GListModel reports every insertion, removal and replacement
            // through one signal, where GtkTreeModel had five.
            _model.ItemsChanged += (o, args) =>
                ApplicationOutput.WriteLine(this,
                    $"ItemsChanged: position {args.Position}, removed {args.Removed}, added {args.Added}");

            for (int i = 0; i < 3; i++)
                Add($"Item {_nextIndex}");

            var view = new ColumnView(_selection);
            view.AppendColumn(IconColumn());
            view.AppendColumn(TextColumn("Index", item => item.Index.ToString()));
            view.AppendColumn(TextColumn("Name", item => item.Name));

            _entry = new Entry { Hexpand = true, PlaceholderText = "Name for add / edit" };

            var buttons = new Box(Orientation.Horizontal, 3);
            buttons.Append(_entry);
            buttons.Append(ActionButton("Add", OnAdd));
            buttons.Append(ActionButton("Edit", OnEdit));
            buttons.Append(ActionButton("Remove", OnRemove));

            Append(buttons);
            Append(new ScrolledWindow { Child = view, Vexpand = true });
        }

        public class Item : GLib.Object
        {
            public Item() : base() { }
            public Item(IntPtr raw) : base(raw) { }

            public int Index { get; set; }
            public string Name { get; set; }
        }

        private Button ActionButton(string label, Action action)
        {
            var button = new Button { Label = label };
            button.Clicked += (o, e) => action();
            return button;
        }

        private void Add(string name)
        {
            _model.Append(new Item { Index = _nextIndex++, Name = name }.Handle);
        }

        private void OnAdd()
        {
            Add(string.IsNullOrWhiteSpace(_entry.Text) ? $"Item {_nextIndex}" : _entry.Text);
        }

        private void OnEdit()
        {
            var selected = Selected();
            if (selected == null)
                return;

            selected.Name = string.IsNullOrWhiteSpace(_entry.Text) ? selected.Name : _entry.Text;

            // The model does not watch its items, so a change to one has to be
            // announced. Re-inserting is the plain way to say "this row differs".
            uint position = _selection.Selected;
            _model.Remove(position);
            _model.Insert(position, selected.Handle);
            _selection.Selected = position;
        }

        private void OnRemove()
        {
            if (Selected() == null)
                return;

            _model.Remove(_selection.Selected);
        }

        private Item Selected()
        {
            // GTK_INVALID_LIST_POSITION is uint.MaxValue.
            if (_selection.Selected == uint.MaxValue)
                return null;

            return GLib.Object.GetObject(_selection.SelectedItem) as Item;
        }

        private ColumnViewColumn IconColumn()
        {
            var factory = new SignalListItemFactory();

            factory.Setup += (o, args) => {
                var listItem = (ListItem) args.Object;
                listItem.Child = new Image((Gdk.IPaintable) _icon) { PixelSize = 32 };
            };

            return new ColumnViewColumn("Icon", factory);
        }

        private ColumnViewColumn TextColumn(string title, Func<Item, string> text)
        {
            var factory = new SignalListItemFactory();

            factory.Setup += (o, args) => {
                var listItem = (ListItem) args.Object;
                listItem.Child = new Label { Halign = Align.Start };
            };

            factory.Bind += (o, args) => {
                var listItem = (ListItem) args.Object;
                var item = (Item) GLib.Object.GetObject(listItem.Item);
                ((Label) listItem.Child).Text = text(item);
            };

            return new ColumnViewColumn(title, factory) { Expand = true };
        }
    }
}
