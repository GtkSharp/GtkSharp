// GladeViewer.cs - Tests for LibGlade in C#
//
// Author: Ricardo Fernández Pascual <ric@users.sourceforge.net>
//
// (c) 2002 Ricardo Fernández Pascual

namespace GladeSamples {
	using System;
	
	using Gtk;
	using Gnome;
	using Glade;
	using GtkSharp;
	using System.IO;
	using System.Reflection;

	public class GladeTest : Program
	{
		[Glade.Widget("main_window")]
		Gtk.Window main_window;

		public static void Main (string[] args)
		{
			new GladeTest (args).Run ();
		}
		public GladeTest (string[] args, params object[] props) 
			: base ("GladeTest", "0.1", Modules.UI, args, props)
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
			Quit ();
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
	}
}

