Imports System
Imports Gtk
Imports UI = Gtk.Builder.ObjectAttribute

Namespace GtkNamespace
    Public Class MainWindow
        Inherits Window

        Private _counter = 0
        <UI>Private _label1 As Label
        <UI>Private _button1 As Button

        Public Sub New (builder as Builder)
            MyBase.New(builder.GetObject("MainWindow").Handle)

            builder.Autoconnect (Me)
            
            AddHandler MyBase.DeleteEvent, AddressOf Window_Delete
            AddHandler _button1.Clicked, AddressOf Button1_Clicked
        End Sub
        
        Public Sub New ()
            Me.New(new Builder("MainWindow.glade"))
        End Sub
        
        Private Sub Window_Delete (ByVal sender As Object, ByVal a As DeleteEventArgs)
            Application.Quit ()
            a.RetVal = true
        End Sub

        Private Sub Button1_Clicked (ByVal sender As Object, ByVal a As EventArgs)
            _counter += 1
            _label1.Text = "Hello World! This button has been clicked " + _counter.ToString() + " time(s)."
        End Sub
        
    End Class
End Namespace
