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
		term.EncodingChanged += new EventHandler (OnEncodingChanged);
		term.CursorBlinks = true;
		term.MouseAutohide = true;
		term.ScrollOnKeystroke = true;
		term.DeleteBinding = TerminalEraseBinding.Auto;
		term.Encoding = "UTF-8";
		term.FontFromString = "Monospace";
		term.Commit += new VteSharp.CommitHandler (OnCommit);
		term.TextDeleted += new EventHandler (OnTextDeleted);

		Gdk.Color white = new Gdk.Color ();
		Gdk.Color.Parse ("white", ref white);
		term.ColorBackground = white;
		
		Console.WriteLine (term.UsingXft);
		Console.WriteLine (term.Encoding);
		Console.WriteLine (term.StatusLine);

		string argv = Environment.GetCommandLineArgs () [0];

		string envv = "";
		// FIXME: send the env vars to ForkCommand
		Console.WriteLine (Environment.GetEnvironmentVariables ().Count);
		Console.WriteLine (Environment.CurrentDirectory);
		
		//int pid = term.ForkCommand ("/bin/bash", argv, envv, Environment.CurrentDirectory, false, true, true);
		//Console.WriteLine ("Child pid: " + pid);

		sw.AddWithViewport (term);

		app.Contents = sw;
		app.ShowAll ();
		program.Run ();
	}

	private void OnCommit (object o, VteSharp.CommitArgs args)
	{
		Terminal term = (Terminal) o;
		term.Feed (args.P0);
	}

	private void OnTextDeleted (object o, EventArgs args)
	{
		Console.WriteLine ("text deleted");
	}
	
	private void OnEncodingChanged (object o, EventArgs args)
	{
		Console.WriteLine ("encoding changed");
	}
	
	private void OnTextInserted (object o, EventArgs args)
	{
	}
	
	private void OnAppDelete (object o, DeleteEventArgs args)
	{
		program.Quit ();
	}
}
