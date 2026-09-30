// Pango.AttrFontFeatures.cs - Pango AttrFontFeatures customizations
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

	public partial struct AttrFontFeatures {

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate IntPtr d_pango_attr_font_features_new (IntPtr features);
		static d_pango_attr_font_features_new pango_attr_font_features_new = FuncLoader.LoadFunction<d_pango_attr_font_features_new> (FuncLoader.GetProcAddress (GLibrary.Load (Library.Pango), "pango_attr_font_features_new"));

		// The one PangoAttribute-returning function in the binding that is a
		// constructor: it allocates, so the wrapper owns what it gets back. The
		// generated form went through the manual symbol's from_fmt, which now
		// borrows, and would have leaked one attribute per call.
		public static Pango.Attribute New (string features)
		{
			IntPtr native_features = GLib.Marshaller.StringToPtrGStrdup (features);
			IntPtr raw_ret = pango_attr_font_features_new (native_features);
			GLib.Marshaller.Free (native_features);
			return Pango.Attribute.GetAttribute (raw_ret, true);
		}
	}
}
