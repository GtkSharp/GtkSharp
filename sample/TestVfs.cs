//
// TestVfs.cs: Test for Gnome.Vfs bindings.
//
// Author:
//        Tamara Roberson (foxxygirltamara@gmail.com)
//
// (C) 2004 Tamara Roberson
//

using System;
using System.Text;
using System.IO;

class FileDialog : Gtk.FileChooserDialog {

        public FileDialog (string title, Gtk.FileChooserAction action) 
        : base (title, null, action, "gnome-vfs")
        {
                this.LocalOnly = false;

                this.AddButton (Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
                this.AddButton (Gtk.Stock.Open, Gtk.ResponseType.Ok);

                this.DefaultResponse = Gtk.ResponseType.Ok;
        }

        public Gnome.Vfs.Uri GetUri ()
        {
                int resp = this.Run ();
                
                this.Hide ();
                        
                if (resp != (int) Gtk.ResponseType.Ok)
                        throw new Exception ("No File Selected.");

                return new Gnome.Vfs.Uri (this.Uri);
        }


        public static Gnome.Vfs.Uri OpenFile (string title)
        {
                FileDialog fd = new FileDialog (title, Gtk.FileChooserAction.Open);
                return fd.GetUri ();
        }

        public static Gnome.Vfs.Uri SaveFile(string title)
        {
                FileDialog fd = new FileDialog (title, Gtk.FileChooserAction.Save);
                return fd.GetUri ();
        }
}


public class TestVfs
{
        public TestVfs ()
        {
                ShowFileInfo ();
                WriteFile ();
                ReadFile ();
                OpenAsync ();
                CreateAsync ();
        }


        private void ShowFileInfo()
        {
                // Ask for a file
                Gnome.Vfs.Uri uri = FileDialog.OpenFile ("Show File Info");
                Console.WriteLine ("Selected uri\t= {0}", uri.ToString ());
                
                // MimeType
                string mimeType = Gnome.Vfs.MimeType.GetMimeTypeForUri (uri.ToString ());
                Console.WriteLine ("Mimetype\t= {0}", mimeType);
        
                // IsLocal
                Gnome.Vfs.FileInfoOptions options = Gnome.Vfs.FileInfoOptions.Default;
                Gnome.Vfs.FileInfo info = new Gnome.Vfs.FileInfo (uri.ToString (), options);
                
                Console.WriteLine ("IsLocal\t\t= {0}", info.IsLocal);
        }
        
        private void WriteFile ()
        {
                // Ask for a file
                Gnome.Vfs.Uri uri = FileDialog.SaveFile ("Write to File");
        
                // Create Stream
                Gnome.Vfs.VfsStream vs = new Gnome.Vfs.VfsStream (uri.ToString (), FileMode.CreateNew);
        
                // Write to the file
                UTF8Encoding utf8 = new UTF8Encoding ();
                byte [] buf = utf8.GetBytes ("Testing 1 2 3, asdjfaskjdhfkajshdf");
                vs.Write (buf, 0, buf.Length);
        
                // Close the file
                vs.Close();
        }
        
        private void ReadFile()
        {
                // Ask for a file
                Gnome.Vfs.Uri uri = FileDialog.OpenFile ("Read File");
        
                // Create Stream
                Gnome.Vfs.VfsStream vs = new Gnome.Vfs.VfsStream (uri.ToString (), FileMode.Open);
        
                // Read File byte by byte
                while (true) {
                        int c = vs.ReadByte ();
        
                        if (c < 0) {
                                Console.WriteLine ("EOF");
                                break;
                        }
        
                        Console.Write ((char) c);
                }
        
                // Close File
                vs.Close ();
        }
        
        private void OpenAsync ()
        {
                // Ask for a file
                Gnome.Vfs.Uri uri = FileDialog.OpenFile ("Open File Asynchronously");
        
                // Open the file Asynchronously
                Gnome.Vfs.AsyncCallback openCallback = new Gnome.Vfs.AsyncCallback (OnUriOpen);
                Gnome.Vfs.Async.Open (uri.ToString (), Gnome.Vfs.OpenMode.Read, 0, openCallback);
        }
        
        private void CreateAsync ()
        {
                // Ask for a file
                Gnome.Vfs.Uri uri = FileDialog.SaveFile ("Create File Asynchronously");
                
                // Create a file Asynchronously
                Gnome.Vfs.FilePermissions perms =
                        Gnome.Vfs.FilePermissions.UserRead  |
                        Gnome.Vfs.FilePermissions.UserWrite |
                        Gnome.Vfs.FilePermissions.GroupRead |
                        Gnome.Vfs.FilePermissions.OtherRead;
                                
                Gnome.Vfs.AsyncCallback createCallback = new Gnome.Vfs.AsyncCallback (OnUriCreate);
                Gnome.Vfs.Async.Create (uri, Gnome.Vfs.OpenMode.Write, false, perms, 0, createCallback);
        }
        
        static void OnUriOpen (Gnome.Vfs.Handle handle, Gnome.Vfs.Result result)
        {
                Console.WriteLine ("Async.Open result\t= {0} ({1})", Gnome.Vfs.Vfs.ResultToString (result), result);
        
                Gnome.Vfs.AsyncReadCallback readCallback = new Gnome.Vfs.AsyncReadCallback (OnUriRead);

                byte [] buffer = new byte [128];
                Gnome.Vfs.Async.Read (handle, out buffer [0], 128, readCallback);
        }
                
        public static void OnUriRead (Gnome.Vfs.Handle handle, Gnome.Vfs.Result result, 
                                      byte [] buffer, ulong bytesRequested, ulong bytesRead)
        {
                Console.WriteLine ("Async.Read result\t= {0} ({1})", Gnome.Vfs.Vfs.ResultToString(result), result);
        
                if (result != Gnome.Vfs.Result.Ok)
                        return;

                Console.WriteLine ("bytesRequested\t\t= {0}", bytesRequested);
                Console.WriteLine ("bytesRead\t\t= {0}", bytesRead);

                Console.Write("bytes\t\t\t= ");
                for (int i = 0; i < (int) bytesRead; i++)
                        Console.Write ((char) buffer [i]);
                Console.WriteLine ();

                Gnome.Vfs.AsyncReadCallback readCallback = new Gnome.Vfs.AsyncReadCallback (OnUriRead);

                byte [] buf = new byte [128];
                Gnome.Vfs.Async.Read (handle, out buf [0], 128, readCallback);
        }
                
        public void OnUriCreate (Gnome.Vfs.Handle handle, Gnome.Vfs.Result result)
        {
                Console.WriteLine ("Async.Create result\t= {0} ({1})", Gnome.Vfs.Vfs.ResultToString (result), result);
                        
                if (result != Gnome.Vfs.Result.Ok)
                        return;
                UTF8Encoding utf8 = new UTF8Encoding ();
                byte [] buffer = utf8.GetBytes ("Testing 1 2 3 asdlfjalsjdfksjdf \nGustavo Giráldez\n");
                Gnome.Vfs.AsyncWriteCallback writeCallback = new Gnome.Vfs.AsyncWriteCallback (OnUriWrite);
                Gnome.Vfs.Async.Write (handle, out buffer [0], (uint) buffer.Length, writeCallback);
        }
                
        public static void OnUriWrite (Gnome.Vfs.Handle handle, Gnome.Vfs.Result result, 
                                       byte [] buffer, ulong bytesRequested, ulong bytesWritten)
        {
                Console.WriteLine ("Async.Write result\t= {0} ({1})", Gnome.Vfs.Vfs.ResultToString (result), result);
        }
        
        
        static void Main (string [] args)
        {
                // Initialize Gtk
                Gtk.Application.Init ();

                new TestVfs ();

                // Run!                        
                Gtk.Application.Run ();
        }        
}
