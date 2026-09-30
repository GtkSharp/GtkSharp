// GLib.GString.cs : Marshaler for GStrings
//
// Author: Mike Kestner  <mkestner@ximian.com>
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


namespace GLib {
	using System;
	using System.Runtime.InteropServices;

	/// <summary>
	/// A growable string buffer owned by GLib.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A dozen calls across gdk, gsk, gio and gtk are shaped
	/// <c>void something_print (Thing *, GString *out)</c>: the caller supplies
	/// the buffer and reads it afterwards. That only works if the caller can
	/// hold on to the buffer and see what landed in it, so this type has to be
	/// a real wrapper -- with a handle the caller keeps and a way to read the
	/// text back -- rather than a marshalling helper.
	/// </para>
	/// <para>
	/// It was neither. There was no way to read a GString at all, and
	/// <see cref="PtrToString"/> decoded the <c>GString*</c> itself as UTF-8
	/// instead of the <c>str</c> field inside it, so it returned whatever the
	/// bytes of a heap pointer happen to spell.
	/// </para>
	/// </remarks>
	public class GString : GLib.IWrapper, IDisposable {

		IntPtr handle;
		bool owned;

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_string_free(IntPtr mem, bool free_segments);
		static d_g_string_free g_string_free = FuncLoader.LoadFunction<d_g_string_free>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_string_free"));

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_string_new(IntPtr text);
		static d_g_string_new g_string_new = FuncLoader.LoadFunction<d_g_string_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_string_new"));

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_string_append_len(IntPtr mem, IntPtr val, IntPtr len);
		static d_g_string_append_len g_string_append_len = FuncLoader.LoadFunction<d_g_string_append_len>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_string_append_len"));

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_string_truncate(IntPtr mem, IntPtr len);
		static d_g_string_truncate g_string_truncate = FuncLoader.LoadFunction<d_g_string_truncate>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_string_truncate"));

		/// <summary>An empty buffer, ready to be printed into.</summary>
		public GString () : this (String.Empty) {}

		public GString (string text)
		{
			IntPtr native_text = Marshaller.StringToPtrGStrdup (text);
			handle = g_string_new (native_text);
			Marshaller.Free (native_text);
			owned = true;
		}

		/// <summary>Wraps a <c>GString*</c> that somebody else owns.</summary>
		public GString (IntPtr handle) : this (handle, false) {}

		public GString (IntPtr handle, bool owned)
		{
			this.handle = handle;
			this.owned = owned;
		}

		~GString ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			// The old finalizer freed unconditionally, so a GString that had
			// never been given a handle -- or one belonging to somebody else --
			// went to g_string_free anyway.
			if (handle == IntPtr.Zero)
				return;

			if (owned)
				g_string_free (handle, true);

			handle = IntPtr.Zero;
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		/// <summary>The text currently in the buffer.</summary>
		public string Str {
			get {
				if (handle == IntPtr.Zero)
					return null;

				// struct GString { gchar *str; gsize len; gsize allocated_len; }
				return Marshaller.Utf8PtrToString (Marshal.ReadIntPtr (handle));
			}
		}

		/// <summary>
		/// How many *bytes* of utf-8 the buffer holds, which is not in general
		/// the length of <see cref="Str"/>.
		/// </summary>
		public long Length {
			get {
				if (handle == IntPtr.Zero)
					return 0;

				return Marshal.ReadIntPtr (handle, IntPtr.Size).ToInt64 ();
			}
		}

		public GString Append (string text)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("GString");
			if (text == null)
				return this;

			byte[] bytes = System.Text.Encoding.UTF8.GetBytes (text);
			IntPtr native = Marshal.AllocHGlobal (bytes.Length + 1);
			try {
				Marshal.Copy (bytes, 0, native, bytes.Length);
				Marshal.WriteByte (native, bytes.Length, 0);
				g_string_append_len (handle, native, new IntPtr (bytes.Length));
			} finally {
				Marshal.FreeHGlobal (native);
			}

			return this;
		}

		/// <summary>Empties the buffer without releasing what it allocated.</summary>
		public GString Truncate (long length)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("GString");

			g_string_truncate (handle, new IntPtr (length));
			return this;
		}

		public override string ToString ()
		{
			return Str;
		}

		/// <summary>Reads the text out of a <c>GString*</c>.</summary>
		public static string PtrToString (IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return null;

			return Marshaller.Utf8PtrToString (Marshal.ReadIntPtr (ptr));
		}
	}
}


