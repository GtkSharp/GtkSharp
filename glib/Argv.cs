// GLib.Argv.cs : Argv marshaling class
//
// Author: Mike Kestner  <mkestner@novell.com>
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
	
	public class Argv {

		IntPtr[] arg_ptrs;
		IntPtr handle;

		[DllImport("libglib-2.0-0.dll")]
		static extern IntPtr g_strdup (string str);

		[DllImport("libglib-2.0-0.dll")]
		static extern IntPtr g_malloc(IntPtr size);

		[DllImport("libglib-2.0-0.dll")]
		static extern void g_free (IntPtr mem);

		~Argv ()
		{
			foreach (IntPtr arg in arg_ptrs)
				g_free (arg);

			g_free (handle);
		}
				
		public Argv (string[] args) 
		{
			arg_ptrs = new IntPtr [args.Length];

			for (int i = 0; i < args.Length; i++)
				arg_ptrs [i] = g_strdup (args[i]);
				
			handle = g_malloc (new IntPtr (IntPtr.Size * args.Length));

			for (int i = 0; i < args.Length; i++)
				Marshal.WriteIntPtr (handle, i * IntPtr.Size, arg_ptrs [i]);
		}
		
		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public string[] GetArgs (int argc)
		{
			string[] result = new string [argc];

			for (int i = 0; i < argc; i++) 
				result [i] = Marshal.PtrToStringAnsi (Marshal.ReadIntPtr (handle, i * IntPtr.Size));

			return result;
		}
	}
}

