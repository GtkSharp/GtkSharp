Imports System
Imports Gtk
Imports UI = Gtk.Builder.ObjectAttribute

Namespace GtkNamespace
    Public Class Gtk_Widget
        Inherits Box

        Public Sub New (builder as Builder)
            MyBase.New (builder.GetObject("Gtk_Widget").Handle)

            builder.Autoconnect (Me)
        End Sub
        
        Public Sub New ()
            Me.New (new Builder("Gtk_Widget.glade"))
        End Sub
        
    End Class
End Namespace
