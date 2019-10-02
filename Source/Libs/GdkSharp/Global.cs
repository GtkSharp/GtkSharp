// Global.cs - customizations to Gdk.Global
//
// Authors: Mike Kestner  <mkestner@ximian.com>
//          Boyd Timothy  <btimothy@novell.com>
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

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public partial class Global {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_list_visuals();
		static d_gdk_list_visuals gdk_list_visuals = FuncLoader.LoadFunction<d_gdk_list_visuals>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_list_visuals"));

		public static Visual[] ListVisuals ()
		{
			IntPtr raw_ret = gdk_list_visuals ();
			if (raw_ret == IntPtr.Zero)
				return new Visual [0];
			GLib.List list = new GLib.List(raw_ret);
			Visual[] result = new Visual [list.Count];
			for (int i = 0; i < list.Count; i++)
				result [i] = list [i] as Visual;
			return result;
		}

		public static Gdk.Atom[] SupportedWindowManagerHints {
			get {
				Gdk.Atom[] atoms;
				if (!Gdk.Property.Get (Screen.Default.RootWindow, Atom.Intern ("_NET_SUPPORTED", false), false, out atoms))
					throw new ApplicationException ("Unable to get _NET_SUPPORTED property");

				return atoms;
			}
		}

#if FIXME30
		public static Gdk.Window[] WindowManagerClientWindows {
			get {
				Gdk.Window [] windows;
				if (!Gdk.Property.Get (Screen.Default.RootWindow, Atom.Intern ("_NET_CLIENT_LIST", false), false, out windows))
					throw new ApplicationException ("Unable to get _NET_CLIENT_LIST property");

				return windows;
			}
		}
#endif

		public static int NumberOfDesktops {
			get {
				int[] data;
				if (!Gdk.Property.Get (Screen.Default.RootWindow, Atom.Intern ("_NET_NUMBER_OF_DESKTOPS", false), false, out data))
					throw new ApplicationException ("Unable to get _NET_NUMBER_OF_DESKTOPS property");

				return data [0];
			}
		}

		public static int CurrentDesktop {
			get {
				int[] data;
				if (!Gdk.Property.Get (Screen.Default.RootWindow, Atom.Intern ("_NET_CURRENT_DESKTOP", false), false, out data))
					throw new ApplicationException ("Unable to get _NET_CURRENT_DESKTOP property");

				return data [0];
			}
		}

#if FIXME30
		public static Gdk.Window ActiveWindow {
			get {
				Gdk.Window [] windows;
				if (!Gdk.Property.Get (Screen.Default.RootWindow, Atom.Intern ("_NET_ACTIVE_WINDOW", false), false, out windows))
					throw new ApplicationException ("Unable to get _NET_ACTIVE_WINDOW property");

				return windows [0];
			}
		}
#endif

		public static Gdk.Rectangle[] DesktopWorkareas {
			get {
				Gdk.Rectangle[] workareas;
				if (!Gdk.Property.Get (Screen.Default.RootWindow, Atom.Intern ("_NET_WORKAREA", false), false, out workareas))
					throw new ApplicationException ("Unable to get _NET_WORKAREA property");

				return workareas;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gdk_init_check(ref int argc, ref IntPtr argv);
		static d_gdk_init_check gdk_init_check = FuncLoader.LoadFunction<d_gdk_init_check>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_init_check"));

		public static bool InitCheck (ref string[] argv)
		{
			GLib.Argv a = new GLib.Argv (argv, true);
			IntPtr buf = a.Handle;
			int argc = argv.Length + 1;

			bool result = gdk_init_check (ref argc, ref buf);
			argv = a.GetArgs (argc);
			return result;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_parse_args(ref int argc, ref IntPtr argv);
		static d_gdk_parse_args gdk_parse_args = FuncLoader.LoadFunction<d_gdk_parse_args>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_parse_args"));

		public static void ParseArgs (ref string[] argv)
		{
			GLib.Argv a = new GLib.Argv (argv, true);
			IntPtr buf = a.Handle;
			int argc = argv.Length + 1;

			gdk_parse_args (ref argc, ref buf);
			argv = a.GetArgs (argc);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_query_depths(out IntPtr depths, out int n_depths);
		static d_gdk_query_depths gdk_query_depths = FuncLoader.LoadFunction<d_gdk_query_depths>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_query_depths"));

		public static int[] QueryDepths ()
		{
			IntPtr ptr;
			int count;
			gdk_query_depths (out ptr, out count);
			int[] result = new int [count];
			Marshal.Copy (ptr, result, 0, count);
			return result;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_query_visual_types(out IntPtr types, out int n_types);
		static d_gdk_query_visual_types gdk_query_visual_types = FuncLoader.LoadFunction<d_gdk_query_visual_types>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_query_visual_types"));

		public static VisualType[] QueryVisualTypes ()
		{
			IntPtr ptr;
			int count;
			gdk_query_visual_types (out ptr, out count);
			int[] tmp = new int [count];
			Marshal.Copy (ptr, tmp, 0, count);
			VisualType[] result = new VisualType [count];
			for (int i = 0; i < count; i++)
				result [i] = (VisualType) tmp [i];
			return result;
		}
	}
}


