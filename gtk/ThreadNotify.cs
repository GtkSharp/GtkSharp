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
		bool disposed;
		ReadyEvent re;
		GLib.IdleHandler idle;
		bool notified;

		/// <summary>
		///   The ReadyEvent delegate will be invoked on the current thread (which should
		///   be the Gtk thread) when another thread wakes us up by calling WakeupMain
		/// </summary>
		public ThreadNotify (ReadyEvent re)
		{
			this.re = re;
			idle = new GLib.IdleHandler (CallbackWrapper);
		}
		
		bool CallbackWrapper ()
		{
			lock (this) {
				if (disposed)
					return false;

				notified = false;
			}
			
			re ();
			return false;
		}

		/// <summary>
		///   Invoke this function from a thread to call the `ReadyEvent'
		///   delegate provided in the constructor on the Main Gtk thread
		/// </summary>
		public void WakeupMain ()
		{
			lock (this){
				if (notified)
					return;
				
				notified = true;
				GLib.Idle.Add (idle);
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
			lock (this) {
				disposed = true;
			}
		}
	}
}

