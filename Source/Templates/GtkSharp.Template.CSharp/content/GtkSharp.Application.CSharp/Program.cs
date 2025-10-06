using Gtk;

namespace GtkNamespace;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.Init();

        var app = new Application("org.GtkNamespace.GtkNamespace", GLib.ApplicationFlags.None);
        app.Register(GLib.Cancellable.Current);

        var win = new MainWindow();
        app.AddWindow(win);

        win.Show();
        Application.Run();
    }
}
