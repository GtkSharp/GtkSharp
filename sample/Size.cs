// Size.cs - struct marshalling test 
//
// Author: Rachel Hestilow <hestilow@ximian.com> 
//
// (c) 2002 Rachel Hestilow

namespace GtkSamples {

	using Gtk;
	using Gdk;
	using GtkSharp;
	using System;

	public class SizeTest {

		public static int Main (string[] args)
		{
			Application.Init ();
			Gtk.Window win = new Gtk.Window ("Gtk# Hello World");
			win.DeleteEvent += new DeleteEventHandler (Window_Delete);
			win.SizeAllocated += new SizeAllocatedHandler (Size_Allocated);
			win.ShowAll ();
			Application.Run ();
			return 0;
		}

		static void Window_Delete (object obj, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
		}

		static void Size_Allocated (object obj, SizeAllocatedArgs args)
		{
			System.Drawing.Rectangle rect = args.Allocation;
			if (rect == System.Drawing.Rectangle.Empty)
				Console.WriteLine ("ERROR: Allocation is null!");
			Console.WriteLine ("Size: ({0}, {1})", rect.Width, rect.Height);
		}
	}
}
