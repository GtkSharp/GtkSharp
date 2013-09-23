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

		public new static GLib.GType GType {
			get {
				return GType.Object;
			}
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void g_object_ref_sink (IntPtr raw);

		protected override IntPtr Raw {
			get {
				return base.Raw;
			}
			set {
				if (value != IntPtr.Zero)
					g_object_ref_sink (value);
				base.Raw = value;
			}
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_object_is_floating (IntPtr raw);

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_object_force_floating (IntPtr raw);

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_object_unref (IntPtr raw);

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

