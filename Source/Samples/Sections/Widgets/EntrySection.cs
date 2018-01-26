// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(Name= "Entry", Category = Category.Widgets)]
    class EntrySection : ListSection
    {
        public EntrySection()
        {
            AddItem(CreateSimpleEntry());
            AddItem(CreateSimpleRightAlignedTextEntry());
            AddItem(CreateMaxLimitEntry());
            AddItem(CreatePlaceholderEntry());
            AddItem(CreateInvisibleCharEntry());
            AddItem(CreateCustomActionsEntry());
            AddItem(CreateProgressEntry());
            AddItem(CreateCompletionEntry());
        }

        public (string, Widget) CreateSimpleEntry()
        {
            var entry = new Entry("Initial Text");
            entry.TooltipText = "This is the tooltip!";

            entry.Changed += (sender, e) => ApplicationOutput.WriteLine(sender, "Changed");

            return ("Simple entry:", entry);
        }

        public (string, Widget) CreateSimpleRightAlignedTextEntry()
        {
            var entry = new Entry("Text is Right Aligned");
            entry.Xalign = 1f;

            entry.Changed += (sender, e) => ApplicationOutput.WriteLine(sender, "Changed");

            return ("Right aligned text entry:", entry);
        }

        public (string, Widget) CreateMaxLimitEntry()
        {
            var entry = new Entry("123");
            entry.MaxLength = 3;

            entry.Changed += (sender, e) => ApplicationOutput.WriteLine(sender, "Changed");

            return ("Text length limited entry:", entry);
        }

        public (string, Widget) CreatePlaceholderEntry()
        {
            var entry = new Entry();
            entry.PlaceholderText = "Please fill with information";

            entry.Changed += (sender, e) => ApplicationOutput.WriteLine(sender, "Changed");

            return ("Placeholder text entry:", entry);
        }

        public (string, Widget) CreateInvisibleCharEntry()
        {
            var entry = new Entry("Invisible text entry");
            entry.Visibility = false;
            entry.InvisibleChar = '\u2022';
            entry.InvisibleCharSet = true;

            entry.Changed += (sender, e) => ApplicationOutput.WriteLine(sender, "Changed");

            return ("Invisible text entry:", entry);
        }

        public (string, Widget) CreateCustomActionsEntry()
        {
            var entry = new Entry();
            entry.PlaceholderText = "Search";
            entry.SetIconFromIconName(EntryIconPosition.Primary, "edit-find-symbolic");
            entry.SetIconFromIconName(EntryIconPosition.Secondary, "edit-clear-symbolic");

            entry.IconRelease += (o, args) =>
            {
                switch (args.P0)
                {
                    case EntryIconPosition.Primary:
                        ApplicationOutput.WriteLine(o, "Clicked Search Icon");
                        break;

                    default:
                        entry.Text = string.Empty;
                        ApplicationOutput.WriteLine(o, "Clicked Clear Icon");
                        break;
                }
            };

            return ("Custom actions entry:", entry);
        }

        public (string, Widget) CreateProgressEntry()
        {
            var entry = new Entry();
            entry.PlaceholderText = "Progress Entry";
            entry.ProgressFraction = 0f;
            entry.MaxLength = 20;

            entry.Changed += (sender, e) => entry.ProgressFraction = (float)entry.Text.Length / entry.MaxLength;

            return ("Progress entry:", entry);
        }

        public (string, Widget) CreateCompletionEntry()
        {
            // create completion object and assign it to entry
            var completion = new EntryCompletion();
            var entry = new Entry();
            entry.Completion = completion;

            // create values store
            var store = new ListStore(typeof(string));
            store.AppendValues("An example to search for");
            store.AppendValues("Better example");
            store.AppendValues("Better and bigger example");
            store.AppendValues("Some other example");

            // create tree column
            var column = new TreeViewColumn();
            column.Reorderable = false;
            column.SortIndicator = false;
            column.Clickable = true;
            column.SortColumnId = 0;

            // create cells
            var cell = new CellRendererText();
            column.PackStart(cell, true);

            // set cell functions
            column.SetCellDataFunc(cell, (TreeViewColumn tree_column, CellRenderer cellRenderer, ITreeModel tree_model, TreeIter iter) =>
            {
                var m = tree_model.GetValue(iter, 0);
                (cellRenderer as CellRendererText).Text = (string)m;
            });
                                          
            // model filter
            var filter = new TreeModelFilter(store, null);
            filter.VisibleFunc = (model, iter) =>
            {
                if (string.IsNullOrEmpty(entry.Text))
                    return true;

                var o = model.GetValue(iter, 0);
                var searchString = o as string;
                if (!string.IsNullOrEmpty(searchString))
                    return searchString.StartsWith(entry.Text, System.StringComparison.InvariantCultureIgnoreCase);

                return true;
            };

            // assign treemodel as the completion
            completion.Model = filter;
            completion.TextColumn = 0;

            return ("Completion Entry:",entry);
        }
    }
}
