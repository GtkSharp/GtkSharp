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

	public class GladeTest : Program
	{
		public static void Main (string[] args)
		{
			new GladeTest (args).Run ();
		}
		public GladeTest (string[] args, params object[] props) 
			: base ("GladeTest", "0.1", Modules.UI, args, props)
		{
			Glade.XML gxml = new Glade.XML ("test.glade", "main_window", null);
			gxml.Autoconnect (this);
		}

		public void OnWindowDeleteEvent (object o, DeleteEventArgs args) 
		{
			Quit ();
			args.RetVal = true;
		}
		
		public void OnButton1Clicked (Object b, EventArgs e) 
		{
			Console.WriteLine ("Button 1 clicked");
		}

		public static void OnButton2Clicked (Object b, EventArgs e) 
		{
			Console.WriteLine ("Button 2 clicked");
		}
		
		public void OnButton2Entered (Object b, EventArgs e) 
		{
			Console.WriteLine ("Button 2 entered");
		}
	}
}

