namespace GtkNamespace

open Gtk

type MainWindow (builder : Builder) as this =
    inherit Window(builder.GetRawOwnedObject("MainWindow"))

    let mutable _label1 : Label = null
    let mutable _button1 : Button = null
    let mutable _counter = 0;

    do
        _label1 <- builder.GetObject("_label1") :?> Label
        _button1 <- builder.GetObject("_button1") :?> Button

        // Gtk 4 removed delete-event; CloseRequest is its replacement.
        this.CloseRequest.Add(fun args ->
            Application.Quit()
            args.RetVal <- false
        )
        _button1.Clicked.Add(fun _ ->
            _counter <- _counter + 1
            _label1.Text <- "Hello World! This button has been clicked " + _counter.ToString() + " time(s)."
        )

    new() = new MainWindow(new Builder("MainWindow.ui"))
