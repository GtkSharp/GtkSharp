// Object.cs - GtkObject class wrapper implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace Gtk {

	using System;
	using System.ComponentModel;
	using System.Runtime.InteropServices;

	public abstract class Object :  GLib.Object {

		/// <summary>
		///	Destroy Event
		/// </summary>
		///
		/// <remarks>
		///	Occurs when the Object is destroyed.
		/// </remarks>

		private static readonly object DestroyEvent = new object ();

		public event EventHandler Destroy
		{
			add
			{
                                if (Events[DestroyEvent] == null)
				{
					ConnectSignal ("destroy", new SimpleSignal (EmitDestroyEvent));
				}
				Events.AddHandler (DeleteEvent, value);
			}
                        remove 
			{
				Events.RemoveHandler (DeleteEvent, value);
			}
		}

		private static void EmitDestroyEvent (IntPtr obj, IntPtr data)
		{
			Glib.Object o = Glib.Object.GetObject(obj);
			EventHandler eh = (EventHandler)(o.Events[DeleteEvent]);
			if (eh != null)
			{
				EventArgs args = new EventArgs ();
				eh(this, args);
			}
		}

		protected delegate void SimpleCallback (IntPtr obj);

		[DllImport("gtk-1.3")]
		static extern void gtk_signal_connect_full (
					IntPtr obj, string evname,
					SimpleCallback cb, IntPtr unsupported, 
					IntPtr data, IntPtr destroycb, 
					int objsig, int after );


		protected void ConnectSignal (string name, SimpleCallback cb)
		{
			gtk_signal_connect_full (RawObject, name, cb,
					new IntPtr (0), new IntPtr (0), 
					new IntPtr (0), 0, 0);
		}


	}
}
