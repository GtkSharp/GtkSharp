using Gtk;

namespace GettingStarted.Pages
{
    /// <summary>
    /// "Actions and menus". GAction replaced GtkAction, GMenu replaced
    /// GtkUIManager, and the widget that triggers an action names it as a
    /// string instead of holding a reference to it.
    /// </summary>
    public class ActionsPage : TourPage
    {
        private readonly GLib.SimpleActionGroup _group = new GLib.SimpleActionGroup();
        private readonly Label _log = Output("nothing activated yet");

        public ActionsPage() : base(
            "Actions and menus",
            "An action is a named, optionally stateful, optionally parameterised callback. "
          + "Menus are a model rendered by a popover, not a tree of widgets.")
        {
            Append(Group("A plain action, reached by name", PlainAction()));
            Append(Group("A parameterised action", ParameterisedAction()));
            Append(Group("A stateful action, and the trap in StateChanged", StatefulAction()));
            Append(Group("GMenu rendered by a MenuButton", MenuDemo()));
            Append(_log);
        }

        private Widget PlainAction()
        {
            var save = new GLib.SimpleAction("save", null);
            save.Activated += (o, args) =>
            {
                _log.Text = "win.save activated";
                Report("win.save activated.");
            };
            _group.AddAction(save);

            var column = Column();
            // Insert the group under a prefix; everything else addresses the
            // action as "win.save" from here on.
            column.InsertActionGroup("win", _group);

            // The button holds a NAME, not a delegate.
            column.Append(new Button { Label = "ActionName = \"win.save\"", ActionName = "win.save", Halign = Align.Start });
            column.Append(Note("Disabling the action disables every widget pointing at it:"));

            var toggleEnabled = new ToggleButton { Label = "Disable win.save", Halign = Align.Start };
            toggleEnabled.Toggled += (o, e) =>
            {
                save.Enabled = !toggleEnabled.Active;
                _log.Text = $"win.save Enabled = {save.Enabled}";
            };
            column.Append(toggleEnabled);
            return column;
        }

        private Widget ParameterisedAction()
        {
            var open = new GLib.SimpleAction("open", GLib.VariantType.String);
            open.Activated += (o, args) =>
            {
                // The parameter arrives as a GVariant; the cast unpacks it.
                _log.Text = $"win.open received \"{(string) args.Parameter}\"";
                Report("A GVariant parameter reached the handler.");
            };
            _group.AddAction(open);

            var column = Column();
            var entry = new Entry { Text = "/tmp/file.txt" };
            var button = new Button { Label = "Activate with this string", Halign = Align.Start };
            button.Clicked += (o, e) => open.Activate(new GLib.Variant(entry.Text));

            column.Append(entry);
            column.Append(button);
            return column;
        }

        private Widget StatefulAction()
        {
            var column = Column();

            // Two identical actions, differing only in whether their handler
            // applies the state -- which is the whole of the trap.
            var broken = new GLib.SimpleAction("broken", null, new GLib.Variant(false));
            broken.StateChanged += (o, args) =>
            {
                // Reads the proposed value and stops. StateChanged IS the
                // change-state signal, and GLib's default handler -- the one
                // that applies the state -- has just been replaced by this.
                _log.Text = $"broken: proposed {(bool) args.Value}, State is still {(bool) broken.State}";
            };

            var correct = new GLib.SimpleAction("correct", null, new GLib.Variant(false));
            correct.StateChanged += (o, args) =>
            {
                correct.State = args.Value;      // applying it is the handler's job
                _log.Text = $"correct: State is now {(bool) correct.State}";
            };

            var brokenButton = new Button { Label = "ChangeState on the broken action", Halign = Align.Start };
            brokenButton.Clicked += (o, e) =>
            {
                broken.ChangeState(new GLib.Variant(!(bool) broken.State));
                Report("State did not move: the handler replaced the default one.");
            };

            var correctButton = new Button { Label = "ChangeState on the correct action", Halign = Align.Start };
            correctButton.Clicked += (o, e) =>
            {
                correct.ChangeState(new GLib.Variant(!(bool) correct.State));
                Report("State moved, because the handler set it.");
            };

            column.Append(Note(
                "StateChanged is named like an observer and behaves like a veto. A handler that "
              + "only reads the value leaves the action on its old state forever."));
            column.Append(brokenButton);
            column.Append(correctButton);
            return column;
        }

        private Widget MenuDemo()
        {
            var menu = new GLib.Menu();
            menu.Append("Save", "win.save");
            menu.Append("Open /tmp/file.txt", null);   // no action: shown insensitive

            var submenu = new GLib.Menu();
            submenu.Append("About", "app.about");
            submenu.Append("Quit", "app.quit");
            menu.AppendSubmenu("Application", submenu);

            var row = Row();
            row.Append(new MenuButton { Label = "Menu", MenuModel = menu });
            row.Append(Note("The model is data: the same GMenu can drive a MenuButton, a "
                          + "PopoverMenu or the application menubar."));
            return row;
        }
    }
}
