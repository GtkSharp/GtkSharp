//
// samples/rsvg/svghelloworkd.cs
//
// Author: Charles Iliya Krempeaux
//
using System;
using Gtk;

    class SvgHelloWorld
    {
            static void Main (string[] args)
            {
		    Application.Init ();
                    MyMainWindow app = new MyMainWindow ();
                    app.ShowAll ();
 
                    Application.Run ();
            }
    }

    class MyMainWindow : Gtk.Window
    {
            public MyMainWindow () : base ("SVG Hello World")
            {
                    this.DeleteEvent += new Gtk.DeleteEventHandler(delete_event);
                    string svg_file_name = "sample.svg";
                    Gdk.Pixbuf pixbuf = Rsvg.Pixbuf.FromFile (svg_file_name);

                    Gtk.Image  image = new Gtk.Image();
                    image.Pixbuf = pixbuf;

                    this.Add (image);
            }

            private void delete_event(object obj, Gtk.DeleteEventArgs args)
            {
                    Application.Quit();
            }
    }


