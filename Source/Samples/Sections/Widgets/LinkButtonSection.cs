
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
            // The single-argument constructor takes the URI, not the label --
            // gtk_link_button_new(uri) -- so passing a caption made Gtk refuse
            // to follow the link: "URI 'A simple link button' is not an
            // absolute URI".
            var btn = new LinkButton("https://github.com/pieroviano/GtkSharp", "A simple link button");
            btn.Clicked += (sender, e) => ApplicationOutput.WriteLine(sender, "Link button Clicked");

            return ("Link button:", btn);
        }
    }
}
