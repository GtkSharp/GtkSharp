// Size.cs - struct marshalling test 
//
// Author: Rachel Hestilow <hestilow@ximian.com> 
//
// (c) 2002 Rachel Hestilow

namespace GtkSamples {

	using Gtk;
	using Gdk;
	using System;

	public class SizeTest {

		public static int Main (string[] args)
		{
			Application.Init ();
			Gtk.Window win = new Gtk.Window ("Gtk# Hello World");
			win.DeleteEvent += new DeleteEventHandler (Window_Delete);
			win.SizeAllocated += new SizeAllocatedHandler (Size_Allocated);
			win.SizeRequested += new SizeRequestedHandler (Size_Requested);
			win.ShowAll ();
			Application.Run ();
			return 0;
		}

		static void Window_Delete (object obj, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
		}

		static void Size_Requested (object obj, SizeRequestedArgs args)
		{
			Requisition req = args.Requisition;
			Console.WriteLine ("Requesting 100 x 100");
			req.Width = req.Height = 100;
			args.Requisition = req;
		}

		static void Size_Allocated (object obj, SizeAllocatedArgs args)
		{
			Rectangle rect = args.Allocation;
			if (rect.Width == 0 || rect.Height == 0)
				Console.WriteLine ("ERROR: Allocation is null!");
			Console.WriteLine ("Size: ({0}, {1})", rect.Width, rect.Height);
		}
	}
}
