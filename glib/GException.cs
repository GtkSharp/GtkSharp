// GException.cs : GError handling
//
// Authors: Rachel Hestilow  <hestilow@ximian.com>
//
// (c) 2002 Rachel Hestilow 

namespace GLib {

	using System;
	using System.Runtime.InteropServices;
	
	public class GException : Exception
	{
		IntPtr errptr;
	
		public GException (IntPtr errptr) : base ()
		{
			this.errptr = errptr;
		}

		[DllImport("gtksharpglue")]
		static extern string gtksharp_error_get_message (IntPtr errptr);
		public override string Message {
			get {
				return gtksharp_error_get_message (errptr);
			}
		}

		[DllImport("glib-2.0")]
		static extern void g_clear_error (ref IntPtr errptr);
		~GException ()
		{
			g_clear_error (ref errptr);
		}
	}
}

