//
// Application.cs - Gst initialization
//
// Author: Alp Toker <alp@atoker.com>
//
// 2002 (C) Copyright, Alp Toker
//


namespace Gst {

	using System;
	using System.Runtime.InteropServices;

	public class Application {

		[DllImport("gstreamer-0.4.1")]
		static extern void gst_init (int argc, IntPtr argv);

		public static void Init ()
		{
			gst_init (0, new IntPtr(0));
		}

		[DllImport("gstreamer-0.4.1")]
		static extern void gst_init (ref int argc, ref String[] argv);

		public static void Init (ref string[] args)
		{
			int argc = args.Length;
			gst_init (ref argc, ref args);
		}

	}
}
