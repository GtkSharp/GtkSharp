// Gdk.Signals.SimpleEvent.cs - Gdk Simple Event Signal implementation
//
// Author: Bob Smith <bob@thestuff.net>
//
// (c) 2001 Bob Smith

namespace Gdk.Signals {
	using System;
	using System.Runtime.InteropServices;
	using Glib;
	using Gdk;

	public class SimpleEventArgs : EventArgs {
		public SimpleEventArgs(Gdk.Event event)
		{
			_event = event;
		}
		private Gdk.Event _event;
		public Gdk.Event Event
		{
			get
			{
				return _event;
			}
		}
		public static explicit operator Gdk.Event(SimpleEventArgs value)
		{
			return value.Event;
		}
	}

	public delegate bool SimpleEventDelegate(IntPtr obj, IntPtr data);
	public class SimpleEvent {
		public SimpleEvent(){}
		private static bool SimpleEventCallback(IntPtr obj, IntPtr e, IntPtr data)
		{
			Glib.Object o = Glib.Object.GetObject(obj);
			EventHandler eh = o.Events[(int)data];
			if (eh != null)
			{
				EventArgs args = new SimpleEventArgs (new Gdk.Event(e));
				eh(o, args);
			}
			return true; //FIXME: How do we manage the return value?
		}
		private static int _simpleRefCount;
		private static SimpleEventDelegate _simpleDelegate;
		private static GCHandle _simpleEventGCHandle;
		public static SimpleEventDelegate Delegate
		{
			get
			{
				if (SimpleEvent._simpleEventDelegate == null)
				{
					SimpleEvent._simpleDelegate = new SimpleEventDelegate(SimpleCallback);
					SimpleEvent._simpleGCHandle = GCHandle.Alloc (SimpleEvent._simpleEventDelegate, GCHandleType.Pinned);
				}
				SimpleEvent._simpleRefCount++;
				return SimpleEvent._simpleEventDelegate;
			}
		}
		public static void Unref()
		{
			SimpleEvent._simpleRefCount--;
			if (SimpleEvent._simpleRefCount < 1)
			{
				SimpleEvent._simpleRefCount = 0;
				SimpleEvent._simpleEventGCHandle.free();
				SimpleEvent._simpleDelegate = null;
			}
		}
	}
}
