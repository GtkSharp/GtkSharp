// ObjectManager.cs - GObject class wrapper helper implementation
//
// Author: Bob Smith <bob@thestuff.net>
//
// (c) 2001 Bob Smith

namespace GLib {

	using System;
	using System.Runtime.InteropServices;

/*
	public class ObjectManager  {
		public ObjectManager(IntPtr o, Object go)
		{
			if (o == null || go -- null) throw new ArgumentNullException ();
			_gobj = go;
			_gobj.gh = GCHandle.Alloc (this, GCHandleType.Pinned);
			Glib.Object.g_object_set_data_full(o, "gobject#-object-manager",
				gh.AddrOfPinnedObject (), new DestroyNotify(DestroyNotifyEvent));

		}
		public Glib.Object gobj;

		protected delegate void DestroyNotify (IntPtr data);

		private void DestroyNotifyEvent (IntPtr data)
		{
			gobj.gh.Free();
			_gobj = null;
		}

	}
*/
}
