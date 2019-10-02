// Pango.Attribute - Attribute "base class"
//
// Copyright (c) 2005, 2007, 2008 Novell, Inc.
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

namespace Pango {

	using System;
	using System.Runtime.InteropServices;

	public class Attribute : GLib.IWrapper, IDisposable {

		IntPtr raw;

		internal Attribute (IntPtr raw)
		{
			this.raw = raw;
		}

		static Pango.AttrType GetAttrType (IntPtr raw)
		{
			if (raw == IntPtr.Zero)
				return AttrType.Invalid;
			IntPtr klass = Marshal.ReadIntPtr (raw);
			return (AttrType) Marshal.ReadInt32 (klass);
		}

		public static Attribute GetAttribute (IntPtr raw)
		{
			switch (GetAttrType (raw)) {
			case Pango.AttrType.Language:
				return new AttrLanguage (raw);
			case Pango.AttrType.Family:
				return new AttrFamily (raw);
			case Pango.AttrType.Style:
				return new AttrStyle (raw);
			case Pango.AttrType.Weight:
				return new AttrWeight (raw);
			case Pango.AttrType.Variant:
				return new AttrVariant (raw);
			case Pango.AttrType.Stretch:
				return new AttrStretch (raw);
			case Pango.AttrType.Size:
				return new AttrSize (raw);
			case Pango.AttrType.FontDesc:
				return new AttrFontDesc (raw);
			case Pango.AttrType.Foreground:
				return new AttrForeground (raw);
			case Pango.AttrType.Background:
				return new AttrBackground (raw);
			case Pango.AttrType.Underline:
				return new AttrUnderline (raw);
			case Pango.AttrType.Strikethrough:
				return new AttrStrikethrough (raw);
			case Pango.AttrType.Rise:
				return new AttrRise (raw);
			case Pango.AttrType.Shape:
				return new AttrShape (raw);
			case Pango.AttrType.Scale:
				return new AttrScale (raw);
			case Pango.AttrType.Fallback:
				return new AttrFallback (raw);
			case Pango.AttrType.LetterSpacing:
				return new AttrLetterSpacing (raw);
			case Pango.AttrType.UnderlineColor:
				return new AttrUnderlineColor (raw);
			case Pango.AttrType.StrikethroughColor:
				return new AttrStrikethroughColor (raw);
			case Pango.AttrType.Gravity:
				return new AttrGravity (raw);
			case Pango.AttrType.GravityHint:
				return new AttrGravityHint (raw);
			default:
				return new Attribute (raw);
			}
		}

		~Attribute ()
		{
			Dispose ();
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_pango_attribute_destroy(IntPtr raw);
		static d_pango_attribute_destroy pango_attribute_destroy = FuncLoader.LoadFunction<d_pango_attribute_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attribute_destroy"));

		public void Dispose ()
		{
			if (raw != IntPtr.Zero) {
				pango_attribute_destroy (raw);
				raw = IntPtr.Zero;
			}
			GC.SuppressFinalize (this);
		}

		public IntPtr Handle {
			get {
				return raw;
			}
		}

		public static GLib.GType GType {
			get {
				return GLib.GType.Pointer;
			}
		}

		public Pango.AttrType Type {
			get { return GetAttrType (raw); }
		}

		internal struct NativeStruct {
			IntPtr klass;
			public uint start_index;
			public uint end_index;
		}

		NativeStruct Native {
			get { return (NativeStruct) Marshal.PtrToStructure (raw, typeof(NativeStruct)); }
		}

		public uint StartIndex {
			get { return Native.start_index; }
			set {
				NativeStruct native = Native;
				native.start_index = value;
				Marshal.StructureToPtr (native, raw, false);
			}
		}

		public uint EndIndex {
			get { return Native.end_index; }
			set {
				NativeStruct native = Native;
				native.end_index = value;
				Marshal.StructureToPtr (native, raw, false);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_pango_attribute_copy(IntPtr raw);
		static d_pango_attribute_copy pango_attribute_copy = FuncLoader.LoadFunction<d_pango_attribute_copy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attribute_copy"));

		public Pango.Attribute Copy () {
			return GetAttribute (pango_attribute_copy (raw));
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_pango_attribute_equal(IntPtr raw1, IntPtr raw2);
		static d_pango_attribute_equal pango_attribute_equal = FuncLoader.LoadFunction<d_pango_attribute_equal>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attribute_equal"));

		public bool Equal (Pango.Attribute attr2) {
			return pango_attribute_equal (raw, attr2.raw);
		}
	}
}

