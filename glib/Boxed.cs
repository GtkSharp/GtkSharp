// GtkSharp.Boxed.cs - Base class for deriving marshallable structures.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GLib {

	using System;

	/// <summary> 
	///	Boxed Class
	/// </summary>
	///
	/// <remarks>
	///	An abstract base class to derive structures and marshal them.
	/// </remarks>

	public class Boxed {
		object raw;

		/// <summary>
		///	Boxed Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Boxed type from a raw ref.
		/// </remarks>

		public Boxed (object o)
		{
			this.raw = raw;
		}

		/// <summary>
		///	Handle Property
		/// </summary>
		/// 
		/// <remarks>
		///	Gets a marshallable IntPtr.
		/// </remarks>

		public virtual object Obj {
			get {
				return raw;
			}
			set {
				raw = value;
			}
		}
	}
}
