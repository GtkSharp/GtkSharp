// GException.cs : GError handling
//
// Authors: Rachel Hestilow  <hestilow@ximian.com>
//
// (c) 2002 Rachel Hestilow 

namespace GLib {

	using System;
	using System.Runtime.InteropServices;
	
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct GError
	{
		[MarshalAs (UnmanagedType.U4)]
		public uint domain;
		[MarshalAs (UnmanagedType.I4)]
		public int code;
		[MarshalAs (UnmanagedType.LPStr)]
		public string message;
	}

	public unsafe class GException : Exception
	{
		GError *errptr;
	
		unsafe public GException (GError *errptr) : base (errptr->message)
		{
			this.errptr = errptr;
		}

		[DllImport("glib-2.0")]
		unsafe static extern void g_clear_error (GError **errptr);
		~GException ()
		{
			unsafe { g_clear_error (&errptr); }
		}
	}
}

