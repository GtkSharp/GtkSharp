// A tour of Docs/getting-started.md, as a running application.
//
// Every page under Pages/ corresponds to one section of that document, and
// demonstrates it by doing it rather than by describing it -- including the
// traps, which are the parts that compile cleanly and behave unexpectedly.

using System;
using Gtk;

namespace GettingStarted
{
    public static class Program
    {
        public static Application App;
        public static MainWindow Win;

        [STAThread]
        public static void Main(string[] args)
        {
            // Gtk 4's gtk_init takes no arguments: it no longer parses the
            // command line, so there is nothing to pass and nothing to get back.
            Application.Init();

            App = new Application("org.gtksharp.GettingStarted", GLib.ApplicationFlags.None);

            // Registration needs a session bus, and there is not always one --
            // notably on Windows. An unregistered GApplication still runs, it
            // just silently declines to track windows or dispatch app.* actions,
            // so saying so beats wondering why the menu does nothing.
            if (!App.Register(GLib.Cancellable.Current))
                Console.Error.WriteLine(
                    "warning: could not register the application (no session bus?); " +
                    "app-level actions and window tracking will not work.");

            AddApplicationActions();

            Win = new MainWindow();
            App.AddWindow(Win);

            // No ShowAll: widgets are visible by default in Gtk 4, so a window
            // only has to be presented.
            Win.Present();

            // gtk_main was deleted in Gtk 4. GtkSharp's Application.Run is a
            // GLib.MainLoop instead.
            Application.Run();
        }

        /// <summary>
        /// GAction replaced GtkAction: an action is a named callback, and the
        /// widgets that trigger it refer to it by name rather than holding a
        /// reference to it.
        /// </summary>
        private static void AddApplicationActions()
        {
            var about = new GLib.SimpleAction("about", null);
            about.Activated += (o, e) => ShowAbout();
            App.AddAction(about);

            var quit = new GLib.SimpleAction("quit", null);
            quit.Activated += (o, e) => Application.Quit();
            App.AddAction(quit);
        }

        private static void ShowAbout()
        {
            var dialog = new AboutDialog
            {
                TransientFor = Win,
                Modal = true,
                ProgramName = "GtkSharp Getting Started Tour",
                Version = "4.22",
                Comments = "Every page here is one section of Docs/getting-started.md, "
                         + "running rather than described.",
                Website = "https://github.com/GtkSharp/GtkSharp",
                WebsiteLabel = "GtkSharp/GtkSharp",
                LogoIconName = "help-about-symbolic",
                License = "Public domain."
            };

            // gtk_dialog_run is gone -- Gtk 4 has no nested main loops -- and
            // GtkAboutDialog is not even a GtkDialog any more: it derives
            // straight from GtkWindow, so there is no response to wait for. It
            // is presented, and the user closes it.
            dialog.Present();
        }
    }
}
