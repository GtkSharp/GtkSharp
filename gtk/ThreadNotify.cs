//
// ThreadNotify.cs: implements a notification for the thread running the Gtk main
// loop from another thread
//
// Author:
//    Miguel de Icaza (miguel@ximian.com).
//
// (C) 2002 Ximian, Inc.
//

namespace Gtk {
	using System.Runtime.InteropServices;
	using System.Threading;
	using System;

	// <summary>
	//    This delegate will be invoked on the main Gtk thread.
	// </summary>
	public delegate void ReadyEvent ();

	/// <summary>
	///   Utility class to help writting multi-threaded Gtk applications
	/// </summary>
	/// <remarks/>
	///   
	public class ThreadNotify {

		//
		// DllImport functions from Gtk
		//
		[DllImport ("libgtk-win32-2.0-0.dll")]
		private static extern int gdk_input_add (int s, int cond, GdkInputFunction f, IntPtr data);
		public delegate void GdkInputFunction (IntPtr data, int source, int cond);

		//
		// Libc stuff
		//
		[DllImport ("libc.so.6")]
		private static extern int pipe (int [] fd);
		
		[DllImport ("libc.so.6")]
		private static extern unsafe int read (int fd, byte *b, int count);
		
		[DllImport ("libc.so.6")]
		private static extern unsafe int write (int fd, byte *b, int count);

		
		GdkInputFunction notify_pipe;
		int [] pipes;

		ReadyEvent re;

		/// <summary>
		///   The ReadyEvent delegate will be invoked on the current thread (which should
		///   be the Gtk thread) when another thread wakes us up by calling WakeupMain
		/// </summary>
		public ThreadNotify (ReadyEvent re)
		{
			notify_pipe = new GdkInputFunction (NotifyPipe);
			pipes = new int [2];
			pipe (pipes);
			gdk_input_add (pipes [0], 1, notify_pipe, (IntPtr) 0);
			this.re = re;
		}
		
		void NotifyPipe (IntPtr data, int source, int cond)
		{
			byte s;
			
			unsafe {
				lock (this) {
					read (pipes [0], &s, 1);
					notified = false;
				}
			}
			
			re ();
		}

		bool notified = false;
		
		/// <summary>
		///   Invoke this function from a thread to call the `ReadyEvent'
		///   delegate provided in the constructor on the Main Gtk thread
		/// </summary>
		public void WakeupMain ()
		{
			unsafe {
				byte s;

				lock (this){
					if (notified)
						return;
					write (pipes [1], &s, 1);
					notified = true;
				}
			}
		}
	}
}
