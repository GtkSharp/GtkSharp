using System;
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
		image.Pixbuf = Pixbuf.LoadFromResource ("sample.svg");
		this.Add (image);

		this.ShowAll ();
	}

	void OnWinDelete (object sender, DeleteEventArgs e)
	{
		Application.Quit ();
	}
}

