// GLib.Signals.Simple.cs - GLib Simple Signal implementation
//
// Authors: Bob Smith <bob@thestuff.net>
//	    Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Bob Smith & Mike Kestner

namespace GLib {
	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
	using GLib;

	/// <summary>
	///	SimpleDelegate Delegate
	/// </summary>
	///
	/// <remarks>
	///	Used to connect to simple signals which contain no signal-
	///	specific data.
	/// </remarks>

	public delegate void SimpleDelegate (IntPtr obj, int key);

	/// <summary>
	///	SimpleSignal Class
	/// </summary>
	///
	/// <remarks>
	///	Wraps a simple signal which contains no single-specific data.
	/// </remarks>

	public class SimpleSignal {

		// A counter used to produce unique keys for instances.
		private static int _NextKey = 0;

		// Hashtable containing refs to all current instances.
		private static Hashtable _Instances = new Hashtable ();

		// locals to create and pin the shared delegate.
		private static SimpleDelegate _Delegate;
		private static GCHandle _GCHandle;

		// Shared delegate that relays events to registered handlers.
		private static void SimpleCallback(IntPtr obj, int inst_key)
		{
			if (!_Instances.Contains (inst_key))
				throw new Exception ("Unexpected signal key");

			SimpleSignal ss = (SimpleSignal) _Instances [inst_key];
			EventArgs args = new EventArgs ();
			ss._handler (ss._obj, args);
		}

		// private instance members
		private GLib.Object _obj;
		private EventHandler _handler;
		private int _key;

		/// <summary>
		///	SimpleSignal Constructor
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
						IntPtr obj, IntPtr name, 
						SimpleDelegate cb,
						int key, IntPtr p, int flags);

		public SimpleSignal (GLib.Object obj, IntPtr raw,
				     String name, EventHandler eh)
		{
			if (_Delegate == null) {
				_Delegate = new SimpleDelegate(SimpleCallback);

				/* FIXME: need layout attribute for 
				 * SimpleCallback to avoid an exception.
 				 * _GCHandle = GCHandle.Alloc (
				 *	_Delegate, GCHandleType.Pinned);
 				 */
			}

			_key = _NextKey++;
			_obj = obj;
			_handler = eh;
			_Instances [_key] = this;

			g_signal_connect_data (
				raw, Marshal.StringToHGlobalAnsi (name),
				_Delegate, _key, new IntPtr (0), 0);
		}

		// Destructor needed to release references from the instance
		// table and unpin the delegate if no refs remain.
		~SimpleSignal ()
		{
			_Instances.Remove (_key);

			if (_Instances.Count == 0) {
				// FIXME: when pin works _GCHandle.Free();
				_Delegate = null;
			}
		}
	}
}
