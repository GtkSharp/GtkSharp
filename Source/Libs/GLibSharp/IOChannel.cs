// glib/IOChannel.cs : IOChannel API wrapper
//
// Author: Mike Kestner  <mkestner@novell.com>
//
// Copyright (c) 2007 Novell, Inc.
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


namespace GLibSharp {

	using System;
	using System.Runtime.InteropServices;
	using GLib;

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate bool IOFuncNative(IntPtr source, int condition, IntPtr data);

	internal class IOFuncWrapper {

		IOFunc managed;

		public IOFuncNative NativeDelegate;

		public IOFuncWrapper (IOFunc managed)
		{
			this.managed = managed;
			NativeDelegate = new IOFuncNative (NativeCallback);
		}
		bool NativeCallback (IntPtr source, int condition, IntPtr data)
		{
			try {
				return managed (IOChannel.FromHandle (source), (IOCondition) condition);
			} catch (Exception e) {
				ExceptionManager.RaiseUnhandledException (e, false);
				return false;
			}
		}
	}
}

namespace GLib {

	using System;
	using System.Runtime.InteropServices;
	using GLibSharp;

	public class IOChannel : IDisposable, IWrapper {

		IntPtr handle;

		private IOChannel(IntPtr handle) 
		{
			this.handle = handle;
		}

		public IOChannel (int fd) : this (g_io_channel_unix_new (fd)) {}

		public IOChannel (string filename, string mode) 
		{
			IntPtr native_filename = Marshaller.StringToPtrGStrdup (filename);
			IntPtr native_mode = Marshaller.StringToPtrGStrdup (mode);
			IntPtr error;

			if (Global.IsWindowsPlatform)
				handle = g_io_channel_new_file_utf8(native_filename, native_mode, out error);
			else
				handle = g_io_channel_new_file(native_filename, native_mode, out error);
			Marshaller.Free (native_filename);
			Marshaller.Free (native_mode);
			if (error != IntPtr.Zero) throw new GException (error);
		}

		public IOCondition BufferCondition 
		{ 
			get {
				return (IOCondition) g_io_channel_get_buffer_condition (Handle);
			}
		}

		public bool Buffered 
		{ 
			get {
				return g_io_channel_get_buffered (Handle);
			}
			set {
				g_io_channel_set_buffered (Handle, value);
			}
		}

		public ulong BufferSize 
		{ 
			get {
				return (ulong) g_io_channel_get_buffer_size (Handle);
			}
			set {
				g_io_channel_set_buffer_size (Handle, new UIntPtr (value));
			}
		}

		public bool CloseOnUnref 
		{ 
			get {
				return g_io_channel_get_close_on_unref (Handle);
			}
			set {
				g_io_channel_set_close_on_unref (Handle, value);
			}
		}

		public string Encoding 
		{ 
			get {
				return Marshaller.Utf8PtrToString (g_io_channel_get_encoding (Handle));
			}
			set {
				IntPtr native_encoding = Marshaller.StringToPtrGStrdup (value);
				IntPtr error;
				g_io_channel_set_encoding (Handle, native_encoding, out error);
				Marshaller.Free (native_encoding);
				if (error != IntPtr.Zero) throw new GException (error);
			}
		}

		public IOFlags Flags 
		{ 
			get {
				return (IOFlags) g_io_channel_get_flags(Handle);
			}
			set {
				IntPtr error;
				g_io_channel_set_flags(Handle, (int) value, out error);
				if (error != IntPtr.Zero) throw new GException (error);
			}
		}

		public char[] LineTerminator {
			get {
				int length;
				IntPtr raw = g_io_channel_get_line_term (Handle, out length);
				if (length == -1)
					return Marshaller.Utf8PtrToString (raw).ToCharArray ();
				byte[] buffer = new byte [length];
				return System.Text.Encoding.UTF8.GetChars (buffer);
			}
			set {
				byte[] buffer = System.Text.Encoding.UTF8.GetBytes (value);
				g_io_channel_set_line_term (Handle, buffer, buffer.Length);
			}
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public int UnixFd {
			get {
				return g_io_channel_unix_get_fd (Handle);
			}
		}

		protected void Init () 
		{
			g_io_channel_init (Handle);
		}

		public void Dispose ()
		{
			g_io_channel_unref (Handle);
		}

		public uint AddWatch (int priority, IOCondition condition, IOFunc func) 
		{
			IOFuncWrapper func_wrapper = null;
			IntPtr user_data = IntPtr.Zero;
			DestroyNotify notify = null;
			if (func != null) {
				func_wrapper = new IOFuncWrapper (func);
				user_data = (IntPtr) GCHandle.Alloc (func_wrapper);
				notify = DestroyHelper.NotifyHandler;
			}
			return g_io_add_watch_full (Handle, priority, (int) condition, func_wrapper.NativeDelegate, user_data, notify);
		}

		public IOStatus Flush () 
		{
			IntPtr error;
			IOStatus ret = (IOStatus) g_io_channel_flush (Handle, out error);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public IOStatus ReadChars (byte[] buf, out ulong bytes_read) 
		{
			UIntPtr native_bytes_read;
			IntPtr error;
			IOStatus ret = (IOStatus) g_io_channel_read_chars (Handle, buf, new UIntPtr ((ulong) buf.Length), out native_bytes_read, out error);
			bytes_read = (ulong) native_bytes_read;
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public IOStatus ReadLine (out string str_return)
		{
			ulong dump;
			return ReadLine (out str_return, out dump);
		}

		public IOStatus ReadLine (out string str_return, out ulong terminator_pos) 
		{
			IntPtr native_string;
			UIntPtr native_terminator_pos;
			IntPtr error;
			IOStatus ret = (IOStatus) g_io_channel_read_line (Handle, out native_string, IntPtr.Zero, out native_terminator_pos, out error);
			terminator_pos = (ulong) native_terminator_pos;
			str_return = null;
			if (ret == IOStatus.Normal)
				str_return = Marshaller.PtrToStringGFree (native_string);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public IOStatus ReadToEnd (out string str_return) 
		{
			IntPtr native_str;
			UIntPtr native_length;
			IntPtr error;
			IOStatus ret = (IOStatus) g_io_channel_read_to_end (Handle, out native_str, out native_length, out error);
			str_return = null;
			if (ret == IOStatus.Normal) {
				byte[] buffer = new byte [(ulong) native_length];
				Marshal.Copy (native_str, buffer, 0, (int)(ulong) native_length);
				str_return = System.Text.Encoding.UTF8.GetString (buffer);
			}
			Marshaller.Free (native_str);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public IOStatus ReadUnichar (out uint thechar) 
		{
			IntPtr error;
			IOStatus ret = (IOStatus) g_io_channel_read_unichar (Handle, out thechar, out error);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public IOStatus SeekPosition (long offset, SeekType type) 
		{
			IntPtr error;
			IOStatus ret = (IOStatus) g_io_channel_seek_position (Handle, offset, (int) type, out error);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public IOStatus Shutdown (bool flush) 
		{
			IntPtr error;
			IOStatus ret = (IOStatus) g_io_channel_shutdown (Handle, flush, out error);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public IOStatus WriteChars (string str, out string remainder) 
		{
			ulong written;
			System.Text.Encoding enc = System.Text.Encoding.UTF8;
			byte[] buffer = enc.GetBytes (str);
			IOStatus ret = WriteChars (buffer, out written);
			remainder = null;
			if ((int) written == buffer.Length)
				return ret;
			int count = buffer.Length - (int) written;
			byte[] rem = new byte [count];
			Array.Copy (buffer, (int) written, rem, 0, count);
			remainder = enc.GetString (rem);
			return ret;
		}

		public IOStatus WriteChars (byte[] buf, out ulong bytes_written) 
		{
			UIntPtr native_bytes_written;
			IntPtr error;
			IOStatus ret = (IOStatus) g_io_channel_write_chars (Handle, buf, new IntPtr (buf.Length), out native_bytes_written, out error);
			bytes_written = (ulong) native_bytes_written;
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public IOStatus WriteUnichar (uint thechar) 
		{
			IntPtr error;
			IOStatus ret = (IOStatus) g_io_channel_write_unichar (Handle, thechar, out error);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public static IOChannel FromHandle (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return null;

			g_io_channel_ref (handle);
			return new IOChannel (handle);
		}

		public static IOChannelError ErrorFromErrno (int en) 
		{
			return (IOChannelError) g_io_channel_error_from_errno (en);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_io_channel_unix_new(int fd);
		static d_g_io_channel_unix_new g_io_channel_unix_new = FuncLoader.LoadFunction<d_g_io_channel_unix_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_unix_new"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_io_channel_new_file(IntPtr filename, IntPtr mode, out IntPtr error);
		static d_g_io_channel_new_file g_io_channel_new_file = FuncLoader.LoadFunction<d_g_io_channel_new_file>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_new_file"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_io_channel_new_file_utf8(IntPtr filename, IntPtr mode, out IntPtr error);
		static d_g_io_channel_new_file_utf8 g_io_channel_new_file_utf8 = FuncLoader.LoadFunction<d_g_io_channel_new_file_utf8>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_new_file_utf8"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_error_quark();
		static d_g_io_channel_error_quark g_io_channel_error_quark = FuncLoader.LoadFunction<d_g_io_channel_error_quark>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_error_quark"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_error_from_errno(int en);
		static d_g_io_channel_error_from_errno g_io_channel_error_from_errno = FuncLoader.LoadFunction<d_g_io_channel_error_from_errno>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_error_from_errno"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_flush(IntPtr raw, out IntPtr error);
		static d_g_io_channel_flush g_io_channel_flush = FuncLoader.LoadFunction<d_g_io_channel_flush>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_flush"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_io_channel_init(IntPtr raw);
		static d_g_io_channel_init g_io_channel_init = FuncLoader.LoadFunction<d_g_io_channel_init>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_init"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_read_chars(IntPtr raw, byte[] buf, UIntPtr count, out UIntPtr bytes_read, out IntPtr error);
		static d_g_io_channel_read_chars g_io_channel_read_chars = FuncLoader.LoadFunction<d_g_io_channel_read_chars>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_read_chars"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_read_line(IntPtr raw, out IntPtr str_return, IntPtr length, out UIntPtr terminator_pos, out IntPtr error);
		static d_g_io_channel_read_line g_io_channel_read_line = FuncLoader.LoadFunction<d_g_io_channel_read_line>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_read_line"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_read_to_end(IntPtr raw, out IntPtr str_return, out UIntPtr length, out IntPtr error);
		static d_g_io_channel_read_to_end g_io_channel_read_to_end = FuncLoader.LoadFunction<d_g_io_channel_read_to_end>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_read_to_end"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_read_unichar(IntPtr raw, out uint thechar, out IntPtr error);
		static d_g_io_channel_read_unichar g_io_channel_read_unichar = FuncLoader.LoadFunction<d_g_io_channel_read_unichar>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_read_unichar"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_seek_position(IntPtr raw, long offset, int type, out IntPtr error);
		static d_g_io_channel_seek_position g_io_channel_seek_position = FuncLoader.LoadFunction<d_g_io_channel_seek_position>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_seek_position"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_shutdown(IntPtr raw, bool flush, out IntPtr err);
		static d_g_io_channel_shutdown g_io_channel_shutdown = FuncLoader.LoadFunction<d_g_io_channel_shutdown>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_shutdown"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_write_chars(IntPtr raw, byte[] buf, IntPtr count, out UIntPtr bytes_written, out IntPtr error);
		static d_g_io_channel_write_chars g_io_channel_write_chars = FuncLoader.LoadFunction<d_g_io_channel_write_chars>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_write_chars"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_write_unichar(IntPtr raw, uint thechar, out IntPtr error);
		static d_g_io_channel_write_unichar g_io_channel_write_unichar = FuncLoader.LoadFunction<d_g_io_channel_write_unichar>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_write_unichar"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_get_buffer_condition(IntPtr raw);
		static d_g_io_channel_get_buffer_condition g_io_channel_get_buffer_condition = FuncLoader.LoadFunction<d_g_io_channel_get_buffer_condition>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_get_buffer_condition"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_io_channel_get_buffered(IntPtr raw);
		static d_g_io_channel_get_buffered g_io_channel_get_buffered = FuncLoader.LoadFunction<d_g_io_channel_get_buffered>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_get_buffered"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_io_channel_set_buffered(IntPtr raw, bool buffered);
		static d_g_io_channel_set_buffered g_io_channel_set_buffered = FuncLoader.LoadFunction<d_g_io_channel_set_buffered>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_set_buffered"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate UIntPtr d_g_io_channel_get_buffer_size(IntPtr raw);
		static d_g_io_channel_get_buffer_size g_io_channel_get_buffer_size = FuncLoader.LoadFunction<d_g_io_channel_get_buffer_size>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_get_buffer_size"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_io_channel_set_buffer_size(IntPtr raw, UIntPtr size);
		static d_g_io_channel_set_buffer_size g_io_channel_set_buffer_size = FuncLoader.LoadFunction<d_g_io_channel_set_buffer_size>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_set_buffer_size"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_io_channel_get_close_on_unref(IntPtr raw);
		static d_g_io_channel_get_close_on_unref g_io_channel_get_close_on_unref = FuncLoader.LoadFunction<d_g_io_channel_get_close_on_unref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_get_close_on_unref"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_io_channel_set_close_on_unref(IntPtr raw, bool do_close);
		static d_g_io_channel_set_close_on_unref g_io_channel_set_close_on_unref = FuncLoader.LoadFunction<d_g_io_channel_set_close_on_unref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_set_close_on_unref"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_io_channel_get_encoding(IntPtr raw);
		static d_g_io_channel_get_encoding g_io_channel_get_encoding = FuncLoader.LoadFunction<d_g_io_channel_get_encoding>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_get_encoding"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_set_encoding(IntPtr raw, IntPtr encoding, out IntPtr error);
		static d_g_io_channel_set_encoding g_io_channel_set_encoding = FuncLoader.LoadFunction<d_g_io_channel_set_encoding>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_set_encoding"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_get_flags(IntPtr raw);
		static d_g_io_channel_get_flags g_io_channel_get_flags = FuncLoader.LoadFunction<d_g_io_channel_get_flags>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_get_flags"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_set_flags(IntPtr raw, int flags, out IntPtr error);
		static d_g_io_channel_set_flags g_io_channel_set_flags = FuncLoader.LoadFunction<d_g_io_channel_set_flags>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_set_flags"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_io_channel_get_line_term(IntPtr raw, out int length);
		static d_g_io_channel_get_line_term g_io_channel_get_line_term = FuncLoader.LoadFunction<d_g_io_channel_get_line_term>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_get_line_term"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_io_channel_set_line_term(IntPtr raw, byte[] term, int length);
		static d_g_io_channel_set_line_term g_io_channel_set_line_term = FuncLoader.LoadFunction<d_g_io_channel_set_line_term>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_set_line_term"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_io_channel_unix_get_fd(IntPtr raw);
		static d_g_io_channel_unix_get_fd g_io_channel_unix_get_fd = FuncLoader.LoadFunction<d_g_io_channel_unix_get_fd>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_unix_get_fd"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_io_channel_ref(IntPtr raw);
		static d_g_io_channel_ref g_io_channel_ref = FuncLoader.LoadFunction<d_g_io_channel_ref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_ref"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_io_channel_unref(IntPtr raw);
		static d_g_io_channel_unref g_io_channel_unref = FuncLoader.LoadFunction<d_g_io_channel_unref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_channel_unref"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_io_add_watch_full(IntPtr raw, int priority, int condition, IOFuncNative func, IntPtr user_data, DestroyNotify notify);
		static d_g_io_add_watch_full g_io_add_watch_full = FuncLoader.LoadFunction<d_g_io_add_watch_full>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_add_watch_full"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_io_create_watch(IntPtr raw, int condition);
		static d_g_io_create_watch g_io_create_watch = FuncLoader.LoadFunction<d_g_io_create_watch>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_io_create_watch"));
	}

	public delegate bool IOFunc (IOChannel source, IOCondition condition);

	public enum IOChannelError {
		FileTooBig,
		Inval,
		IO,
		IsDir,
		NoSpace,
		Nxio,
		Overflow,
		Pipe,
		Failed,
	}

	[Flags]
	public enum IOCondition {
		In = 1 << 0,
		Out = 1 << 2,
		Pri = 1 << 1,
		Err = 1 << 3,
		Hup = 1 << 4,
		Nval = 1 << 5,
	}

	[Flags]
	public enum IOFlags {
		Append = 1 << 0,
		Nonblock = 1 << 1,
		IsReadable = 1 << 2,
		IsWriteable = 1 << 3,
		IsSeekable = 1 << 4,
		Mask = 1 << 5- 1,
		GetMask = Mask,
		SetMask = Append | Nonblock,
	}

	public enum IOStatus {
		Error,
		Normal,
		Eof,
		Again,
	}

	public enum SeekType {
		Cur,
		Set,
		End,
	}
}

