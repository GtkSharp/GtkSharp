
// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(LinkButton), Category = Category.Widgets)]
    class LinkButtonSection : ListSection
    {
        public LinkButtonSection()
        {
            AddItem(CreateLinkButton());
        }

        public (string, Widget) CreateLinkButton()
        {
            var btn = new LinkButton("A simple link button");
            btn.Clicked += (sender, e) => ApplicationOutput.WriteLine(sender, "Link button Clicked");

            return ("Link button:", btn);
        }
    }
}
