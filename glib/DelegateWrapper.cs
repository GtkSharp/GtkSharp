// DelegateWrapper.cs - Delegate wrapper implementation
//
// Authors:
// 	Rachel Hestilow <hestilow@ximian.com>
// 	Gonzalo Panigua Javier <gonzalo@ximian.com>
//
// (c) 2002 Rachel Hestilow
// (c) 2003 Ximian, Inc. (http://www.ximian.com)

namespace GLib {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

	/// <summary>
	///	DelegateWrapper Class
	/// </summary>
	///
	/// <remarks>
	///	Wrapper class for delegates.
	/// </remarks>

	public class DelegateWrapper
	{
		// Keys in the hashtable are instances of classes derived from this one.
		// Values are each instance's destroy notification delegate
		static Hashtable instances = new Hashtable ();

		// This list holds references to wrappers for static
		// methods. These will never expire.
		static ArrayList static_instances = new ArrayList ();

		static int notify_count = 0;
		
		// The object 'o' is the object that creates the instance of the DelegateWrapper
		// derived class or null if created from a static method.
		// Note that the instances will never be disposed if they are created in a static
		// method.
		protected DelegateWrapper (object o)
		{
			if (o != null) {
				// If o is a GObject, we can get
				// destroy notification. Otherwise
				// no additional references to
				// the wrapper are kept.
				// FIXME: This should work because
				// currently only GObjects store
				// callbacks over the long-term

				if (o is GLib.Object) {
					AddDestroyNotify ((GLib.Object) o);
				}
			} else {
				// If o is null, we cannot ask for a destroy
				// notification, so the wrapper never expires.

				lock (typeof (DelegateWrapper)) {
					static_instances.Add (this);
				}
			}
		}

		private delegate void DestroyNotify (IntPtr data);

		[DllImport("libgobject-2.0-0.dll")]
		private static extern void g_object_set_data (IntPtr obj, string name, IntPtr data, DestroyNotify destroy);
		
		private void AddDestroyNotify (GLib.Object o) {
			// This is a bit of an ugly hack. There is no
			// way of getting a destroy notification
			// explicitly, so we set some data and ask
			// for notification when it is removed

			string name = String.Format ("_GtkSharpDelegateWrapper_{0}", notify_count);
			DestroyNotify destroy = new DestroyNotify (this.OnDestroy);

			g_object_set_data (o.Handle, name, IntPtr.Zero, destroy);
			lock (typeof (DelegateWrapper)) {
				instances[this] = destroy;
				notify_count++;
			}
		}

		// This callback is invoked by GLib to indicate that the
		// object that owned the native delegate wrapper no longer
		// exists and the instance of the delegate itself is removed from the hash table.
		private void OnDestroy (IntPtr data) {
			lock (typeof (DelegateWrapper)) {
				if (instances.ContainsKey (this)) {
					instances.Remove (this);
				}
			}
		}
	}
}

