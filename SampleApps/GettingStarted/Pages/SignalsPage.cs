using Gtk;

namespace GettingStarted.Pages
{
    /// <summary>
    /// "Signals, and one trap worth knowing first". A lambda cannot carry
    /// [GLib.ConnectBefore], because the attribute is read off the delegate's
    /// MethodInfo -- so a lambda always runs AFTER the default handler, which
    /// for some signals is the thing that performs the operation.
    /// </summary>
    public class SignalsPage : TourPage
    {
        private readonly TextBuffer _buffer = new TextBuffer(new TextTagTable());
        private readonly Label _afterSaw = Output();
        private readonly Label _beforeSaw = Output();

        public SignalsPage() : base(
            "Signals",
            "Signals are C# events. The order they run in is the part that surprises people: "
          + "+= connects after the widget's own default handler unless the handler is a named "
          + "method carrying [GLib.ConnectBefore].")
        {
            Append(Group("An ordinary handler", ClickDemo()));
            Append(Group("The trap: what each handler sees when text is inserted", OrderDemo()));

            // A lambda: connected after GtkTextBuffer's default handler, which
            // is what actually inserts the text. By the time this runs, the
            // buffer already contains it.
            _buffer.InsertText += (o, args) =>
                _afterSaw.Text = $"lambda (after):  buffer = \"{BufferText()}\"";

            // A named method with [GLib.ConnectBefore]: runs first, so the
            // buffer is still in its pre-insertion state.
            _buffer.InsertText += OnInsertTextBefore;
        }

        [GLib.ConnectBefore]
        private void OnInsertTextBefore(object o, InsertTextArgs args)
            => _beforeSaw.Text = $"[ConnectBefore]: buffer = \"{BufferText()}\", inserting \"{args.Text}\"";

        private string BufferText()
        {
            _buffer.GetBounds(out var start, out var end);
            return _buffer.GetText(start, end, false);
        }

        private Widget ClickDemo()
        {
            var column = Column();
            var output = Output("not clicked yet");
            var clicks = 0;

            var button = new Button { Label = "Click me", Halign = Align.Start };
            button.Clicked += (o, e) =>
            {
                output.Text = $"Clicked {++clicks} time(s)";
                Report($"Button.Clicked fired ({clicks}).");
            };

            column.Append(button);
            column.Append(output);
            return column;
        }

        private Widget OrderDemo()
        {
            var column = Column();

            column.Append(Note(
                "Both handlers are connected to the same signal on the same buffer. Press the "
              + "button and read what each one saw: the lambda is looking at the world after "
              + "the insertion it was supposed to inspect."));

            var button = new Button { Label = "Insert \"world\" at the end", Halign = Align.Start };
            button.Clicked += (o, e) =>
            {
                _buffer.GetBounds(out _, out var end);
                _buffer.Insert(ref end, "world ");
                Report("InsertText: compare what the two handlers saw.");
            };

            column.Append(button);
            column.Append(_beforeSaw);
            column.Append(_afterSaw);
            column.Append(Note(
                "The same applies to DeleteRange, where a lambda reads the empty string out of "
              + "the range it was handed -- the iterators have already collapsed onto the "
              + "deletion point. Nothing errors; you simply get the wrong half of the transaction."));

            return column;
        }
    }
}
