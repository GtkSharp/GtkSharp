// GladeViewer.cs - Tests for LibGlade in C#
//
// Author: Ricardo Fernández Pascual <ric@users.sourceforge.net>
//
// (c) 2002 Ricardo Fernández Pascual

namespace GladeSamples {
	using System;
	
	using Gtk;
	using Glade;

	public class GladeTest
	{
		[Glade.Widget]
		Gtk.Window main_window;

		public static void Main (string[] args)
		{
			Application.Init ();

			new GladeTest ();

			Application.Run ();
		}

		public GladeTest () 
		{
			/* Note that we load the XML info from the assembly instead of using 
			   an external file. You don't have to distribute the .glade file if 
			   you don't want */
			Glade.XML gxml = new Glade.XML (null, "test.glade", "main_window", null);
			gxml.Autoconnect (this);

			if (main_window != null)
				Console.WriteLine ("Main Window Title: \"{0}\"", main_window.Title);
			else
				Console.WriteLine ("WidgetAttribute is broken.");
		}

		public void OnWindowDeleteEvent (object o, DeleteEventArgs args) 
		{
			Application.Quit ();
			args.RetVal = true;
		}
		
		public void OnButton1Clicked (System.Object b, EventArgs e) 
		{
			Console.WriteLine ("Button 1 clicked");
		}

		public static void OnButton2Clicked (System.Object b, EventArgs e) 
		{
			Console.WriteLine ("Button 2 clicked");
		}
		
		public void OnButton2Entered (System.Object b, EventArgs e) 
		{
			Console.WriteLine ("Button 2 entered");
		}

		public void OnQuitActivated (object o, EventArgs args)
		{
			Application.Quit ();
		}
	}
}

