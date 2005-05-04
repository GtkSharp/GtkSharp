// Pango.Attribute - Attribute "base class"
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

namespace Pango {

	using System;
	using System.Runtime.InteropServices;

	public class Attribute : GLib.IWrapper, IDisposable {

		IntPtr raw;

		internal Attribute (IntPtr raw)
		{
			this.raw = raw;
		}

		[DllImport("pangosharpglue-2")]
		static extern Pango.AttrType pangosharp_attribute_get_attr_type (IntPtr raw);

		public static Attribute GetAttribute (IntPtr raw)
		{
			switch (pangosharp_attribute_get_attr_type (raw)) {
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
#if GTK_SHARP_2_6
			case Pango.AttrType.LetterSpacing:
				return new AttrLetterSpacing (raw);
			case Pango.AttrType.UnderlineColor:
				return new AttrUnderlineColor (raw);
			case Pango.AttrType.StrikethroughColor:
				return new AttrStrikethroughColor (raw);
#endif
			default:
				return new Attribute (raw);
			}
		}

		~Attribute ()
		{
			Dispose ();
		}

		[DllImport("libpango-1.0-0.dll")]
		static extern void pango_attribute_destroy (IntPtr raw);

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
			get {
				return pangosharp_attribute_get_attr_type (raw);
			}
		}

		[DllImport("pangosharpglue-2")]
		static extern uint pangosharp_attribute_get_start_index (IntPtr raw);

		[DllImport("pangosharpglue-2")]
		static extern void pangosharp_attribute_set_start_index (IntPtr raw, uint index);

		public uint StartIndex {
			get {
				return pangosharp_attribute_get_start_index (raw);
			}
			set {
				pangosharp_attribute_set_start_index (raw, value);
			}
		}

		[DllImport("pangosharpglue-2")]
		static extern uint pangosharp_attribute_get_end_index (IntPtr raw);

		[DllImport("pangosharpglue-2")]
		static extern void pangosharp_attribute_set_end_index (IntPtr raw, uint index);

		public uint EndIndex {
			get {
				return pangosharp_attribute_get_end_index (raw);
			}
			set {
				pangosharp_attribute_set_end_index (raw, value);
			}
		}

		[DllImport("libpango-1.0-0.dll")]
		static extern IntPtr pango_attribute_copy (IntPtr raw);

		public Pango.Attribute Copy () {
			return GetAttribute (pango_attribute_copy (raw));
		}

		[DllImport("libpango-1.0-0.dll")]
		static extern bool pango_attribute_equal (IntPtr raw1, IntPtr raw2);

		public bool Equal (Pango.Attribute attr2) {
			return pango_attribute_equal (raw, attr2.raw);
		}
	}
}
