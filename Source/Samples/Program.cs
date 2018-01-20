using System;
using Gtk;

namespace Samples
{
    class Program
    {
        public static Application App;

        public static Window Win;

        [STAThread]
        public static void Main(string[] args)
        {
            Application.Init();

            App = new Application("org.Samples.Samples", GLib.ApplicationFlags.None);
            App.Register(GLib.Cancellable.Current);

            Win = new MainWindow();
            App.AddWindow(Win);

            var menu = new GLib.Menu();
            menu.AppendItem(new GLib.MenuItem("Help", "app.help"));
            menu.AppendItem(new GLib.MenuItem("About", "app.about"));
            menu.AppendItem(new GLib.MenuItem("Quit", "app.quit"));
            App.AppMenu = menu;

            var helpAction = new GLib.SimpleAction("help", null);
            helpAction.Activated += HelpActivated;
            App.AddAction(helpAction);

            var aboutAction = new GLib.SimpleAction("about", null);
            aboutAction.Activated += AboutActivated;
            App.AddAction(aboutAction);

            var quitAction = new GLib.SimpleAction("quit", null);
            quitAction.Activated += QuitActivated;
            App.AddAction(quitAction);

            Win.ShowAll();
            Application.Run();
        }

        private static void HelpActivated(object sender, EventArgs e)
        {

        }

        private static void AboutActivated(object sender, EventArgs e)
        {
            var dialog = new AboutDialog();
            dialog.TransientFor = Win;
            dialog.ProgramName = "GtkSharp Sample Application";
            dialog.Version = "1.0.0.0";
            dialog.Comments = "A sample application for the GtkSharp project.";
            dialog.LogoIconName = "system-run-symbolic";
            dialog.License = "This sample application is licensed under public domain.";
            dialog.Website = "https://www.github.com/GtkSharp/GtkSharp";
            dialog.WebsiteLabel = "GtkSharp Website";

            dialog.Run();
            dialog.Hide();
        }

        private static void QuitActivated(object sender, EventArgs e)
        {
            Application.Quit();
        }
    }
}
