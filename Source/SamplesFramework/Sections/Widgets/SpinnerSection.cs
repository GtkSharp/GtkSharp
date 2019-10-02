// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(Spinner), Category = Category.Widgets)]
    class SpinnerSection : ListSection
    {
        public SpinnerSection()
        {
            AddItem(CreateSimpleSpinner());
        }

        public (string, Widget) CreateSimpleSpinner()
        {
            var sp = new Spinner();

            sp.Start();

            // can be stopped with
            // Stop()

            return ("Simple Spinner:", sp);
        }
    }
}
