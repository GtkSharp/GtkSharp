// GLib.Idle.cs - Idle class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//         Rachel Hestilow <hestilow@ximian.com>
//
// (c) 2002 Mike Kestner, Rachel Hestilow

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

		[DllImport("libglib-2.0-0.dll")]
		static extern uint g_idle_add (IdleHandler d, IntPtr data);

		public static uint Add (IdleHandler hndlr)
		{
			return g_idle_add (hndlr, IntPtr.Zero);
		}
	}
}

