using Gtk;

namespace Samples
{
    [Section(Name= "Entry", Category = Category.Widgets)]
    class EntrySection : ListSection
    {
        public EntrySection()
        {
            AddItem(CreateSimpleEntry());
            AddItem(CreateSimpleCenterAlignedTextEntry());
            AddItem(CreateSimpleRightAlignedTextEntry());
            AddItem(CreateMaxLimitEntry());
            AddItem(CreateHolderTextEntry());
            AddItem(CreateInvisibleCharEntry());
        }

        public (string, Widget) CreateSimpleEntry()
        {
            var entry = new Entry("Initial Text");
            entry.TooltipText = "This is the tooltip!";

            entry.Changed += (sender, e) => ApplicationOutput.WriteLine(sender, "Changed");

            return ("Simple entry:", entry);
        }

        public (string, Widget) CreateSimpleCenterAlignedTextEntry()
        {
            var entry = new Entry("Center Aligned Text");
            entry.Xalign = 0.5f;

            entry.Changed += (sender, e) => ApplicationOutput.WriteLine(sender, "Changed");

            return ("Center aligned text entry:", entry);
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

        public (string, Widget) CreateHolderTextEntry()
        {
            var entry = new Entry();
            entry.PlaceholderText = "Please fill with information";

            entry.Changed += (sender, e) => ApplicationOutput.WriteLine(sender, "Changed");

            return ("Holder text entry:", entry);
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

    }
}
