using System;
using Gtk;

namespace GettingStarted.Pages
{
    /// <summary>
    /// The traps table at the end of getting-started.md, run rather than read.
    /// Every one of these compiles cleanly, which is what makes them expensive.
    /// </summary>
    public class TrapsPage : TourPage
    {
        public TrapsPage() : base(
            "Traps",
            "Each of these compiles without a warning and does something other than what it "
          + "looks like. They are demonstrated here so the behaviour is a fact rather than a "
          + "claim.")
        {
            Append(Group("Widget.Activate() does not raise Clicked", ActivateDemo()));
            Append(Group("GLib.Bytes.Data on an empty Bytes is null, not empty", BytesDemo()));
            Append(Group("An int argument can select the raw-pointer constructor", IntPtrDemo()));
            Append(Group("CloseRequest: true VETOES the close", CloseDemo()));
        }

        private Widget ActivateDemo()
        {
            var column = Column();
            var output = Output();
            var clicks = 0;

            var button = new Button { Label = "(the button under test)", Halign = Align.Start };
            button.Clicked += (o, e) => clicks++;

            var run = new Button { Label = "Call Activate(), then click it yourself", Halign = Align.Start };
            run.Clicked += (o, e) =>
            {
                var before = clicks;
                button.Activate();

                output.Text = $"Activate() raised Clicked: {clicks > before} "
                            + $"(handler has run {clicks} time(s) in total)";
                Report("Gtk 4 routes a press through a gesture; Activate does not reach Clicked.");
            };

            column.Append(button);
            column.Append(run);
            column.Append(output);
            column.Append(Note(
                "This one cost a test suite its meaning: a theory that pressed every button in "
              + "the sample application passed on all 31 sections while pressing nothing."));
            return column;
        }

        private Widget BytesDemo()
        {
            var column = Column();
            var output = Output();

            var button = new Button { Label = "Read .Data off an empty and a non-empty Bytes", Halign = Align.Start };
            button.Clicked += (o, e) =>
            {
                var empty = new GLib.Bytes(new byte[0]);
                var full = new GLib.Bytes(new byte[] { 1, 2, 3 });

                output.Text = $"empty.Data is null: {empty.Data == null}\n"
                            + $"full.Data.Length:  {full.Data.Length}\n"
                            + "so `foreach (var b in bytes.Data)` throws on the empty one.";
                Report("An empty GLib.Bytes hands back null rather than an empty array.");
            };

            column.Append(button);
            column.Append(output);
            return column;
        }

        private Widget IntPtrDemo()
        {
            var column = Column();

            column.Append(Note(
                "GLib.Object subclasses carry a constructor taking a raw IntPtr handle, and C# "
              + "converts an int literal to IntPtr implicitly. So `new GLib.ValueArray(2)` and "
              + "`new GLib.Date(2)` do not pass a count -- they pass a pointer with the value 2, "
              + "and the object wraps address 0x2."));

            var output = Output();
            var button = new Button { Label = "Construct a ValueArray the safe way", Halign = Align.Start };
            button.Clicked += (o, e) =>
            {
                // 2u picks the (uint) overload -- the count -- instead of the
                // (IntPtr) one. The wrong call is described above rather than
                // executed, because it produces a wrapper around address 0x2
                // that takes the process down when it is collected.
                var array = new GLib.ValueArray(2u);
                array.Append(new GLib.Value(41));
                array.Append(new GLib.Value("forty-two"));

                output.Text = $"new GLib.ValueArray(2u) -> Count {array.Count}, "
                            + $"[0] = {((GLib.Value) array[0]).Val}, "
                            + $"[1] = {((GLib.Value) array[1]).Val}";
                Report("Use 2u or 2L when the parameter is a count, not a handle.");
            };

            column.Append(button);
            column.Append(output);
            return column;
        }

        private Widget CloseDemo()
        {
            var column = Column();

            var veto = new CheckButton { Label = "Veto the close (args.RetVal = true)" };

            var button = new Button { Label = "Open a window and try to close it", Halign = Align.Start };
            button.Clicked += (o, e) =>
            {
                var window = new Window
                {
                    Title = veto.Active ? "Try to close me — I will refuse" : "Close me",
                    DefaultWidth = 360,
                    DefaultHeight = 140,
                    TransientFor = Program.Win
                };

                var label = new Label(veto.Active
                    ? "RetVal = true, so the close request is refused.\nUntick the box to let it close."
                    : "RetVal = false, so the close proceeds.") { Wrap = true };
                window.Child = label;

                window.CloseRequest += (s, args) =>
                {
                    // DeleteEvent's convention was the opposite: true meant
                    // "handled, do not close". CloseRequest's true is the veto.
                    args.RetVal = veto.Active;
                    Report(veto.Active
                        ? "CloseRequest returned true: the close was vetoed."
                        : "CloseRequest returned false: the window closed.");
                };

                window.Present();
            };

            column.Append(veto);
            column.Append(button);
            return column;
        }
    }
}
