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

	public abstract class Boxed {

		private IntPtr raw;

		public Boxed () : this (IntPtr.Zero) {}

		/// <summary>
		///	Boxed Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Boxed type from a raw ref.
		/// </remarks>

		public Boxed (IntPtr raw)
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

		public virtual IntPtr Handle {
			get {
				return raw;
			}
			set {
				raw = value;
			}
		}

		/// <summary>
		///	Raw Property
		/// </summary>
		/// 
		/// <remarks>
		///	Gets or sets a marshallable IntPtr.
		/// </remarks>

		protected IntPtr Raw {
			get {
				return raw;
			}
			set {
				raw = value;
			}
		}

		/// <summary>
		///	FromNative Method
		/// </summary>
		/// 
		/// <remarks>
		///	Gets a Boxed type from a raw IntPtr.
		/// </remarks>

		public static GLib.Boxed FromNative (IntPtr raw) 
		{
			// FIXME:
			return null;
		}
	}
}
