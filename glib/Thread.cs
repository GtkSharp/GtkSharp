// Thread.cs - thread awareness
//
// Author: Alp Toker <alp@atoker.com>
//
// (c) 2002 Alp Toker

namespace GLib
{
	using System;
	using System.Runtime.InteropServices;

	public class Thread
	{
		[DllImport("libgthread-2.0-0.dll")]
		static extern void g_thread_init (IntPtr i);

		public static void Init ()
		{
			g_thread_init (IntPtr.Zero);
		}
	}
}
