using System;
using Gtk;

namespace GettingStarted.Pages
{
    /// <summary>
    /// "Lists". The Gtk 4 stack is a pipeline of GListModels: each stage is
    /// itself a model, so filters and sorters compose, and the view builds row
    /// widgets through a factory rather than through cell renderers.
    /// </summary>
    public class ListsPage : TourPage
    {
        /// <summary>A row is a real GObject -- a managed GLib.Object subclass.</summary>
        public class Person : GLib.Object
        {
            public Person() : base() { }
            public Person(IntPtr raw) : base(raw) { }

            public string Name { get; set; }
            public int Age { get; set; }
        }

        public ListsPage() : base(
            "Lists",
            "TreeView still exists and is still deprecated. New code stacks GListModels: "
          + "source, sort, filter, selection, view.")
        {
            Append(Group("StringList -> SortListModel -> FilterListModel -> ListView", PipelineDemo()));
            Append(Group("A managed GObject as a row, in a ColumnView", ColumnViewDemo()));
        }

        private Widget PipelineDemo()
        {
            var source = new StringList(new[] { "delta", "alpha", "charlie", "bravo", "echo" });

            // Each stage wraps the previous one and is itself a GListModel.
            var sorter = new StringSorter(new PropertyExpression(StringObject.GType, null, "string"));
            var sorted = new SortListModel(source, sorter);

            var filter = new CustomFilter(item =>
                ((StringObject) GLib.Object.GetObject(item)).String.Length == 5);
            var filtered = new FilterListModel(sorted, filter);

            var selection = new SingleSelection(filtered);

            // The factory creates a row widget once and then binds many items to
            // it, because rows are recycled as the view scrolls.
            var factory = new SignalListItemFactory();
            factory.Setup += (o, args) =>
            {
                var item = (ListItem) args.Object;
                item.Child = new Label("") { Xalign = 0 };
            };
            factory.Bind += (o, args) =>
            {
                var item = (ListItem) args.Object;
                var text = ((StringObject) GLib.Object.GetObject(item.Item)).String;
                ((Label) item.Child).Text = text;
            };

            var view = new ListView(selection, factory);
            var scroller = new ScrolledWindow { Child = view, HeightRequest = 140 };

            var column = Column();
            column.Append(Note(
                "Source order is delta, alpha, charlie, bravo, echo. The sorter puts them in "
              + "alphabetical order and the filter keeps only the five-letter ones, so "
              + "\"charlie\" disappears and the rest arrive sorted."));
            column.Append(scroller);

            var output = Output();
            var button = new Button { Label = "Report what each stage holds", Halign = Align.Start };
            button.Clicked += (o, e) =>
            {
                output.Text = $"source {source.NItems} -> sorted {sorted.NItems} -> "
                            + $"filtered {filtered.NItems}, selected index {selection.Selected}";
                Report("Every stage of the pipeline is a model in its own right.");
            };
            column.Append(button);
            column.Append(output);
            return column;
        }

        private Widget ColumnViewDemo()
        {
            var store = new GLib.ListStore((GLib.GType) typeof(Person));
            store.Append(new Person { Name = "Ada", Age = 36 }.Handle);
            store.Append(new Person { Name = "Grace", Age = 45 }.Handle);
            store.Append(new Person { Name = "Alan", Age = 41 }.Handle);

            var selection = new SingleSelection(store);
            var view = new ColumnView(selection);
            view.AppendColumn(new ColumnViewColumn("Name", TextColumn(p => p.Name)) { Expand = true });
            view.AppendColumn(new ColumnViewColumn("Age", TextColumn(p => p.Age.ToString())));

            var column = Column();
            column.Append(new ScrolledWindow { Child = view, HeightRequest = 140 });

            var output = Output();
            var button = new Button { Label = "Read the selected row", Halign = Align.Start };
            button.Clicked += (o, e) =>
            {
                // SelectedItem is a gpointer in the GIR, so it arrives as an
                // IntPtr; GetObject turns it back into the managed wrapper.
                var person = GLib.Object.GetObject(selection.SelectedItem) as Person;
                output.Text = person == null
                    ? "nothing selected"
                    : $"index {selection.Selected}: {person.Name}, {person.Age}";
                Report("A managed GObject survived the round trip through the store.");
            };

            column.Append(button);
            column.Append(output);
            column.Append(Note(
                "Rows hold real widgets, so an editable cell is just an Entry -- there is no "
              + "CellRendererText.Edited dance."));
            return column;
        }

        private static SignalListItemFactory TextColumn(Func<Person, string> value)
        {
            var factory = new SignalListItemFactory();
            factory.Setup += (o, args) =>
                ((ListItem) args.Object).Child = new Label("") { Xalign = 0 };
            factory.Bind += (o, args) =>
            {
                var item = (ListItem) args.Object;
                var person = GLib.Object.GetObject(item.Item) as Person;
                ((Label) item.Child).Text = person == null ? "" : value(person);
            };
            return factory;
        }
    }
}
