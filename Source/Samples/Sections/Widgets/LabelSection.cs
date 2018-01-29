// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(Label), Category = Category.Widgets)]
    class LabelSection : ListSection
    {
        public LabelSection()
        {
            AddItem(CreateSimpleLabel());
            AddItem(CreateMarkupLabel());
        }

        public (string, Widget) CreateSimpleLabel()
        {
            var label = new Label();

            // can be defined at constructor
            label.LabelProp = "This is a label";

            // right align text, center is default
            label.Xalign = 1f; 

            return ("Label :", label);
        }

        public (string, Widget) CreateMarkupLabel()
        {
            var label = new Label();

            // activate markup, default is false
            label.UseMarkup = true;

            // define label with pango markup
            label.LabelProp = "This is a <span foreground=\"red\" size=\"large\">label</span> with <b>custom</b> markup";

            // right align text, center is default
            label.Xalign = 1f;

            return ("Label Markup:", label);
        }

    }
}
