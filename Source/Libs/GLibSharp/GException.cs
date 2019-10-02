// GException.cs : GError handling
//
// Authors: Rachel Hestilow  <hestilow@ximian.com>
//
// Copyright (c) 2002 Rachel Hestilow 
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
	
	public class GException : Exception
	{
		IntPtr errptr;
	
		public GException (IntPtr errptr) : base ()
		{
			this.errptr = errptr;
		}

		struct GError {
			public int Domain;
			public int Code;
			public IntPtr Msg;
		}

		public int Code {
			get {
				GError err = (GError) Marshal.PtrToStructure (errptr, typeof (GError));
				return err.Code;
			}
		}

		public int Domain {
			get {
				GError err = (GError) Marshal.PtrToStructure (errptr, typeof (GError));
				return err.Domain;
			}
		}

		public override string Message {
			get {
				GError err = (GError) Marshal.PtrToStructure (errptr, typeof (GError));
				return Marshaller.Utf8PtrToString (err.Msg);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_clear_error(ref IntPtr errptr);
		static d_g_clear_error g_clear_error = FuncLoader.LoadFunction<d_g_clear_error>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_clear_error"));
		~GException ()
		{
			g_clear_error (ref errptr);
		}
	}
}


