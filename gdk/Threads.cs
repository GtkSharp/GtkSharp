// Threads.cs - thread awareness
//
// Author: Alp Toker <alp@atoker.com>
//
// (c) 2002 Alp Toker

namespace Gdk
{
	using System;
	using System.Runtime.InteropServices;

	public class Threads
	{
		[DllImport("gdk-x11-2.0")]
		static extern void gdk_threads_init ();

		public static void Init ()
		{
			gdk_threads_init ();
		}

		[DllImport("gdk-x11-2.0")]
		static extern void gdk_threads_enter ();

		public static void Enter ()
		{
			gdk_threads_enter ();
		}

		[DllImport("gdk-x11-2.0")]
		static extern void gdk_threads_leave ();

		public static void Leave ()
		{
			gdk_threads_leave ();
		}
	}
}
