// Gdk.Signals.SimpleEvent.cs - Gdk Simple Event Signal implementation
//
// Author: Bob Smith <bob@thestuff.net>
//
// (c) 2001 Bob Smith

namespace Gdk {
	using System;
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

	public delegate bool SimpleEventDelegate(IntPtr obj, IntPtr e, String name);
	public class SimpleEvent {

		private static bool SimpleEventCallback (IntPtr obj, IntPtr e, String name)
		{
			GLib.Object o = GLib.Object.GetObject (obj);
			EventHandler eh = (EventHandler) o.Events [name];
			if (eh != null)
			{
				EventArgs args = new SimpleEventArgs (new Gdk.Event(e));
				eh(o, args);
			}
			return true; //FIXME: How do we manage the return value?
		}
		private static int _RefCount;
		private static SimpleEventDelegate _Delegate;
		private static GCHandle _GCHandle;
		public static SimpleEventDelegate Delegate
		{
			get
			{
				if (_Delegate == null)
				{
					_Delegate = new SimpleEventDelegate(SimpleEventCallback);
					_GCHandle = GCHandle.Alloc (_Delegate, GCHandleType.Pinned);
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
