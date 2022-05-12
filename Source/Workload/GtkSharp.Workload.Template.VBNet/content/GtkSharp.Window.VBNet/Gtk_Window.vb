Imports System
Imports Gtk
Imports UI = Gtk.Builder.ObjectAttribute

Namespace GtkNamespace
    Public Class Gtk_Window
        Inherits Window

        Public Sub New (builder as Builder)
            MyBase.New (builder.GetRawOwnedObject("Gtk_Window"))

            builder.Autoconnect (Me)
        End Sub
        
        Public Sub New ()
            Me.New (new Builder ("Gtk_Window.glade"))
        End Sub
        
    End Class
End Namespace
