// TextIter.cs - customizations to Gtk.TextIter
//
// Authors: Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2004 Novell, Inc.
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

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	public partial struct TextIter {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_gtk_text_iter_get_char(ref Gtk.TextIter raw);
		static d_gtk_text_iter_get_char gtk_text_iter_get_char = FuncLoader.LoadFunction<d_gtk_text_iter_get_char>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_text_iter_get_char"));

		public string Char { 
			get {
				return GLib.Marshaller.GUnicharToString (gtk_text_iter_get_char (ref this));
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_text_iter_get_marks(ref TextIter iter);
		static d_gtk_text_iter_get_marks gtk_text_iter_get_marks = FuncLoader.LoadFunction<d_gtk_text_iter_get_marks>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_text_iter_get_marks"));

		public TextMark[] Marks {
			get {
				IntPtr raw_ret = gtk_text_iter_get_marks (ref this);
				if (raw_ret == IntPtr.Zero)
					return new TextMark [0];
				GLib.SList list = new GLib.SList(raw_ret);
				TextMark[] result = new TextMark [list.Count];
				for (int i = 0; i < list.Count; i++)
					result [i] = list [i] as TextMark;
				return result;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_text_iter_get_tags(ref TextIter iter);
		static d_gtk_text_iter_get_tags gtk_text_iter_get_tags = FuncLoader.LoadFunction<d_gtk_text_iter_get_tags>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_text_iter_get_tags"));

		public TextTag[] Tags {
			get {
				IntPtr raw_ret = gtk_text_iter_get_tags (ref this);
				if (raw_ret == IntPtr.Zero)
					return new TextTag [0];
				GLib.SList list = new GLib.SList(raw_ret);
				TextTag[] result = new TextTag [list.Count];
				for (int i = 0; i < list.Count; i++)
					result [i] = list [i] as TextTag;
				return result;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_text_iter_get_toggled_tags(ref TextIter iter, bool toggled_on);
		static d_gtk_text_iter_get_toggled_tags gtk_text_iter_get_toggled_tags = FuncLoader.LoadFunction<d_gtk_text_iter_get_toggled_tags>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_text_iter_get_toggled_tags"));

		public TextTag[] GetToggledTags (bool toggled_on)
		{
			IntPtr raw_ret = gtk_text_iter_get_toggled_tags (ref this, toggled_on);
			if (raw_ret == IntPtr.Zero)
				return new TextTag [0];
			GLib.SList list = new GLib.SList(raw_ret);
			TextTag[] result = new TextTag [list.Count];
			for (int i = 0; i < list.Count; i++)
				result [i] = list [i] as TextTag;
			return result;
		}

		[Obsolete("Replaced by overload without IntPtr argument")]
		public bool ForwardFindChar (Gtk.TextCharPredicate pred, IntPtr user_data, Gtk.TextIter limit) {
			return ForwardFindChar (pred, limit);
		}

		[Obsolete("Replaced by overload without IntPtr argument")]
		public bool BackwardFindChar (Gtk.TextCharPredicate pred, IntPtr user_data, Gtk.TextIter limit) {
			return BackwardFindChar (pred, limit);
		}
	}
}

