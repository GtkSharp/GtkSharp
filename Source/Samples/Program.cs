// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using System;
using Gtk;

namespace Samples
{
    // Public so the test project can bootstrap the application the same way.
    public class Program
    {
        public static Application App;
        public static Window Win;

        /// <summary>
        /// Creates the GApplication the sections rely on -- several of them add
        /// actions to it. Shared with the test project so the sections are
        /// exercised against the same setup the real application gives them.
        /// </summary>
        public static Application EnsureApplication()
        {
            if (App == null)
            {
                App = new Application("org.Samples.Samples", GLib.ApplicationFlags.None);

                // Registration needs a session bus, and there is not always one
                // -- notably on Windows. The result used to be discarded, which
                // matters because an unregistered GApplication silently refuses
                // AddWindow: the window still works, but the application never
                // tracks it. Saying so beats wondering why Windows is empty.
                if (!App.Register(GLib.Cancellable.Current))
                    Console.Error.WriteLine(
                        "warning: could not register the application (no session bus?); " +
                        "application-level window tracking and actions will not work.");
            }

            return App;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Application.Init();

            EnsureApplication();

            Win = new MainWindow();
            App.AddWindow(Win);

            var menu = new GLib.Menu();
            menu.AppendItem(new GLib.MenuItem("Help", "app.help"));
            menu.AppendItem(new GLib.MenuItem("About", "app.about"));
            menu.AppendItem(new GLib.MenuItem("Quit", "app.quit"));
            // Gtk 4 removed the app menu (gtk_application_set_app_menu); the
            // desktop shell no longer shows one. A menubar is the closest
            // remaining application-level menu.
            App.Menubar = menu;

            var helpAction = new GLib.SimpleAction("help", null);
            helpAction.Activated += HelpActivated;
            App.AddAction(helpAction);

            var aboutAction = new GLib.SimpleAction("about", null);
            aboutAction.Activated += AboutActivated;
            App.AddAction(aboutAction);

            var quitAction = new GLib.SimpleAction("quit", null);
            quitAction.Activated += QuitActivated;
            App.AddAction(quitAction);

            // Gtk 4 has no ShowAll: widgets are visible by default, so a
            // window only needs to be presented.
            Win.Present();
            Application.Run();
        }

        private static void HelpActivated(object sender, System.EventArgs e)
        {

        }

        private static void AboutActivated(object sender, System.EventArgs e)
        {
            var dialog = new AboutDialog
            {
                TransientFor = Win,
                ProgramName = "GtkSharp Sample Application",
                Version = "1.0.0.0",
                Comments = "A sample application for the GtkSharp project.",
                LogoIconName = "system-run-symbolic",
                License = "This sample application is licensed under public domain.",
                Website = "https://www.github.com/GtkSharp/GtkSharp",
                WebsiteLabel = "GtkSharp Website"
            };
            // Gtk 4 removed gtk_dialog_run, which spun a nested main loop.
            // GtkAboutDialog is no longer a GtkDialog either -- it derives
            // straight from GtkWindow -- so it has no response to wait for:
            // it is presented, and the user closes it.
            dialog.Present();
        }

        private static void QuitActivated(object sender, System.EventArgs e)
        {
            Application.Quit();
        }
    }
}
