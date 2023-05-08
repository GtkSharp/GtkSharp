Imports System
Imports Gtk

Namespace GtkNamespace
    Public Class MainClass

        Public Shared Sub Main ()
            Application.Init ()

            Dim app as new Application ("org.GtkNamespace.GtkNamespace", GLib.ApplicationFlags.None)
            app.Register (GLib.Cancellable.Current)

            Dim win as new MainWindow ()
            app.AddWindow (win)

            win.Show ()
            Application.Run ()
        End Sub
        
    End Class
End Namespace
