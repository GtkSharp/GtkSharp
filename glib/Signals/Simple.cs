// Glib.Signals.Simple.cs - Glib Simple Signal implementation
//
// Author: Bob Smith <bob@thestuff.net>
//
// (c) 2001 Bob Smith

namespace Glib.Signals {
	using System;
	using System.Runtime.InteropServices;
	using Glib;

	public delegate void SimpleDelegate(IntPtr obj, IntPtr data);
	public class Simple {
		public Simple(){}
		private static void SimpleCallback(IntPtr obj, IntPtr data)
		{
			Glib.Object o = Glib.Object.GetObject(obj);
			EventHandler eh = o.Events[(int)data];
			if (eh != null)
			{
				eh(o, EventArgs.Empty);
			}
		}
		private static SimpleDelegate _simpleDelegate;
		private static GCHandle _simpleGCHandle;
		public static SimpleDelegate Delegate
		{
			get
			{
				if (Simple._simpleDelegate == null)
				{
					Simple._simpleDelegate = new SimpleDelegate(SimpleCallback);
					Simple._simpleGCHandle = GCHandle.Alloc (Simple._simpleDelegate, GCHandleType.Pinned);
				}
				return Simple._simpleDelegate;
			}
		}
	}
}
