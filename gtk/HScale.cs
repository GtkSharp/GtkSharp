// Gtk.HScale.cs - Gtk HScale class customizations
//
// Author: Mike Kestner <mkestner@ximian.com> 
//
// Copyright (C) 2004 Novell, Inc.
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

	public partial class HScale {

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_hscale_new_with_range (double min, double max, double step);

		public HScale (double min, double max, double step) : base (IntPtr.Zero)
		{
			if (GetType() != typeof (HScale)) {
				Adjustment adj = new Adjustment (min, min, max, step, 10 * step, 0);
				string[] names = new string [1];
				GLib.Value[] vals = new GLib.Value [1];
				names [0] = "adjustment";
				vals [0] = new GLib.Value (adj);
				CreateNativeObject (names, vals);
				vals [0].Dispose ();
				return;
			}

			Raw = gtk_hscale_new_with_range (min, max, step);
		}
	}
}
