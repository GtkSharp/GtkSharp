using System;
using System.IO;
using Gtk;
using Rsvg;

class RsvgResourceTest : Window
{
	static void Main ()
	{
		Application.Init ();
		new RsvgResourceTest ();
		Application.Run ();
	}

	RsvgResourceTest () : base ("Rsvg Draw Test")
	{
		this.DeleteEvent += new DeleteEventHandler (OnWinDelete);

		Image image = new Image ();

		// contrived way to get a stream
		System.IO.Stream s = System.Reflection.Assembly.GetCallingAssembly ().GetManifestResourceStream ("sample.svg");
                if (s == null)
                        throw new ArgumentException ("resource must be a valid resource name of 'assembly'.");

		image.Pixbuf = Pixbuf.LoadFromStream (s);
		this.Add (image);

		this.ShowAll ();
	}

	void OnWinDelete (object sender, DeleteEventArgs e)
	{
		Application.Quit ();
	}
}

