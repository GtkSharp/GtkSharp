// Pango.ScriptIter
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

	public class ScriptIter : GLib.Opaque {

		IntPtr native_text;

		public ScriptIter(IntPtr raw) : base(raw) {}

		[DllImport("libpango-1.0-0.dll")]
		static extern IntPtr pango_script_iter_new(IntPtr text, int length);

		public ScriptIter (string text)
		{
			native_text = GLib.Marshaller.StringToPtrGStrdup (text);
			Raw = pango_script_iter_new (native_text, -1);
		}

		[DllImport("libpango-1.0-0.dll")]
		static extern void pango_script_iter_free (IntPtr raw);

		~ScriptIter ()
		{
			GLib.Marshaller.Free (native_text);
			pango_script_iter_free (Raw);
			Raw = IntPtr.Zero;
		}

		[DllImport("libpango-1.0-0.dll")]
		static extern void pango_script_iter_get_range (IntPtr raw, out IntPtr start, out IntPtr end, out Pango.Script script);

		[DllImport("libglib-2.0-0.dll")]
		static extern IntPtr g_utf8_pointer_to_offset (IntPtr str, IntPtr pos);

		public void GetRange (out int start, out int len, out Pango.Script script)
		{
			IntPtr start_ptr;
			IntPtr end_ptr;

			pango_script_iter_get_range (Handle, out start_ptr, out end_ptr, out script);
			start = (int)g_utf8_pointer_to_offset (native_text, start_ptr);
			len = (int)g_utf8_pointer_to_offset (start_ptr, end_ptr);
		}

		[DllImport("libpango-1.0-0.dll")]
		static extern bool pango_script_iter_next (IntPtr raw);

		public bool Next ()
		{
			return pango_script_iter_next (Handle);
		}

		[Obsolete ("Replaced by garbage collection")]
		public void Free ()
		{
		}
	}
}
