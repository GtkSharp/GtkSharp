namespace GtkNamespace
module Program =

    open Gtk

    [<EntryPoint>]
    let main argv =
        Application.Init()

        let app = new Application("org.GtkNamespace.GtkNamespace", GLib.ApplicationFlags.None)
        app.Register(GLib.Cancellable.Current) |> ignore;

        let win = new MainWindow()
        app.AddWindow(win)

        win.Show()
        Application.Run()
        0
