// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(CheckButton), Category = Category.Widgets)]
    class CheckButtonSection : ListSection
    {
        public CheckButtonSection()
        {
            AddItem(CreateCheckButton());
            AddItem(CreateImageCheckButton());
        }

        public (string, Widget) CreateCheckButton()
        {
            var checkButton = new CheckButton("A simple check button");

            checkButton.Clicked += (sender, e) => 
                ApplicationOutput.WriteLine(sender, $"Check is now {checkButton.Active}");

            return ("Check button:", checkButton);
        }

        public (string, Widget) CreateImageCheckButton()
        {
            var checkButton = new CheckButton();

            checkButton.Image = Image.NewFromIconName("document-print-symbolic", IconSize.LargeToolbar);
            checkButton.ImagePosition = PositionType.Right;
            checkButton.AlwaysShowImage = true;

            checkButton.Clicked += (sender, e) =>
                ApplicationOutput.WriteLine(sender, $"Check is now {checkButton.Active}");

            return ("Check button with image:", checkButton);
        }
    }
}
