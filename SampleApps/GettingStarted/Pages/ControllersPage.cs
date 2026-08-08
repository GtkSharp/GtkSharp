using Gtk;

namespace GettingStarted.Pages
{
    /// <summary>
    /// "Input: gestures and controllers". Widget event signals are gone, and so
    /// is GtkEventBox and the event-mask bookkeeping: a controller is attached
    /// to any widget and declares its own interest.
    /// </summary>
    public class ControllersPage : TourPage
    {
        public ControllersPage() : base(
            "Input controllers",
            "ButtonPressEvent, MotionNotifyEvent and KeyPressEvent no longer exist. Input "
          + "arrives through EventController objects you attach with AddController.")
        {
            Append(Group("GestureClick: press anywhere in the box", ClickDemo()));
            Append(Group("EventControllerMotion: Enter, Motion, Leave", MotionDemo()));
            Append(Group("EventControllerKey: type in the entry", KeyDemo()));
            Append(Note(
                "EventControllerFocus, EventControllerScroll, DropTarget and DragSource follow "
              + "the same shape. controller.Widget names what it is attached to, and "
              + "RemoveController detaches it -- after which Widget is null."));
        }

        private Widget ClickDemo()
        {
            var column = Column();
            var output = Output("no press yet");

            var target = new Frame { Child = new Label("press here") { HeightRequest = 60 } };

            var click = new GestureClick();
            click.Pressed += (o, args) =>
            {
                // NPress counts the click within a double/triple-click sequence,
                // which used to mean inspecting GdkEventType by hand.
                output.Text = $"press {args.NPress} at ({args.X:0.#}, {args.Y:0.#})";
                Report($"GestureClick: {args.NPress} press(es).");
            };
            click.Released += (o, args) => Report("GestureClick: released.");

            // The controller goes on the widget -- any widget. There is no
            // EventBox any more, because none is needed.
            target.AddController(click);

            column.Append(target);
            column.Append(output);
            column.Append(Note(
                "Writing this page found a defect here, since fixed. Gtk.PressedArgs was shared "
              + "between GtkGestureClick's pressed(n_press, x, y) and GtkGestureLongPress's "
              + "pressed(x, y); the long-press shape was the one generated, so a click's X was "
              + "really its n_press and nothing held the coordinates. GtkGestureLongPress's "
              + "signal is now LongPressed, and the audit that found it fixed fifteen more of "
              + "the same collision across four assemblies."));
            return column;
        }

        private Widget MotionDemo()
        {
            var column = Column();
            var output = Output("pointer is outside");

            var target = new Frame { Child = new Label("move over here") { HeightRequest = 60 } };

            var motion = new EventControllerMotion();
            motion.Enter += (o, args) => output.Text = "entered";
            motion.Leave += (o, args) => output.Text = "left";
            motion.Motion += (o, args) =>
                output.Text = $"motion at ({args.X:0.#}, {args.Y:0.#})";

            target.AddController(motion);

            column.Append(target);
            column.Append(output);
            column.Append(Note(
                "gdk_device_get_position is gone: a client cannot ask where the pointer is, "
              + "it only learns from a motion controller."));
            return column;
        }

        private Widget KeyDemo()
        {
            var column = Column();
            var output = Output("nothing typed yet");
            var entry = new Entry { PlaceholderText = "type here (Escape is reported specially)" };

            var keys = new EventControllerKey();
            keys.KeyPressed += (o, args) =>
            {
                var name = Gdk.Keyval.Name(args.Keyval) ?? args.Keyval.ToString();
                output.Text = args.Keyval == (uint) Gdk.Key.Escape
                    ? $"Escape (keyval {args.Keyval})"
                    : $"key \"{name}\", state {args.State}";

                // false lets the event continue to the entry itself; true would
                // consume it, and the character would never be typed.
                args.RetVal = false;
            };

            entry.AddController(keys);

            column.Append(entry);
            column.Append(output);
            return column;
        }
    }
}
