// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(Name = "ComboBox", Category = Category.Widgets)]
    class ComboBoxSection : ListSection
    {
        public ComboBoxSection()
        {
            AddItem(CreateSimpleCombo());
            AddItem(CreateEntryCombo());
        }

        public (string, Widget) CreateSimpleCombo()
        {
            var combo = new ComboBox(new string[] {
                "First Value",
                "Second Value",
                "Third Value"
            });

            combo.Changed += (sender, e) => 
                ApplicationOutput.WriteLine(sender, $"Combo selection index is now {combo.Active}");

            return ("ComboBox:", combo);
        }

        public (string, Widget) CreateEntryCombo()
        {
            var combo = ComboBox.NewWithEntry();

            combo.Clear();

            // create values store
            var store = new ListStore(typeof(string));
            store.AppendValues("First Value");
            store.AppendValues("Second Value");
            store.AppendValues("Third Value");

            // create cell
            var cell = new CellRendererText();

            // add cell to combo
            combo.PackStart(cell, true);

            // add text attribute to combo
            combo.AddAttribute(cell, "text", 0);

            // assign store to combo
            combo.Model = store;

            return ("ComboBox with Entry:", combo);
        }
    }
}
