using Gtk;

namespace GettingStarted.Pages
{
    /// <summary>
    /// "Styling with CSS". The Gtk 3 theming API is gone -- ModifyBg, Gtk.Style,
    /// style properties, all of it -- and providers are registered per
    /// GdkDisplay, because GdkScreen no longer exists.
    /// </summary>
    public class CssPage : TourPage
    {
        private const string Css = @"
            .tour-danger  { color: white; background: #c01c28; padding: 6px; border-radius: 6px; }
            .tour-calm    { color: white; background: #26a269; padding: 6px; border-radius: 6px; }
            .tour-frame   { border: 2px dashed #3584e4; }
            button.tour-danger:hover { opacity: 0.8; }
        ";

        private readonly Label _sample = new Label("I take my colours from CSS") { Xalign = 0 };

        public CssPage() : base(
            "CSS styling",
            "AddCssClass / RemoveCssClass / HasCssClass replace the Gtk 3 style-context juggling, "
          + "and a provider is added for a display rather than a screen.")
        {
            var provider = new CssProvider();
            provider.ParsingError += (o, args) =>
                Report("CSS parsing error -- the provider reports it rather than throwing.");
            provider.LoadFromString(Css);

            // Gtk 3 added providers to a GdkScreen. Screens are gone; a display
            // is the thing that has a style now.
            StyleContext.AddProviderForDisplay(Gdk.Display.Default, provider,
                                               StyleProviderPriority.Application);

            Append(Group("Toggling a class on a widget", ClassDemo()));
            Append(Group("The element name a widget type uses as a selector", CssNameDemo()));
        }

        private Widget ClassDemo()
        {
            var column = Column();
            column.Append(_sample);

            var row = Row();

            var danger = new ToggleButton { Label = "tour-danger" };
            danger.Toggled += (o, e) => Apply(danger.Active, "tour-danger");

            var calm = new ToggleButton { Label = "tour-calm" };
            calm.Toggled += (o, e) => Apply(calm.Active, "tour-calm");

            row.Append(danger);
            row.Append(calm);
            column.Append(row);

            var output = Output();
            var query = new Button { Label = "HasCssClass?", Halign = Align.Start };
            query.Clicked += (o, e) =>
                output.Text = $"tour-danger: {_sample.HasCssClass("tour-danger")}, "
                            + $"tour-calm: {_sample.HasCssClass("tour-calm")}";

            column.Append(query);
            column.Append(output);
            return column;
        }

        private void Apply(bool add, string cssClass)
        {
            if (add)
                _sample.AddCssClass(cssClass);
            else
                _sample.RemoveCssClass(cssClass);

            Report($"{(add ? "Added" : "Removed")} .{cssClass}");
        }

        private Widget CssNameDemo()
        {
            var column = Column();
            var output = Output();

            var button = new Button { Label = "Ask three widget types for their CSS name", Halign = Align.Start };
            button.Clicked += (o, e) =>
            {
                // The element name a selector matches, which is not the C# type
                // name and is worth checking before guessing at a stylesheet.
                output.Text = $"Button -> {Widget.GetCssName(Button.GType)}\n"
                            + $"Label  -> {Widget.GetCssName(Label.GType)}\n"
                            + $"Window -> {Widget.GetCssName(Window.GType)}";
                Report("Widget.GetCssName gives the selector, not the type name.");
            };

            column.Append(button);
            column.Append(output);
            return column;
        }
    }
}
