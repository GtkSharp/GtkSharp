// Gdk.SimpleEvent.cs - Gdk Simple Event Signal implementation
//
// Author: Bob Smith <bob@thestuff.net>
//	   Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Bob Smith and Mike Kestner

namespace Gdk {
	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
	using GLib;
	using Gdk;

	public class SimpleEventArgs : EventArgs {
		public SimpleEventArgs(Gdk.Event evnt)
		{
			_evnt = evnt;
		}
		private Gdk.Event _evnt;
		public Gdk.Event Event
		{
			get
			{
				return _evnt;
			}
		}
		public static explicit operator Gdk.Event(SimpleEventArgs value)
		{
			return value.Event;
		}
	}

	/// <summary>
	///	SimpleEventDelegate Delegate
	/// </summary>
	///
	/// <remarks>
	///	Callback used to connect to GdkEvent based signals.
	/// </remarks>

	public delegate bool SimpleEventDelegate(IntPtr obj, IntPtr e, 
						 int key);

	/// <summary>
	///	SimpleEvent Class
	/// </summary>
	///
	/// <remarks>
	///	Connects to a specified signal on a raw object and relays
	///	events to an EventHandle when they occur.
	/// </remarks>

	public class SimpleEvent {

		// A Counter used to produce unique keys for instances.
		private static int _NextKey = 0;

		// A hash table containing refs to all current instances.
		private static Hashtable _Instances = new Hashtable ();

		// locals to create and pin the shared delegate.
		private static SimpleEventDelegate _Delegate;
		private static GCHandle _GCHandle;

		// Shared delegate that relays events to registered handlers
		private static bool SimpleEventCallback (IntPtr obj, IntPtr e, 
							 int inst_key)
		{
			if (!_Instances.Contains (inst_key)) 
				throw new Exception ("Unexpected event key");

			SimpleEvent se = (SimpleEvent) _Instances [inst_key];
			Event evnt = new Event ();
			Marshal.PtrToStructure (e, evnt);
			EventArgs args = new SimpleEventArgs (evnt);
			se._handler (se._obj, args);
			return true; //FIXME: How do we manage the return value?
		}

		// private instance members
		private GLib.Object _obj;
		private EventHandler _handler;
		private int _key;

		/// <summary>
		///	SimpleEvent Constructor
		/// </summary>
		///
		/// <remarks>
		///	Registers a new event handler for a specified signal.
		///	A connection to the raw object signal is made which
		///	causes any events which occur to be relayed to the
		///	event handler.
		/// </remarks>

		[DllImport ("gobject-1.3.dll", CharSet=CharSet.Ansi,
			    CallingConvention=CallingConvention.Cdecl)]
		static extern void g_signal_connect_data (
				IntPtr obj, IntPtr name, SimpleEventDelegate eh,
				int key, IntPtr p, int flags);

		public SimpleEvent (GLib.Object obj, IntPtr raw, 
				    String name, EventHandler eh)
		{
			if (_Delegate == null) {
				_Delegate = new SimpleEventDelegate (
							SimpleEventCallback);
				/* FIXME: Exception thrown for lack of layout
				_GCHandle = GCHandle.Alloc (
						_Delegate, GCHandleType.Pinned);
				*/
			}

			_key = _NextKey++;
			_Instances [_key] = this;
			_obj = obj;
			_handler = eh;

			g_signal_connect_data (
				raw, Marshal.StringToHGlobalAnsi (name),
				_Delegate, _key, new IntPtr (0), 0);
		}

		// Destructor is needed to release references from the instance
		// table and unpin the delegate if no refs remain.

		~SimpleEvent ()
		{
			// FIXME: Disconnect the signal
			_Instances.Remove (_key);

			if (_Instances.Count == 0) {
				/* FIXME: when the handle can be obtained
				_GCHandle.Free();
				*/
				_Delegate = null;
			}
		}
	}
}
