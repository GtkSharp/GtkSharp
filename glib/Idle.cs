// GLib.Idle.cs - Idle class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//         Rachel Hestilow <hestilow@ximian.com>
//
// Copyright (c) 2002 Mike Kestner
// Copyright (c) Rachel Hestilow
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
	///	IdleHandler Delegate
	/// </summary>
	///
	/// <remarks>
	///	Delegate used for idle handlerss in the GLib main loop. Return
	///	true to restart the idle.  Returning false clears the
	///	idle.
	/// </remarks>

	public delegate bool IdleHandler ();

	/// <summary>
	///	Idle Class
	/// </summary>
	///
	/// <remarks>
	///	Allows the installation of Idle Handlers on the GLib main
	///	loop.
	/// </remarks>

	public class Idle {

		private Idle ()
		{
		}
		
		[DllImport("libglib-2.0-0.dll")]
		static extern uint g_idle_add (IdleHandler d, IntPtr data);

		public static uint Add (IdleHandler hndlr)
		{
			return g_idle_add (hndlr, IntPtr.Zero);
		}
		
		[DllImport("libglib-2.0-0.dll")]
		static extern bool g_source_remove_by_funcs_user_data (IdleHandler d, IntPtr data);
                                                                                
		public static bool Remove (IdleHandler hndlr)
		{
			return g_source_remove_by_funcs_user_data (hndlr, IntPtr.Zero);
		}

	}
}

