Imports System
Imports Gtk
Imports UI = Gtk.Builder.ObjectAttribute

Namespace GtkNamespace
    Public Class Gtk_Dialog
        Inherits Dialog

        Public Sub New (builder as Builder)
            MyBase.New (builder.GetObject("Gtk_Dialog").Handle)

            builder.Autoconnect (Me)
            DefaultResponse = ResponseType.Cancel
            
            AddHandler MyBase.Response, AddressOf Dialog_OnResponse
        End Sub
        
        Public Sub New ()
            Me.New (new Builder ("Gtk_Dialog.glade"))
        End Sub
        
        Private Sub Dialog_OnResponse (ByVal sender As Object, ByVal args As ResponseArgs)
            Hide ()
        End Sub
        
    End Class
End Namespace
