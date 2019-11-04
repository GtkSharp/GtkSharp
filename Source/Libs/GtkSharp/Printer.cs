// Printer.cs - customizations to Gtk.Printer
//
// Authors: Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2006 Novell, Inc.
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

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	public partial class Printer {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_enumerate_printers(GtkSharp.PrinterFuncNative func, IntPtr func_data, GLib.DestroyNotify destroy, bool wait);
		static d_gtk_enumerate_printers gtk_enumerate_printers = FuncLoader.LoadFunction<d_gtk_enumerate_printers>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_enumerate_printers"));

		public static void EnumeratePrinters (Gtk.PrinterFunc func, bool wait) 
		{
			GtkSharp.PrinterFuncWrapper func_wrapper;
			IntPtr func_data;
			GLib.DestroyNotify destroy;
			if (func == null) {
				func_wrapper = null;
				func_data = IntPtr.Zero;
				destroy = null;
			} else {
				func_wrapper = new GtkSharp.PrinterFuncWrapper (func);
				func_data = (IntPtr) GCHandle.Alloc (func_wrapper);
				destroy = GLib.DestroyHelper.NotifyHandler;
			}
			gtk_enumerate_printers (func_wrapper.NativeDelegate, func_data, destroy, wait);
		}
	}
}

