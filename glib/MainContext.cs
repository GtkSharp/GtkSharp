// GLib.MainContext.cs - mainContext class implementation
//
// Author: Radek Doulik <rodo@matfyz.cz>
//
// (C) 2003 Radek Doulik

namespace GLib {

	using System;
	using System.Runtime.InteropServices;

        public class MainContext {
		
		[DllImport("glib-2.0")]
		static extern bool g_main_context_iteration (IntPtr Raw, bool MayBlock);

		public static bool Iteration ()
		{
			return g_main_context_iteration (IntPtr.Zero, false);
		}

		public static bool Iteration (bool MayBlock)
		{
			return g_main_context_iteration (IntPtr.Zero, MayBlock);
		}

		[DllImport("glib-2.0")]
		static extern bool g_main_context_pending (IntPtr Raw);
		
		public static bool Pending ()
		{
			return g_main_context_pending (IntPtr.Zero);
		}
	}
}
