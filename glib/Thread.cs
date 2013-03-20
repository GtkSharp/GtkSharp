// Thread.cs - thread awareness
//
// Author: Alp Toker <alp@atoker.com>
//
// Copyright (c) 2002 Alp Toker
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


namespace GLib
{
	using System;
	using System.Runtime.InteropServices;

	public class Thread
	{
		private Thread () {}
		
#if ENABLE_GTHREAD_INIT
		[DllImport ("libgthread-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_thread_init (IntPtr i);

		public static void Init ()
		{
			g_thread_init (IntPtr.Zero);
		}

		[DllImport ("libgthread-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_thread_get_initialized ();

		public static bool Supported
		{
			get {
				return g_thread_get_initialized ();
			}
		}
#else
		public static void Init ()
		{
			// GLib automatically inits threads in 2.31 and above
			// http://developer.gnome.org/glib/unstable/glib-Deprecated-Thread-APIs.html#g-thread-init
		}

		public static bool Supported
		{
			get { return true; }
		}
#endif

	}
}
