using System;
using Gtk;

namespace GettingStarted.Pages
{
    /// <summary>
    /// "Building UI from .ui files". The window this page lives in was itself
    /// built by GtkBuilder; this page shows the other half -- building from a
    /// string, which is what tests and generated UI do -- and demonstrates what
    /// happens to a document that declares signals.
    /// </summary>
    public class BuilderPage : TourPage
    {
        private const string Markup =
            "<interface>" +
            "  <object class='GtkBox' id='root'>" +
            "    <property name='orientation'>horizontal</property>" +
            "    <property name='spacing'>8</property>" +
            "    <child><object class='GtkLabel' id='hello'>" +
            "      <property name='label'>built from a string</property>" +
            "    </object></child>" +
            "    <child><object class='GtkButton' id='press'>" +
            "      <property name='label'>and wired in C#</property>" +
            "    </object></child>" +
            "  </object>" +
            "</interface>";

        // The same markup with a <signal> element, which is the part Gtk 4
        // moved to GtkBuilderScope and GtkSharp does not bind yet.
        private const string MarkupWithSignal =
            "<interface>" +
            "  <object class='GtkButton' id='press'>" +
            "    <property name='label'>never gets this far</property>" +
            "    <signal name='clicked' handler='OnPressed'/>" +
            "  </object>" +
            "</interface>";

        public BuilderPage() : base(
            "Builder and .ui files",
            "This whole window was loaded from MainWindow.ui and bound with [UI] fields. "
          + "Builder also takes markup as a string, which is how tests build UI.")
        {
            Append(Group("Builder.AddFromString, then GetObject", FromStringDemo()));
            Append(Group("A document that declares <signal> fails loudly", SignalDemo()));
            Append(Note(
                "Autoconnect binds [UI] fields by name -- the field name IS the id unless "
              + "[UI(\"other-id\")] says otherwise. Connect handlers in C#: gtk_builder_connect_"
              + "signals_full was replaced by GtkBuilderScope, which this binding does not "
              + "implement, so a document declaring signals throws NotSupportedException naming "
              + "the cause rather than silently ignoring every click. It throws from the Add"
              + "FromString/AddFromFile call, not from Autoconnect -- Gtk 4 resolves the handler "
              + "while parsing, so the document never loads. Catch it there, and catch it inside "
              + "the signal handler that triggered it: an exception that escapes a handler is not "
              + "reported, it exits the process."));
        }

        private Widget FromStringDemo()
        {
            var column = Column();
            var output = Output();

            var builder = new Builder();
            builder.AddFromString(Markup);

            var root = (Box) builder.GetObject("root");
            var label = (Label) builder.GetObject("hello");
            var button = (Button) builder.GetObject("press");

            var presses = 0;
            button.Clicked += (o, e) =>
            {
                label.Text = $"pressed {++presses}";
                output.Text = $"GetObject(\"hello\").Text is now \"{label.Text}\"";
                Report("Widgets from Builder are ordinary widgets.");
            };

            column.Append(root);
            column.Append(output);
            return column;
        }

        private Widget SignalDemo()
        {
            var column = Column();
            var output = Output();

            var button = new Button { Label = "Load markup containing <signal>", Halign = Align.Start };
            button.Clicked += (o, e) =>
            {
                // AddFromString is inside the try, not Autoconnect: the throw
                // happens while *parsing*, because Gtk 4 resolves a <signal>
                // handler through GtkBuilderScope at parse time and the document
                // fails to load at all. Autoconnect is never reached.
                //
                // Catching it here is not decoration. This runs in a Clicked
                // handler, so an escaping exception unwinds into native Gtk,
                // where SignalClosure hands it to GLib.ExceptionManager -- which,
                // with nothing subscribed to UnhandledException, calls
                // Environment.Exit(1). An uncaught exception in a signal handler
                // does not surface as a stack trace; it ends the application.
                try
                {
                    var builder = new Builder();
                    builder.AddFromString(MarkupWithSignal);
                    builder.Autoconnect(this);
                    output.Text = "no exception -- unexpected";
                }
                catch (Exception ex)
                {
                    output.Text = $"{ex.GetType().Name}: {ex.Message}";
                    Report("A .ui file declaring <signal> throws, by design.");
                }
            };

            column.Append(button);
            column.Append(output);
            return column;
        }
    }
}
