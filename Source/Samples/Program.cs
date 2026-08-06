// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

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
            if (Array.IndexOf(args, "--smoke-exit") >= 0)
            {
                Environment.Exit(SmokeRun());
                return;
            }

            Application.Init();

            App = new Application("org.Samples.Samples", GLib.ApplicationFlags.None);
            App.Register(GLib.Cancellable.Current);

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

        /// <summary>
        /// Non-interactive run for CI. This repository has no test project, so
        /// this is the only end-to-end verification available.
        /// </summary>
        /// <remarks>
        /// It asserts a real oracle rather than just failing on an unhandled
        /// exception: every type carrying [Section] must construct and produce a
        /// widget, and the number that do must equal the number declared. A
        /// section that throws, or that quietly returns nothing, fails the run
        /// and is named. Exercising the sections is what makes this worth
        /// running -- they are where the bindings actually get used.
        /// </remarks>
        private static int SmokeRun()
        {
            Application.Init();

            App = new Application("org.Samples.Samples", GLib.ApplicationFlags.None);
            App.Register(GLib.Cancellable.Current);

            Win = new MainWindow();
            App.AddWindow(Win);
            Win.Present();

            var declared = new System.Collections.Generic.List<Type>();
            foreach (var type in typeof(SectionAttribute).Assembly.GetTypes())
                foreach (var attribute in type.GetCustomAttributes(true))
                    if (attribute is SectionAttribute)
                        declared.Add(type);

            if (declared.Count == 0)
            {
                Console.Error.WriteLine("smoke: no [Section] types found; the sample app is empty");
                return 1;
            }

            int built = 0;
            var failures = new System.Collections.Generic.List<string>();

            foreach (var type in declared)
            {
                try
                {
                    if (Activator.CreateInstance(type) is Widget widget && widget.Handle != IntPtr.Zero)
                        built++;
                    else
                        failures.Add(type.Name + ": produced no widget");
                }
                catch (Exception e)
                {
                    failures.Add(type.Name + ": " + e.GetType().Name + ": " + e.Message);
                }
            }

            // Let anything the sections queued actually run, so a crash in
            // layout or a draw function is attributed here rather than escaping.
            for (int i = 0; i < 100 && Application.EventsPending(); i++)
                Application.RunIteration(false);

            Console.WriteLine($"smoke: {built}/{declared.Count} sections constructed");
            foreach (var failure in failures)
                Console.Error.WriteLine("smoke: FAILED " + failure);

            return built == declared.Count ? 0 : 1;
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
