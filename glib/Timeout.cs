// GLib.Timeout.cs - Timeout class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2002 Mike Kestner

namespace GLib {

	using System;
	using System.Runtime.InteropServices;

	/// <summary>
	///	TimeoutHandler Delegate
	/// </summary>
	///
	/// <remarks>
	///	Delegate used for Timeouts in the GLib main loop. Return
	///	true to restart the timeout.  Returning false clears the
	///	timeout.
	/// </remarks>

	public delegate bool TimeoutHandler ();

	/// <summary>
	///	Timeout Class
	/// </summary>
	///
	/// <remarks>
	///	Allows the installation of Timeout Handlers on the GLib main
	///	loop.
	/// </remarks>

	public class Timeout {

		[DllImport("glib-2.0")]
		static extern uint g_timeout_add (uint interval, TimeoutHandler d, IntPtr data);

		public static uint Add (uint interval, TimeoutHandler hndlr)
		{
			return g_timeout_add (interval, hndlr, IntPtr.Zero);
		}
	}
}

