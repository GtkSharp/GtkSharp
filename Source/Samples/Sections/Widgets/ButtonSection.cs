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
            AddItem(CreateMnemonicButton());
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

        public (string, Widget) CreateMnemonicButton()
        {
            // Gtk 4 removed the stock item registry. What stock buttons mostly
            // provided -- a translated label with a mnemonic -- is now written
            // directly, with the underscore marking the mnemonic character.
            var btn = Button.NewWithMnemonic("_About");
            btn.Clicked += (sender, e) => ApplicationOutput.WriteLine(sender, "Clicked");

            return ("Mnemonic button:", btn);
        }

        public (string, Widget) CreateImageButton()
        {
            // A Gtk 4 button takes an icon by name. Gtk 3 needed a child Image
            // widget plus AlwaysShowImage to defeat the theme's gtk-button-images
            // setting, which no longer exists.
            var btn = new Button();
            btn.IconName = "document-new-symbolic";
            btn.Clicked += (sender, e) => ApplicationOutput.WriteLine(sender, "Clicked");

            return ("Image button:", btn);
        }

        public (string, Widget) CreateImageTextButton()
        {
            // ImagePosition is gone with the rest of the image handling: a
            // button holding both an icon and a label is now built by giving it
            // a box as its child, which also makes the arrangement arbitrary
            // rather than one of four positions.
            var content = new Box(Orientation.Vertical, 4);
            content.Append(Image.NewFromIconName("document-new-symbolic"));
            content.Append(new Label("Some text"));

            var btn = new Button();
            btn.Child = content;
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