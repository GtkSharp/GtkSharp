// GLib.Type.cs - GLib GType class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2003 Mike Kestner
// Copyright (c) 2003 Novell, Inc.
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
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct GType {

		IntPtr val;

		public GType (IntPtr val) {
			this.val = val;
		}

		public static readonly GType Invalid = new GType ((IntPtr) TypeFundamentals.TypeInvalid);
		public static readonly GType None = new GType ((IntPtr) TypeFundamentals.TypeNone);
		public static readonly GType String = new GType ((IntPtr) TypeFundamentals.TypeString);
		public static readonly GType Boolean = new GType ((IntPtr) TypeFundamentals.TypeBoolean);
		public static readonly GType Int = new GType ((IntPtr) TypeFundamentals.TypeInt);
		public static readonly GType Int64 = new GType ((IntPtr) TypeFundamentals.TypeInt64);
		public static readonly GType UInt64 = new GType ((IntPtr) TypeFundamentals.TypeUInt64);

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
