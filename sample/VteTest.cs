using System;
using Gtk;
using GtkSharp;
using Gnome;
using GnomeSharp;
using Vte;
using VteSharp;

class T
{
	Program program;
	
	static void Main (string[] args)
	{
		new T (args);
	}
	
	T (string[] args)
	{
		program = new Program ("test", "0.0", Modules.UI, args);
		App app = new App ("test", "Test for vte widget");
		app.SetDefaultSize (600, 450);
		app.DeleteEvent += new DeleteEventHandler (OnAppDelete);
		
		ScrolledWindow sw = new ScrolledWindow ();
		Terminal term = new Terminal ();
		term.CursorBlinks = true;
		term.ScrollOnOutput = true;
		//term.BackgroundTransparent = true;
		term.Encoding = "UTF-8";
		
		Console.WriteLine (term.UsingXft);
		Console.WriteLine (term.Encoding);
		Console.WriteLine (term.StatusLine);
		
		sw.AddWithViewport (term);

		app.Contents = sw;
		app.ShowAll ();
		program.Run ();
	}
	
	private void OnTextInserted (object o, EventArgs args)
	{
	}
	
	private void OnAppDelete (object o, DeleteEventArgs args)
	{
		program.Quit ();
	}
}
