using System;
using System.Collections;
using Gtk;
using Gnome;
using Vte;

class T
{
	static void Main (string[] args)
	{
		new T (args);
	}
	
	T (string[] args)
	{
		Program program = new Program ("vte-sharp-test", "0.0", Modules.UI, args);
		App app = new App ("vte-sharp-test", "Test for vte widget");
		app.SetDefaultSize (600, 450);
		app.DeleteEvent += new DeleteEventHandler (OnAppDelete);
		
		HBox hbox = new HBox ();
		Terminal term = new Terminal ();
		term.EncodingChanged += new EventHandler (OnEncodingChanged);
		term.CursorBlinks = true;
		term.MouseAutohide = true;
		term.ScrollOnKeystroke = true;
		term.DeleteBinding = TerminalEraseBinding.Auto;
		term.BackspaceBinding = TerminalEraseBinding.Auto;
		term.Encoding = "UTF-8";
		term.FontFromString = "Monospace 12";
		term.TextDeleted += new EventHandler (OnTextDeleted);
		term.ChildExited += new EventHandler (OnChildExited);

		VScrollbar vscroll = new VScrollbar (term.Adjustment);
		hbox.PackStart (term);
		hbox.PackStart (vscroll);

		Gdk.Color white = new Gdk.Color ();
		Gdk.Color.Parse ("white", ref white);
		// FIXME: following line is broken
		//term.ColorBackground = white;

		Gdk.Color black = new Gdk.Color ();
		Gdk.Color.Parse ("black", ref black);
		// FIXME: following line is broken
		//term.ColorForeground = black;
		
		// Create a palette with 0 colors. this could be replaced with
		// a palette of colors with a size of 0, 8, 16, or 24.
		Gdk.Color[] palette = new Gdk.Color[0];
		
		term.SetColors (black, white, palette, palette.Length);
		
		//Console.WriteLine (term.UsingXft);
		//Console.WriteLine (term.Encoding);
		//Console.WriteLine (term.StatusLine);

		string[] argv = Environment.GetCommandLineArgs ();
		// seems to want an array of "variable=value"
		string[] envv = new string [Environment.GetEnvironmentVariables ().Count];
		int i = 0;
		foreach (DictionaryEntry e in Environment.GetEnvironmentVariables ())
		{
			if (e.Key == "" || e.Value == "")
				continue;
			string tmp = String.Format ("{0}={1}", e.Key, e.Value);
			envv[i] = tmp;
			i ++;
		}
		
		int pid = term.ForkCommand (Environment.GetEnvironmentVariable ("SHELL"), argv, envv, Environment.CurrentDirectory, false, true, true);
		Console.WriteLine ("Child pid: {0}", pid);

		app.Contents = hbox;
		app.ShowAll ();
		program.Run ();
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
		Console.WriteLine ("text inserted");
	}

	private void OnChildExited (object o, EventArgs args)
	{
		// optionally we could just reset instead of quitting
		Console.WriteLine ("child exited");
		Application.Quit ();
	}
	
	private void OnAppDelete (object o, DeleteEventArgs args)
	{
		Application.Quit ();
	}
}
