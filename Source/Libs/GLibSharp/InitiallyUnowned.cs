// InitiallyUnowned.cs - GInitiallyUnowned class wrapper implementation
//
// Authors: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2004-2005 Novell, Inc.
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

	public class InitiallyUnowned : Object {

		protected InitiallyUnowned (IntPtr raw) : base (raw) {}

		delegate IntPtr d_g_initially_unowned_get_type ();
		static d_g_initially_unowned_get_type g_initially_unowned_get_type = FuncLoader.LoadFunction<d_g_initially_unowned_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_initially_unowned_get_type"));

		public new static GLib.GType GType {
			get {
				IntPtr raw_ret = g_initially_unowned_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_object_ref_sink(IntPtr raw);
		static d_g_object_ref_sink g_object_ref_sink = FuncLoader.LoadFunction<d_g_object_ref_sink>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_ref_sink"));

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_object_is_floating(IntPtr raw);
		static d_g_object_is_floating g_object_is_floating = FuncLoader.LoadFunction<d_g_object_is_floating>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_is_floating"));

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_object_force_floating(IntPtr raw);
		static d_g_object_force_floating g_object_force_floating = FuncLoader.LoadFunction<d_g_object_force_floating>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_force_floating"));

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_object_unref(IntPtr raw);
		static d_g_object_unref g_object_unref = FuncLoader.LoadFunction<d_g_object_unref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_unref"));

		public bool IsFloating {
			get {
				return g_object_is_floating (Handle);
			}
			set {
			  	if (value == true) {
					if (!IsFloating)
						g_object_force_floating (Handle);
				} else {
					g_object_ref_sink (Handle);
					g_object_unref (Handle);
				}
			}
		}
	}
}

