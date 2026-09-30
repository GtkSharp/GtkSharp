// TextBuffer.cs - customizations to Gtk.TextBuffer.
// 
// Authors:  Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2004-2006  Novell, Inc.
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

	public partial class TextBuffer {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_text_buffer_set_text(IntPtr raw, IntPtr text, int len);
		static d_gtk_text_buffer_set_text gtk_text_buffer_set_text = FuncLoader.LoadFunction<d_gtk_text_buffer_set_text>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_text_buffer_set_text"));

		public void Clear ()
		{
			Gtk.TextIter start = StartIter, end = EndIter;
			Delete (ref start, ref end);
		}

		[Obsolete ("Replaced by 'ref TextIter, ref TextIter' overload")]
		public void Delete (TextIter start, TextIter end )
		{
			Delete (ref start, ref end);
		}

		// PasteClipboard(Clipboard): GtkClipboard is gone; Gtk 4 uses GdkClipboard.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_text_buffer_insert(IntPtr raw, ref Gtk.TextIter iter, IntPtr text, int len);
		static d_gtk_text_buffer_insert gtk_text_buffer_insert = FuncLoader.LoadFunction<d_gtk_text_buffer_insert>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_text_buffer_insert"));

		[Obsolete ("Replaced by 'ref TextIter iter' overload")]
		public void Insert (TextIter iter, string text)
		{
			Insert (ref iter, text);
		}

		public void Insert (ref Gtk.TextIter iter, string text)
		{
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (text);
			gtk_text_buffer_insert (Handle, ref iter, native, -1);
			GLib.Marshaller.Free (native);
		}

		[Obsolete ("Replaced by 'ref TextIter iter' overload")]
		public void InsertRange (TextIter iter, TextIter start, TextIter end )
		{
			InsertRange (ref iter, start, end);
		}

		[Obsolete ("Replaced by 'ref TextIter iter' overload")]
		public void InsertWithTags (TextIter iter, string text, params TextTag[] tags)
		{
			InsertWithTags (ref iter, text, tags);
		}

		public void InsertWithTags (ref TextIter iter, string text, params TextTag[] tags)
		{
			TextIter start;
			int offset = iter.Offset;
			Insert (ref iter, text);

			start = GetIterAtOffset (offset);
			iter = GetIterAtOffset (offset + text.Length);

			foreach (TextTag t in tags)
				this.ApplyTag (t, start, iter);
		}

		public void InsertWithTagsByName (ref TextIter iter, string text, params string[] tagnames)
		{
			TextIter start;
			int offset = iter.Offset;
			Insert (ref iter, text);

			start = GetIterAtOffset (offset);
			iter = GetIterAtOffset (offset + text.Length);

			foreach (string tagname in tagnames) {
				TextTag tag = TagTable.Lookup (tagname);
				if (tag != null)
					this.ApplyTag (tag, start, iter);
			}
		}

		[Obsolete("Use the TextBuffer.Text property's setter")]
		public void SetText (string text)
		{
			Text = text;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_text_buffer_insert_interactive(IntPtr raw, ref Gtk.TextIter iter, IntPtr text, int len, bool default_editable);
		static d_gtk_text_buffer_insert_interactive gtk_text_buffer_insert_interactive = FuncLoader.LoadFunction<d_gtk_text_buffer_insert_interactive>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_text_buffer_insert_interactive"));

		public bool InsertInteractive(ref Gtk.TextIter iter, string text, bool default_editable)
		{
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (text);
			bool result = gtk_text_buffer_insert_interactive(Handle, ref iter, native, -1, default_editable);
			GLib.Marshaller.Free (native);
			return result;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_text_buffer_insert_interactive_at_cursor(IntPtr raw, IntPtr text, int len, bool default_editable);
		static d_gtk_text_buffer_insert_interactive_at_cursor gtk_text_buffer_insert_interactive_at_cursor = FuncLoader.LoadFunction<d_gtk_text_buffer_insert_interactive_at_cursor>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_text_buffer_insert_interactive_at_cursor"));

		public bool InsertInteractiveAtCursor(string text, bool default_editable)
		{
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (text);
			bool result = gtk_text_buffer_insert_interactive_at_cursor(Handle, native, -1, default_editable);
			GLib.Marshaller.Free (native);
			return result;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_text_buffer_insert_at_cursor(IntPtr raw, IntPtr text, int len);
		static d_gtk_text_buffer_insert_at_cursor gtk_text_buffer_insert_at_cursor = FuncLoader.LoadFunction<d_gtk_text_buffer_insert_at_cursor>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_text_buffer_insert_at_cursor"));

		public void InsertAtCursor(string text)
		{
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (text);
			gtk_text_buffer_insert_at_cursor(Handle, native, -1);
			GLib.Marshaller.Free (native);
		}

	}
}

