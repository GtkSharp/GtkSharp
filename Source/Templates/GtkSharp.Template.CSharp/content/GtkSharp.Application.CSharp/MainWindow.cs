using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace GtkNamespace
{
    class MainWindow : Window
    {
        [UI] private Label _label1 = null;
        [UI] private Button _button1 = null;

        private int _counter;

        public MainWindow() : this(new Builder("MainWindow.ui")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            // Gtk 4 removed GtkWidget::delete-event. CloseRequest is the
            // signal a window gets when the user asks to close it; returning
            // false lets the default handler go ahead with the close.
            CloseRequest += Window_CloseRequest;
            _button1.Clicked += Button1_Clicked;
        }

        private void Window_CloseRequest(object sender, CloseRequestArgs a)
        {
            Application.Quit();
            a.RetVal = false;
        }

        private void Button1_Clicked(object sender, EventArgs a)
        {
            _counter++;
            _label1.Text = "Hello World! This button has been clicked " + _counter + " time(s).";
        }
    }
}
