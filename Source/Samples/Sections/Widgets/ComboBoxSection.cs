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
            AddItem(CreateSimpleCombo());
            AddItem(CreateEntryCombo());
        }

        public (string, Widget) CreateSimpleCombo()
        {
            // initialize with a new store of values
            var combo = new ComboBox(new string[] {
                "First Value",
                "Second Value",
                "Third Value"
            });

            // selects the first index to be displayed
            combo.Active = 0;

            // changed event
            combo.Changed += (sender, e) => 
                ApplicationOutput.WriteLine(sender, $"Combo selection index is now {combo.Active}");

            return ("ComboBox:", combo);
        }

        public (string, Widget) CreateEntryCombo()
        {
            var combo = ComboBox.NewWithEntry();

            // set column wich displays values
            combo.EntryTextColumn = 0;

            // create values store
            var store = new ListStore(typeof(string));
            store.AppendValues("First Value");
            store.AppendValues("Second Value");
            store.AppendValues("Third Value");

            // assign store to combo
            combo.Model = store;

            // selects the third choice from store to be displayed
            combo.Active = 2;

            // changed event
            combo.Changed += (sender, e) =>
                ApplicationOutput.WriteLine(sender, $"Combo selection index is now {combo.Active}");

            return ("ComboBox with Entry:", combo);
        }
    }
}
