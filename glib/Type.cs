// GLib.Type.cs - GLib GType class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2003 Mike Kestner, Novell, Inc.

namespace GLib {

	using System;
	using System.Runtime.InteropServices;

	/// <summary> 
	///	GType Class
	/// </summary>
	///
	/// <remarks>
	///	An arbitrary data type similar to a CORBA Any which is used
	///	to get and set properties on Objects.
	/// </remarks>

	[StructLayout(LayoutKind.Sequential)]
	public struct GType {

		IntPtr val;

		/// <summary>
		///	GType Constructor
		/// </summary>
		///
		/// <remarks>
		///	Constructs a new GType from a native GType value.
		/// </remarks>

		public GType (IntPtr val) {
			this.val = val;
		}

		public static readonly GType Invalid = new GType ((IntPtr) TypeFundamentals.TypeInvalid);
		public static readonly GType None = new GType ((IntPtr) TypeFundamentals.TypeNone);
		public static readonly GType String = new GType ((IntPtr) TypeFundamentals.TypeString);
		public static readonly GType Boolean = new GType ((IntPtr) TypeFundamentals.TypeBoolean);
		public static readonly GType Int = new GType ((IntPtr) TypeFundamentals.TypeInt);
		public static readonly GType Double = new GType ((IntPtr) TypeFundamentals.TypeDouble);
		public static readonly GType Float = new GType ((IntPtr) TypeFundamentals.TypeFloat);
		public static readonly GType Char = new GType ((IntPtr) TypeFundamentals.TypeChar);
		public static readonly GType UInt = new GType ((IntPtr) TypeFundamentals.TypeUInt);
		public static readonly GType Object = new GType ((IntPtr) TypeFundamentals.TypeObject);
		public static readonly GType Pointer = new GType ((IntPtr) TypeFundamentals.TypePointer);
		public static readonly GType Boxed = new GType ((IntPtr) TypeFundamentals.TypeBoxed);

		public IntPtr Val {
			get {
				return val;
			}
		}

		public override bool Equals (object o)
		{
			if (!(o is GType))
				return false;

			return ((GType) o) == this;
		}

		public static bool operator == (GType a, GType b)
		{
			return a.Val == b.Val;
		}

		public static bool operator != (GType a, GType b)
		{
			return a.Val != b.Val;
		}

		public override int GetHashCode ()
		{
			return val.GetHashCode ();
		}

		public override string ToString ()
		{
			return val.ToString();
		}
	}
}
