// GLib.Signals.Simple.cs - GLib Simple Signal implementation
//
// Authors: Bob Smith <bob@thestuff.net>
//	    Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Bob Smith & Mike Kestner

namespace GLib {
	using System;
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

	public delegate void SimpleDelegate (IntPtr obj, String name);

	/// <summary>
	///	SimpleSignal Class
	/// </summary>
	///
	/// <remarks>
	///	Wraps a simple signal which contains no single-specific data.
	/// </remarks>

	public class SimpleSignal {

		private static int _RefCount = 0;
		private static SimpleDelegate _Delegate;
		private static GCHandle _GCHandle;

		private static void SimpleCallback(IntPtr obj, String name)
		{
			Object o = Object.GetObject(obj);
			EventHandler eh = (EventHandler) o.Events[name];
			if (eh != null)
			{
				eh(o, EventArgs.Empty);
			}
		}

		public static SimpleDelegate Delegate
		{
			get
			{
				if (_Delegate == null)
				{
					_Delegate = new SimpleDelegate(SimpleCallback);

/* FIXME: Can't do this until a layout attribute is defined for SimpleCallback
 * apparently, since this throws an ArgumentException:Type does not have a 
 * layout attribute.
 *
 *					_GCHandle = GCHandle.Alloc (_Delegate, GCHandleType.Pinned);
 */
				}
				_RefCount++;
				return _Delegate;
			}
		}

		public static void Unref()
		{
			_RefCount--;
			if (_RefCount < 1)
			{
				_RefCount = 0;
				_GCHandle.Free();
				_Delegate = null;
			}
		}
	}
}
