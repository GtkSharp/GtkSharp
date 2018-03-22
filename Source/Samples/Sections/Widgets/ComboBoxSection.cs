// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(ComboBox), Category = Category.Widgets)]
    class ComboBoxSection : ListSection
    {
        public ComboBoxSection()
        {
            AddItem(CreateSimpleComboBox());
            AddItem(CreateMultiColumnComboBox());
            AddItem(CreateEditableComboBox());
        }

        public (string, Widget) CreateSimpleComboBox()
        {
            // initialize with a simple string list
            var combo = new ComboBox(
                new string[] 
                {
                    "Combo Entry 1",
                    "Combo Entry 2",
                    "Combo Entry 3",
                    "Combo Entry 4"
                }
            );

            // event to notify for index changes in our combo
            combo.Changed += (sender, e) => 
                ApplicationOutput.WriteLine(sender, $"Index changed to:{((ComboBox)sender).Active}");

            // set the active selection to index 2 (Combo Entry 3)
            combo.Active = 2;

            return ("Simple combo:", combo);
        }

        public (string, Widget) CreateMultiColumnComboBox()
        {
            // create a store for our combo
            var store = new ListStore(typeof(string), typeof(string), typeof(bool));

            // lets append some stock icons, passing the icon names, and a simple text column
            store.AppendValues("dialog-warning", "Warning", true);
            store.AppendValues("process-stop", "Stop", false);
            store.AppendValues("document-new", "New", true);
            store.AppendValues("edit-clear", "Clear", true);

            // create cells
            var imageCell = new CellRendererPixbuf();
            var textCell = new CellRendererText();

            // create the combo and pass the values in
            var combo = new ComboBox(store);
            combo.PackStart(imageCell, true);
            combo.PackStart(textCell, true);

            // add combo attributes to show in columns
            combo.AddAttribute(imageCell, "icon-name", 0);
            combo.AddAttribute(textCell, "text", 1);

            // lets use the store bool values to control sensitive rows
            // Process-stop (store index one) should be disabled in this sample
            // For a ComboBox item to be disabled, all cell renderers for the item need to have
            // their sensitivity disabled
            combo.AddAttribute(imageCell, "sensitive", 2);
            combo.AddAttribute(textCell, "sensitive", 2);

            // listen to index changed on combo
            combo.Changed += (sender, e) =>
                ApplicationOutput.WriteLine(sender, $"Index changed to:{((ComboBox)sender).Active}");

            // lets preselect the first option
            combo.Active = 0;

            return ("Combo with Icons and Text:", combo);
        }

        public (string, Widget) CreateEditableComboBox()
        {
            var combo = ComboBoxText.NewWithEntry();
            combo.AppendText("Example 1");
            combo.AppendText("Example 2");
            combo.AppendText("Example 3");
            combo.AppendText("Example 4");

            // combos with entry have a real entry inside it
            // we can use it by
            combo.Entry.PlaceholderText = "Write something";

            return ("Combo with entry:", combo);
        }
    }
}
