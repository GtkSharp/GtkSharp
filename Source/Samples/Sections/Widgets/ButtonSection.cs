// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;

namespace Samples
{
    [Section(Name = "Button", Category = Category.Widgets)]
    class ButtonSection : ListSection
    {
        public ButtonSection()
        {
            AddItem(CreateSimpleButton());
            AddItem(CreateStockButton());
            AddItem(CreateImageButton());
            AddItem(CreateImageTextButton());
            AddItem(CreateActionButton());
            AddItem(CreateToggleButton());
            AddItem(CreateLinkButton());
            AddItem(CreateSpinButton());
            AddItem(CreateSwitchButton());
            AddItem(CreateColorButton());
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

        public (string, Widget) CreateToggleButton()
        {
            var btn = new ToggleButton("Toggle Me");
            btn.Toggled += (sender, e) => ApplicationOutput.WriteLine(sender, "Buton Toggled");

            return ("Toggle button:", btn);
        }

        public (string, Widget) CreateLinkButton()
        {
            var btn = new LinkButton("A simple link button");
            btn.Clicked += (sender, e) => ApplicationOutput.WriteLine(sender, "Link button Clicked");

            return ("Link button:", btn);
        }

        public (string, Widget) CreateSpinButton()
        {
            // Spinbutton constructor takes MinValue, MaxValue and StepValue 
            var btn = new SpinButton(0, 1000, 1);

            // Button constructor also takes the adjustment object
            // and it can be redefined any time like CurrentVal, MinVal, MaxVal, Step, PageStep, PageSize
            btn.Adjustment.Configure(888, 0, 1000, 1, 100, 0); 

            // Default values are double, use ValueAsInt method to get Int
            btn.ValueChanged += (sender, e) => 
                ApplicationOutput.WriteLine(sender, $"Spin button changed: {btn.ValueAsInt}");

            return ("Spin button:", btn);
        }

        public (string, Widget) CreateSwitchButton()
        {
            var btn = new Switch();

            btn.ButtonReleaseEvent += (o, args) => 
                ApplicationOutput.WriteLine(o, $"Switch is now: {!btn.Active}");

            return ("Switch:", btn);
        }

        public (string, Widget) CreateColorButton()
        {
            var btn = new ColorButton();

            // Set RGBA color
            btn.Rgba = new Gdk.RGBA()
            {
                Red = 0,
                Green = 0,
                Blue = 255,
                Alpha = 0.2 // 20% translucent
            };

            // Or Parse hex
            btn.Rgba.Parse("#729FCF");

            // UseAlpha default is false
            btn.UseAlpha = true;

            return ("Color button:", btn);
        }

    }
}