// Opaque .cs - Opaque struct wrapper implementation
//
// Authors: Bob Smith <bob@thestuff.net>
//	    Mike Kestner <mkestner@speakeasy.net>
//	    Rachel Hestilow <hestilow@ximian.com>
//
// (c) 2001 Bob Smith and Mike Kestner, 2002 Rachel Hestilow

namespace GLib {

	using System;
	using System.Collections;
	using System.ComponentModel;
	using System.Runtime.InteropServices;

	/// <summary>
	///	Object Class
	/// </summary>
	///
	/// <remarks>
	///	Wrapper class for GObject.
	/// </remarks>

	public class Opaque : IWrapper {

		// Private class and instance members
		IntPtr _obj;
		static Hashtable Opaques = new Hashtable();

		/// <summary>
		///	GetObject Shared Method 
		/// </summary>
		///
		/// <remarks>
		///	Used to obtain a CLI typed object associated with a 
		///	given raw object pointer. This method is primarily
		///	used to wrap object references that are returned 
		///	by either the signal system or raw class methods that
		///	return opaque struct references.
		/// </remarks>
		///
		/// <returns>
		///	The wrapper instance.
		/// </returns>

		public static Opaque GetOpaque(IntPtr o)
		{
			Opaque obj = (Opaque)Opaques[(int)o];
			if (obj != null) return obj;
			return null; //FIXME: Call TypeParser here eventually.
		}

		/// <summary>
		///	Opaque Constructor
		/// </summary>
		///
		/// <remarks>
		///	Dummy constructor needed for derived classes.
		/// </remarks>

		public Opaque () {}

		/// <summary>
		///	Opaque Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates an opaque wrapper from a raw object reference.
		/// </remarks>

		public Opaque (IntPtr raw)
		{
			Raw = raw;
		}

		/// <summary>
		///	Raw Property
		/// </summary>
		///
		/// <remarks>
		///	The raw Opaque reference associated with this wrapper.
		///	Only subclasses of Opaque can access this read/write
		///	property.  For public read-only access, use the
		///	Handle property.
		/// </remarks>

		protected IntPtr Raw {
			get {
				return _obj;
			}
			set {
				Opaques [value] = this;
				_obj = value;
			}
		}       

		/// <summary>
		///	Handle Property
		/// </summary>
		///
		/// <remarks>
		///	The raw Opaque reference associated with this object.
		///	Subclasses can use Raw property for read/write
		///	access.
		/// </remarks>

		public IntPtr Handle {
			get {
				return _obj;
			}
		}

		public override bool Equals (object o)
		{
			if (!(o is Opaque))
				return false;

			return (Handle == ((Opaque) o).Handle);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>

		public override int GetHashCode ()
		{
			return Handle.GetHashCode ();
		}

		public static explicit operator System.IntPtr (Opaque opaque) {
			return opaque.Handle;
		}
	}
}
