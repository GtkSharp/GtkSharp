//
// ThreadNotify.cs: implements a notification for the thread running the Gtk main
// loop from another thread
//
// Authors:
//    Miguel de Icaza (miguel@ximian.com).
//    Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc.
// (C) 2004 Novell, Inc.
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
	public class ThreadNotify : IDisposable {

		//
		// DllImport functions from Gtk
		//
		[DllImport ("libgtk-win32-2.0-0.dll")]
		static extern uint gdk_input_add (int s, int cond, GdkInputFunction f, IntPtr data);
		public delegate void GdkInputFunction (IntPtr data, int source, int cond);

		[DllImport ("gtksharpglue")]
		static extern int pipe_create (int [] fd);
		
		[DllImport ("gtksharpglue")]
		static extern unsafe int pipe_read (int fd, byte *b, int count);
		
		[DllImport ("gtksharpglue")]
		static extern unsafe int pipe_write (int fd, byte *b, int count);

		[DllImport ("gtksharpglue")]
		static extern int pipe_close (int [] fd);

		GdkInputFunction notify_pipe;
		int [] pipes;
		bool disposed;
		uint tag;

		ReadyEvent re;

		/// <summary>
		///   The ReadyEvent delegate will be invoked on the current thread (which should
		///   be the Gtk thread) when another thread wakes us up by calling WakeupMain
		/// </summary>
		public ThreadNotify (ReadyEvent re)
		{
			notify_pipe = new GdkInputFunction (NotifyPipe);
			pipes = new int [2];
			pipe_create (pipes);
			tag = gdk_input_add (pipes [0], 1, notify_pipe, (IntPtr) 0);
			this.re = re;
		}
		
		void NotifyPipe (IntPtr data, int source, int cond)
		{
			byte s;
			
			unsafe {
				lock (this) {
					pipe_read (pipes [0], &s, 1);
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
					
					pipe_write (pipes [1], &s, 1);
					notified = true;
				}
			}
		}

		public void Close ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		~ThreadNotify ()
		{
			Dispose (false);
		}

		void IDisposable.Dispose ()
		{
			Close ();
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (!disposed) {
				disposed = true;
				GLib.Source.Remove (tag);
				pipe_close (pipes);
			}

			pipes = null;
			notify_pipe = null;
		}
	}
}

