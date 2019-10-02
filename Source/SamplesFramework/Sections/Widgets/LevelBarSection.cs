// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(LevelBar), Category = Category.Widgets)]
    class LevelBarSection : ListSection
    {
        public LevelBarSection()
        {
            AddItem(CreateSimpleLevelBar());
        }

        public (string, Widget) CreateSimpleLevelBar()
        {
            // constructor takes MinValue, MaxValue
            var lb = new LevelBar(0, 100);

            // lets add a visible request size in our example
            lb.WidthRequest = 100;

            // set the value to 75%
            lb.Value = 75d;

            return ("Level Bar:", lb);
        }

    }
}
