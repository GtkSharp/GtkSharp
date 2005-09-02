//
// Thread.cs - Using threads with Gtk#
//
// Author: Miguel de Icaza
//
// (c) 2005 Novell, Inc.

namespace GtkSamples {

	using Gtk;
	using Gdk;
	using System;
	using System.Threading;

	public class HelloThreads  {
		static Label msg;
		static Button but;
		static int count;
		static Thread thr;
		
		public static int Main (string[] args)
		{
			Application.Init ();
			Gtk.Window win = new Gtk.Window ("Gtk# Hello World");
			win.DeleteEvent += new DeleteEventHandler (Window_Delete);
			msg = new Label ("Click to quit");
			but = new Button (msg);
			but.Clicked += delegate { thr.Abort (); Application.Quit (); };
			win.Add (but);
			win.ShowAll ();

			thr = new Thread (ThreadMethod);
			thr.Start ();
			
			Application.Run ();
			
			return 0;
		}

		static void ThreadMethod ()
		{
			Console.WriteLine ("Starting thread");
			while (true){
				count++;
				Thread.Sleep (500);
				Application.Invoke (delegate {
					msg.Text = String.Format ("Click to Quit ({0})", count);
				});
			}
		}
		
		static void Window_Delete (object obj, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
		}

	}
}
