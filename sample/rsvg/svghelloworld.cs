//
// samples/rsvg/svghelloworkd.cs
//
// Author: Charles Iliya Krempeaux
//

    class SvgHelloWorld
    {
            static void Main(string[] args)
            {
                    Gnome.Program program =
                    new Gnome.Program("Hello World", "1.0", Gnome.Modules.UI, args);

                    MyMainWindow app = new MyMainWindow(program);
                    app.Show();
 
                    program.Run();
            }
    }



    class MyMainWindow
            : Gnome.App
    {
            Gnome.Program program;

            public MyMainWindow(Gnome.Program gnome_program)
                    : base("SVG Hello World", "SVG Hello World")
            {
                    this.program = gnome_program;

                    this.DeleteEvent += new Gtk.DeleteEventHandler(delete_event);


                    string svg_file_name = "sample.svg";
                    Gdk.Pixbuf pixbuf = Rsvg.Tool.PixbufFromFile(svg_file_name);

                    Gtk.Image  image = new Gtk.Image();
                    image.Pixbuf = pixbuf;

                    this.Contents = image;
            }

            private void delete_event(object obj, Gtk.DeleteEventArgs args)
            {
                    this.program.Quit();
            }
    }


