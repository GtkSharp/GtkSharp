// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(Button), Category = Category.Widgets)]
    class ButtonSection : ListSection
    {
        public ButtonSection()
        {
            AddItem(CreateSimpleButton());
            AddItem(CreateStockButton());
            AddItem(CreateImageButton());
            AddItem(CreateImageTextButton());
            AddItem(CreateActionButton());
        }

        public (string, Widget) CreateSimpleButton()
        {
            var btn = new Button("Simple Button");
            btn.Clicked += (sender, e) => ApplicationOutput.WriteLine(sender, "Clicked");

            return ("Simple button:", btn);
        }

        public (string, Widget) CreateStockButton()
        {
            var btn = new Button(Stock.About);
            btn.Clicked += (sender, e) => ApplicationOutput.WriteLine(sender, "Clicked");

            return ("Stock button:", btn);
        }

        public (string, Widget) CreateImageButton()
        {
            var btn = new Button();
            btn.AlwaysShowImage = true;
            btn.Image = Image.NewFromIconName("document-new-symbolic", IconSize.Button);
            btn.Clicked += (sender, e) => ApplicationOutput.WriteLine(sender, "Clicked");

            return ("Image button:", btn);
        }

        public (string, Widget) CreateImageTextButton()
        {
            var btn = new Button();
            btn.Label = "Some text";
            btn.ImagePosition = PositionType.Top;
            btn.AlwaysShowImage = true;
            btn.Image = Image.NewFromIconName("document-new-symbolic", IconSize.Button);
            btn.Clicked += (sender, e) => ApplicationOutput.WriteLine(sender, "Clicked");

            return ("Image and text button:", btn);
        }

        public (string, Widget) CreateActionButton()
        {
            var sa = new GLib.SimpleAction("SampleAction", null);
            sa.Activated += (sender, e) => ApplicationOutput.WriteLine(sender, "SampleAction Activated");
            Program.App.AddAction(sa);

            var btn = new Button();
            btn.Label = "SampleAction Button";
            btn.ActionName = "app.SampleAction";

            return ("Action button:", btn);
        }
    }
}