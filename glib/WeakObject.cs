// WeakObject.cs - Object to hold managed references via native weakref.
//
// Authors: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2005 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General 
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


namespace GLib {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

	internal class WeakObject {

		GCHandle gc_handle;

		static DestroyNotify notify = new DestroyNotify (OnNativeDestroy);
		delegate void DestroyNotify (IntPtr data);
		static void OnNativeDestroy (IntPtr data)
		{
			GCHandle gch = (GCHandle) data;
			WeakObject obj = gch.Target as WeakObject;
			obj.Dispose ();
			gch.Free ();
		}

		void Dispose ()
		{
			signals = null;
			data = null;
		}

		WeakObject (IntPtr obj)
		{
			gc_handle = GCHandle.Alloc (this);
			g_object_set_data_full (obj, "gtk_sharp_weak_object", (IntPtr) gc_handle, notify);
		}

		Hashtable data;
		public Hashtable Data {
			get {
				if (data == null)
					data = new Hashtable ();
				return data;
			}
		}

		Hashtable signals;
		public Hashtable Signals {
			get {
				if (signals == null)
					signals = new Hashtable ();
				return signals;
			}
		}

		public static WeakObject Lookup (IntPtr obj)
		{
			IntPtr data = g_object_get_data (obj, "gtk_sharp_weak_object");
			if (data == IntPtr.Zero)
				return new WeakObject (obj);

			GCHandle gch = (GCHandle) data;
			return gch.Target as WeakObject;
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_object_get_data (IntPtr instance, string key);

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_set_data_full (IntPtr instance, string key, IntPtr data, DestroyNotify notify);
	}
}

