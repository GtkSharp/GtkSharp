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
		// Values are WeakReference instances to the object that creates the
		// delegate or instances of derived classes (if created from static methods).
		static Hashtable weakReferences = new Hashtable ();
		
		// The object 'o' is the object that creates the instance of the DelegateWrapper
		// derived class or null if created from a static method.
		// Note that the instances will never be disposed if they are created in a static
		// method.
		protected DelegateWrapper (object o)
		{
			if (o == null)
				o = this; // Never expires. Used in static methods.

			weakReferences [this] = new WeakReference (o);
		}

		// IMPORTANT: this method must be the first one called from the callback methods that
		// are invoked from unmanaged code.
		// If this method returns true, the object that created the delegate wrapper no longer
		// exists and the instance of the delegate itself is removed from the hash table.
		protected bool RemoveIfNotAlive ()
		{
			WeakReference r = null;
			r = weakReferences [this] as WeakReference;
			if (r != null && !r.IsAlive) {
				weakReferences.Remove (this);
				r = null;
			}

			return (r == null);
		}
	}
}

