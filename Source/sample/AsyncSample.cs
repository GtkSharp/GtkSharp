// AsyncSample.cs - Async call using SynchronizationContext
//
// Author: Bertrand Lorentz <bertrand.lorentz@gmail.com>
//
// Copyright (c) 2012 Bertrand Lorentz
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.

namespace Samples
{
	using Gtk;
	using Gdk;
	using System;
	using System.Threading;

	public class AsyncSample
	{
		static Label msg;
		static Label label;
		static Button button;
		static int count;
		static Thread main_thread;
		
		public static int Main (string[] args)
		{
			Application.Init ();
			main_thread = Thread.CurrentThread;

			Gtk.Window win = new Gtk.Window ("Gtk# Hello Async World");
			win.DeleteEvent += new DeleteEventHandler (Window_Delete);
			label = new Label ("Doing nothing");
			msg = new Label ("Do Work");
			button = new Button (msg);
			button.Clicked += delegate { DoWork (); };
			VBox box = new VBox (true, 8);
			box.Add (label);
			box.Add (button);
			win.Add (box);
			win.BorderWidth = 4;
			win.ShowAll ();

			Application.Run ();
			
			return 0;
		}

#if NET_4_5 // Enable this when we require Mono 3.0
		static async void DoWork ()
		{
			label.Text = "Starting Work";

			int res = await DoLongOperation ();

			CheckThread ("Updating UI");
			label.Text = String.Format ("Work Done ({0})", res);
		}
#else
		static void DoWork ()
		{
			label.Text = "Starting Work";

			var sc = SynchronizationContext.Current;
			ThreadPool.QueueUserWorkItem (delegate {
				int res = DoLongOperation ();
				sc.Post (delegate {
					CheckThread ("Updating UI");
					label.Text = String.Format ("Work Done ({0})", res);
				}, null);
			});
		}
#endif

		static int DoLongOperation ()
		{
			count++;
			CheckThread ("Doing long operation");
			Thread.Sleep (2000);
			return count;
		}

		static void CheckThread (string msg)
		{
			if (Thread.CurrentThread == main_thread) {
				Console.WriteLine ("In main thread - {0}", msg);
			} else {
				Console.WriteLine ("Not in main thread - {0}", msg);
			}
		}

		static void Window_Delete (object obj, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
		}
	}
}

