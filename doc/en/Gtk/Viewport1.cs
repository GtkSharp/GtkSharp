// ViewportApp.cs - Gtk.Viewport class Test implementation
//
// Author: Lee Mallabone <gnome@fonicmonkey.net>
//
// (c) 2003 Lee Mallabone

namespace GtkSamples {

	using Gtk;
	using System;

	public class ViewportApp  {

		public static ScrolledWindow CreateViewport()
		{
			ScrolledWindow scroller = new ScrolledWindow();
			Viewport viewer = new Viewport();
			
			Table widgets = new Table(1, 2, false);
			
			widgets.Attach(new Entry("This is example Entry 1"), 0, 1, 0, 1);
			widgets.Attach(new Entry("This is example Entry 2"), 1, 2, 0, 1);
			
			// Place the widgets in a Viewport, and the Viewport in a ScrolledWindow
			viewer.Add(widgets);
			scroller.Add(viewer);
			return scroller;
		}
		
		public static int Main (string[] args)
		{
			Application.Init ();
			Window win = new Window ("Viewport Tester");
			win.SetDefaultSize (180, 50);
			win.DeleteEvent += new DeleteEventHandler (Window_Delete);
			ScrolledWindow scroller = CreateViewport();
			win.Add (scroller);
			win.ShowAll ();
			Application.Run ();
			return 0;
		}

		static void Window_Delete (object obj, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
		}

	}
}
