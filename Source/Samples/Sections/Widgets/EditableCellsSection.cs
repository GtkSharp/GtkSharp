using System;
using Gtk;

namespace Samples.Sections.Widgets
{
    // Editable cells were awkward under Gtk 3: a CellRendererText had to be told
    // it was editable, and the edit arrived as an "edited" signal carrying a
    // path string that had to be resolved back to a row.
    //
    // Gtk 4 puts real widgets in the rows, so a cell that can be edited is just
    // an Entry or a SpinButton, and it writes straight back to the row object.
    // That is the whole of the difference, and it is why this file is half the
    // length of the version it replaces.
    [Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    public class EditableCellsSection : Box
    {
        private readonly GLib.ListStore _model;
        private readonly SingleSelection _selection;

        public EditableCellsSection() : base(Orientation.Vertical, 3)
        {
            _model = new GLib.ListStore((GLib.GType) typeof(Article));
            foreach (var article in Initial)
                _model.Append(article.Handle);

            _selection = new SingleSelection(_model);

            var view = new ColumnView(_selection);
            view.AppendColumn(NumberColumn());
            view.AppendColumn(ProductColumn());

            var scroller = new ScrolledWindow { HasFrame = true, Vexpand = true, Child = view };
            scroller.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            Append(scroller);

            var buttons = new Box(Orientation.Horizontal, 4) { Homogeneous = true };

            var add = new Button("Add item");
            add.Clicked += (o, e) => _model.Append(new Article { Number = 1, Product = "New article" }.Handle);
            add.Hexpand = true;
            buttons.Append(add);

            var remove = new Button("Remove item");
            remove.Clicked += (o, e) => {
                if (_selection.Selected != uint.MaxValue)
                    _model.Remove(_selection.Selected);
            };
            remove.Hexpand = true;
            buttons.Append(remove);

            Append(buttons);
        }

        public class Article : GLib.Object
        {
            public Article() : base() { }
            public Article(IntPtr raw) : base(raw) { }

            public int Number { get; set; }
            public string Product { get; set; }
        }

        /// <summary>An editable number, as a spin button living in the row.</summary>
        private ColumnViewColumn NumberColumn()
        {
            var factory = new SignalListItemFactory();

            factory.Setup += (o, args) => {
                var listItem = (ListItem) args.Object;
                listItem.Child = new SpinButton(new Adjustment(1, 0, 1000, 1, 10, 0), 1, 0);
            };

            factory.Bind += (o, args) => {
                var listItem = (ListItem) args.Object;
                var article = (Article) GLib.Object.GetObject(listItem.Item);
                var spin = (SpinButton) listItem.Child;

                spin.Value = article.Number;
                spin.ValueChanged += (s, e) => article.Number = (int) spin.Value;
            };

            return new ColumnViewColumn("Number", factory);
        }

        /// <summary>An editable product name, as an entry living in the row.</summary>
        private ColumnViewColumn ProductColumn()
        {
            var factory = new SignalListItemFactory();

            factory.Setup += (o, args) => {
                var listItem = (ListItem) args.Object;
                listItem.Child = new Entry { Hexpand = true };
            };

            factory.Bind += (o, args) => {
                var listItem = (ListItem) args.Object;
                var article = (Article) GLib.Object.GetObject(listItem.Item);
                var entry = (Entry) listItem.Child;

                entry.Text = article.Product ?? string.Empty;
                entry.Changed += (s, e) => article.Product = entry.Text;
            };

            return new ColumnViewColumn("Product", factory) { Expand = true };
        }

        private static readonly Article[] Initial = {
            new Article { Number = 3, Product = "bottles of coke" },
            new Article { Number = 5, Product = "packages of noodles" },
            new Article { Number = 2, Product = "packages of chocolate chip cookies" },
            new Article { Number = 1, Product = "can vanilla ice cream" },
            new Article { Number = 6, Product = "eggs" },
        };
    }
}
